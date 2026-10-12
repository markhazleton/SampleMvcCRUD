#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
#
# The only place in DevSpark that talks to Azure DevOps Boards. Every operation emits exactly
# one canonical JSON line and exits 0 even when the operation failed, so callers parse a result
# instead of interpreting an exit code. A non-zero exit means the script was invoked wrongly.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('check', 'read', 'propose-update', 'apply-update', 'comment',
                 'propose-create', 'apply-create', 'verify-link')]
    [string]$Operation,

    [string]$Id = '',
    [string]$Url = '',
    [string[]]$Field = @(),
    [string[]]$Value = @(),
    [string]$Type = '',
    [string]$Title = '',
    [string]$DescriptionHtml = '',
    [string]$AcceptanceCriteriaHtml = '',
    [string[]]$Reference = @(),
    [string]$Text = '',
    [string]$Organization = '',
    [string]$Project = '',
    [string]$AreaPath = '',
    [int]$Rev = -1,
    [switch]$Confirmed,
    [string]$Fixture = '',
    [switch]$Json
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

. "$PSScriptRoot/platform.ps1"
. "$PSScriptRoot/common.ps1"

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

$script:ContractVersion = 1

# Retrieved free text is capped in characters rather than bytes. The consumer limit is a text
# budget, and counting characters cannot split a multi-byte character the way a byte cut can.
$script:MaxFieldChars = 8000

$script:UntrustedOpenMarker = '<<<UNTRUSTED-WORKITEM-DATA'
$script:UntrustedCloseMarker = 'UNTRUSTED-WORKITEM-DATA'
$script:UntrustedNeutralised = 'UNTRUSTED_WORKITEM_DATA_NEUTRALIZED'

# Applied to every created item so a reader of the board can tell where it came from.
$script:AgentTag = 'devspark-generated'

$script:FieldMap = [ordered]@{
    title               = 'System.Title'
    state               = 'System.State'
    area_path           = 'System.AreaPath'
    iteration_path      = 'System.IterationPath'
    tags                = 'System.Tags'
    description         = 'System.Description'
    acceptance_criteria = 'Microsoft.VSTS.Common.AcceptanceCriteria'
    story_points        = 'Microsoft.VSTS.Scheduling.StoryPoints'
    assigned_to         = 'System.AssignedTo'
}

$script:Warnings = [System.Collections.Generic.List[string]]::new()

function Add-WorkItemWarning {
    param([string]$Code)
    if (-not $script:Warnings.Contains($Code)) { $script:Warnings.Add($Code) }
}

# ---------------------------------------------------------------------------
# Canonical JSON emitter
#
# Output must be byte-identical to the Bash twin, so nothing here relies on a serializer's
# default key ordering or escaping. Every object is built from an explicit ordered key list.
# ---------------------------------------------------------------------------

function ConvertTo-JsonStringLiteral {
    param([AllowNull()][string]$Value)
    # An absent value is null. The contract admits no empty string, so the distinction between
    # "not set" and "set to nothing" is collapsed here rather than at every call site.
    if ([string]::IsNullOrEmpty($Value)) { return 'null' }
    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.Append('"')
    foreach ($ch in $Value.ToCharArray()) {
        $code = [int]$ch
        if ($ch -eq '"') { [void]$sb.Append('\"') }
        elseif ($ch -eq '\') { [void]$sb.Append('\\') }
        elseif ($code -eq 8) { [void]$sb.Append('\b') }
        elseif ($code -eq 12) { [void]$sb.Append('\f') }
        elseif ($code -eq 10) { [void]$sb.Append('\n') }
        elseif ($code -eq 13) { [void]$sb.Append('\r') }
        elseif ($code -eq 9) { [void]$sb.Append('\t') }
        elseif ($code -lt 32 -or $code -eq 127) { [void]$sb.Append(('\u{0:x4}' -f $code)) }
        else { [void]$sb.Append($ch) }
    }
    [void]$sb.Append('"')
    return $sb.ToString()
}

function ConvertTo-JsonScalar {
    param([AllowNull()]$Value)
    if ($null -eq $Value) { return 'null' }
    if ($Value -is [bool]) { if ($Value) { return 'true' } else { return 'false' } }
    if ($Value -is [int] -or $Value -is [long]) { return "$Value" }
    if ($Value -is [double] -or $Value -is [decimal]) {
        return [string]::Format([cultureinfo]::InvariantCulture, '{0}', $Value)
    }
    return (ConvertTo-JsonStringLiteral ([string]$Value))
}

function ConvertTo-JsonArray {
    param([AllowNull()][object[]]$Items)
    if ($null -eq $Items -or $Items.Count -eq 0) { return '[]' }
    $parts = foreach ($i in $Items) { ConvertTo-JsonScalar $i }
    return '[' + ($parts -join ', ') + ']'
}

# Build an object from an ordered map whose values are already-encoded JSON fragments.
function ConvertTo-JsonObjectFromFragments {
    param([System.Collections.Specialized.OrderedDictionary]$Fragments)
    if ($Fragments.Count -eq 0) { return '{}' }
    $parts = foreach ($key in $Fragments.Keys) {
        (ConvertTo-JsonStringLiteral ([string]$key)) + ': ' + $Fragments[$key]
    }
    return '{' + ($parts -join ', ') + '}'
}

# Build an object from an ordered map of plain scalar values.
function ConvertTo-JsonObject {
    param([System.Collections.Specialized.OrderedDictionary]$Map)
    $frag = [ordered]@{}
    foreach ($key in $Map.Keys) { $frag[$key] = ConvertTo-JsonScalar $Map[$key] }
    return ConvertTo-JsonObjectFromFragments $frag
}

# ---------------------------------------------------------------------------
# Result emission
# ---------------------------------------------------------------------------

function Write-Result {
    param(
        [string]$DataFragment = '{}',
        [System.Collections.Specialized.OrderedDictionary]$ErrorMap = $null
    )
    $envelope = [ordered]@{}
    $envelope['contract'] = ConvertTo-JsonScalar $script:ContractVersion
    $envelope['operation'] = ConvertTo-JsonStringLiteral $Operation
    $envelope['ok'] = if ($null -eq $ErrorMap) { 'true' } else { 'false' }
    $envelope['error'] = if ($null -eq $ErrorMap) { 'null' } else { ConvertTo-JsonObject $ErrorMap }
    $envelope['warnings'] = ConvertTo-JsonArray ($script:Warnings.ToArray())
    $envelope['data'] = $DataFragment

    $line = ConvertTo-JsonObjectFromFragments $envelope

    # Written straight to the standard output handle as UTF-8 without a BOM, bypassing the
    # host's console encoding. On a machine whose console code page is not UTF-8 -- the
    # default on Windows -- letting PowerShell encode this would silently replace every
    # non-ASCII character, and the twin helper would stop producing identical bytes.
    $stream = [Console]::OpenStandardOutput()
    $writer = New-Object System.IO.StreamWriter($stream, (New-Object System.Text.UTF8Encoding($false)))
    try {
        $writer.Write($line)
        $writer.Write("`n")
        $writer.Flush()
    } finally {
        $writer.Dispose()
    }
    exit 0
}

# Every error names what went wrong and what the caller can do about it. The two are always
# distinct: a cause that repeats itself as an action tells the reader nothing.
function Write-Failure {
    param([string]$Code, [string]$Cause, [string]$Action, [string]$DataFragment = '{}')
    $map = [ordered]@{ code = $Code; cause = $Cause; action = $Action }
    Write-Result -DataFragment $DataFragment -ErrorMap $map
}

# A usage error is the caller's mistake, not a handled outcome, so it is the one path that
# leaves the canonical envelope behind and exits non-zero.
function Exit-Usage {
    param([string]$Message)
    [Console]::Error.WriteLine("workitem: $Message")
    exit 2
}

# ---------------------------------------------------------------------------
# Untrusted-data handling
# ---------------------------------------------------------------------------

# Retrieved free text is attacker-influenced. It is neutralised, capped, then wrapped in a
# fixed envelope so a caller cannot mistake it for instructions. Neutralisation runs before
# the cap so the cap can never split a marker into something that looks like a real one.
function Protect-RetrievedText {
    param([AllowNull()][string]$Value, [string]$FieldName)
    if ($null -eq $Value -or $Value -eq '') { return $Value }

    $neutralised = $Value.Replace($script:UntrustedCloseMarker, $script:UntrustedNeutralised)

    if ($neutralised.Length -gt $script:MaxFieldChars) {
        $neutralised = $neutralised.Substring(0, $script:MaxFieldChars)
        Add-WorkItemWarning 'content_truncated'
        if ($FieldName -and $script:TruncatedFields -notcontains $FieldName) {
            $script:TruncatedFields += $FieldName
        }
    }

    return $script:UntrustedOpenMarker + "`n" + $neutralised + "`n" + $script:UntrustedCloseMarker
}

# The caller-facing names of every field the cap shortened. A warning says something was cut;
# this says what, which is the difference between a caller retrying and a caller guessing.
$script:TruncatedFields = @()

# ---------------------------------------------------------------------------
# Azure CLI invocation
# ---------------------------------------------------------------------------

# Strip anything credential-shaped before any CLI diagnostic reaches the caller, then cap the
# remainder. Verbose and debug are never enabled: they are the switches that make the CLI print
# request headers.
function Get-ScrubbedStderr {
    param([AllowNull()][string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) { return '' }
    $scrubbed = $Value
    $scrubbed = [regex]::Replace($scrubbed, '(?i)bearer\s+[A-Za-z0-9\-\._~\+\/]+=*', 'bearer [redacted]')
    $scrubbed = [regex]::Replace($scrubbed, '(?i)(pat|token|password|secret|authorization)\s*[=:]\s*\S+', '$1=[redacted]')
    $scrubbed = [regex]::Replace($scrubbed, '\b[A-Za-z0-9_\-]{40,}\b', '[redacted]')
    $scrubbed = [regex]::Replace($scrubbed, '\s+', ' ').Trim()
    if ($scrubbed.Length -gt 400) { $scrubbed = $scrubbed.Substring(0, 400) }
    return $scrubbed
}

# Run az with an argument array. Nothing is interpolated into a command string, so free text
# never has to survive a round trip through shell quoting.
function Invoke-Az {
    param([string[]]$Arguments)
    $outFile = [System.IO.Path]::GetTempFileName()
    $errFile = [System.IO.Path]::GetTempFileName()
    try {
        $proc = Start-Process -FilePath 'az' -ArgumentList $Arguments -NoNewWindow -Wait -PassThru `
            -RedirectStandardOutput $outFile -RedirectStandardError $errFile
        $stdout = if (Test-Path $outFile) { Get-Content $outFile -Raw } else { '' }
        $stderr = if (Test-Path $errFile) { Get-Content $errFile -Raw } else { '' }
        return [PSCustomObject]@{
            ExitCode = $proc.ExitCode
            Stdout   = $stdout
            Stderr   = (Get-ScrubbedStderr $stderr)
        }
    } finally {
        Remove-Item $outFile -Force -ErrorAction SilentlyContinue
        Remove-Item $errFile -Force -ErrorAction SilentlyContinue
    }
}

# Write a request body to a process-unique UTF-8 file with no BOM and hand the CLI the path.
# The caller removes it on every exit path, including failure.
function New-PayloadFile {
    param([string]$Content)
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ("devspark-workitem-$PID-$([guid]::NewGuid().ToString('N')).json")
    [System.IO.File]::WriteAllText($path, $Content, (New-Object System.Text.UTF8Encoding $false))
    return $path
}

# ---------------------------------------------------------------------------
# Fixture-backed mode
# ---------------------------------------------------------------------------

$script:FixtureData = $null

function Initialize-Fixture {
    if (-not $Fixture) { return }
    if ($Confirmed) {
        Exit-Usage 'a fixture path and -Confirmed are mutually exclusive; fixture mode never performs a real write'
    }
    if (-not (Test-Path $Fixture)) { Exit-Usage "fixture not found: $Fixture" }
    $script:FixtureData = Get-Content $Fixture -Raw | ConvertFrom-Json
    Add-WorkItemWarning 'fixture_mode'
}

function Get-FixtureResponse {
    param([string]$Key)
    if ($null -eq $script:FixtureData) { return $null }
    $responses = $script:FixtureData.responses
    if ($null -eq $responses) { return $null }
    if ($responses.PSObject.Properties.Name -contains $Key) { return $responses.$Key }
    return $null
}

# ---------------------------------------------------------------------------
# Context
# ---------------------------------------------------------------------------

function Resolve-Context {
    $ctx = Get-WorkTrackingContext -Organization $Organization -Project $Project -AreaPath $AreaPath
    return $ctx
}

function Get-OrganizationUrl {
    param([string]$Organization)
    if ($Organization -match '^https?://') { return $Organization.TrimEnd('/') }
    return "https://dev.azure.com/$Organization"
}

# The marker is appended rather than configurable: a team that could switch it off would lose
# the only way to tell an agent-created item from a hand-written one.
function Get-CreateTags {
    param($Context)
    $tags = @()
    if ($Context.Tags) { $tags += @($Context.Tags | Where-Object { $_ }) }
    $tags += $script:AgentTag
    return ($tags -join '; ')
}

function Assert-Context {
    param($Context)
    if (-not $Context.Organization) {
        Write-Failure 'context_unresolved' `
            'no organization could be resolved from an argument, the work_tracking block, the top-level config or the origin remote' `
            'set "organization" in .devspark.work/devspark.json or pass -Organization'
    }
    if (-not $Context.Project) {
        Write-Failure 'context_unresolved' `
            'no project could be resolved from an argument, the work_tracking block, the top-level config or the origin remote' `
            'set "project" in .devspark.work/devspark.json or pass -Project'
    }
}

function Assert-Capability {
    if ($script:FixtureData) { return }
    $cap = Get-AzdoWorkItemCapability
    if (-not $cap.CliPresent) {
        Write-Failure 'cli_missing' `
            'the az command was not found on PATH' `
            'install the Azure CLI from https://learn.microsoft.com/cli/azure/install-azure-cli and reopen the shell'
    }
    if (-not $cap.ExtensionPresent) {
        Write-Failure 'extension_missing' `
            'the azure-devops extension is not installed for the Azure CLI' `
            "run: az extension add --name azure-devops (version $($cap.MinimumVersion) or later)"
    }
    if (-not $cap.MeetsMinimum) {
        Write-Failure 'extension_missing' `
            "the installed azure-devops extension is version $($cap.ExtensionVersion), below the verified minimum" `
            "run: az extension update --name azure-devops (version $($cap.MinimumVersion) or later)"
    }
}

function Assert-Authenticated {
    if ($script:FixtureData) { return }
    $probe = Invoke-Az @('account', 'show', '--only-show-errors', '--output', 'none')
    if ($probe.ExitCode -ne 0) {
        Write-Failure 'auth_failed' `
            'the Azure CLI has no usable credential for this organization' `
            'run: az login, then re-run the command'
    }
}

function Assert-Confirmed {
    # A write happens because a person said so in this run. No configuration value, environment
    # variable or autonomy setting can stand in for that.
    #
    # This exits as a usage error rather than returning an error envelope. The flag is a
    # required argument for a write, and a caller that omitted it has not made a request the
    # helper can answer -- as distinct from a request it tried and could not complete.
    #
    # Fixture mode is exempt because a recorded response has no service behind it: the write
    # path records what it would have sent and stops. Confirming a write against invented data
    # would mean nothing, which is why supplying both is rejected as a usage error instead.
    if ($script:FixtureData) { return }
    if (-not $Confirmed) {
        Exit-Usage "operation '$Operation' writes to Azure DevOps and requires -Confirmed; re-run with -Confirmed once the proposed change has been reviewed"
    }
}

function Assert-Rev {
    if ($Rev -lt 0) {
        Exit-Usage 'a write requires -Rev carrying the revision the proposal was built from'
    }
}

# ---------------------------------------------------------------------------
# Work item retrieval
# ---------------------------------------------------------------------------

function Get-FieldValue {
    param($Fields, [string]$Name)
    if ($null -eq $Fields) { return $null }
    if ($Fields.PSObject.Properties.Name -contains $Name) { return $Fields.$Name }
    return $null
}

function Get-WorkItemSnapshot {
    param($Context, [string]$WorkItemId)

    $fixture = Get-FixtureResponse "read:$WorkItemId"
    if ($fixture) { return $fixture }
    if ($script:FixtureData) {
        Write-Failure 'item_unresolvable' `
            "the fixture has no recorded response for work item $WorkItemId" `
            'add a "read:<id>" entry to the fixture, or run without -Fixture'
    }

    $orgUrl = Get-OrganizationUrl $Context.Organization
    $result = Invoke-Az @(
        'boards', 'work-item', 'show',
        '--id', $WorkItemId,
        '--org', $orgUrl,
        '--only-show-errors',
        '--output', 'json'
    )
    if ($result.ExitCode -ne 0) {
        Write-Failure 'item_unresolvable' `
            "work item $WorkItemId could not be retrieved: $($result.Stderr)" `
            'confirm the id exists in this project and that the signed-in account can read it'
    }
    try { return ($result.Stdout | ConvertFrom-Json) } catch {
        Write-Failure 'item_unresolvable' `
            "work item $WorkItemId returned a response that could not be parsed" `
            'retry, and if it persists run the same az boards work-item show command directly to inspect it'
    }
}

function ConvertTo-SnapshotFragment {
    param($Item, $Context, [switch]$WrapText)

    $fields = if ($Item.PSObject.Properties.Name -contains 'fields') { $Item.fields } else { $null }
    $id = if ($Item.PSObject.Properties.Name -contains 'id') { [int]$Item.id } else { $null }

    $rev = Get-FieldValue $fields 'System.Rev'
    if ($null -eq $rev -and $Item.PSObject.Properties.Name -contains 'rev') { $rev = $Item.rev }

    $areaPath = Get-FieldValue $fields 'System.AreaPath'
    if ($Context -and $Context.AreaPath -and $areaPath -and
        -not ("$areaPath".StartsWith("$($Context.AreaPath)", [StringComparison]::OrdinalIgnoreCase))) {
        Add-WorkItemWarning 'area_mismatch'
    }

    $iteration = Get-FieldValue $fields 'System.IterationPath'
    if (-not $iteration) { Add-WorkItemWarning 'iteration_missing' }

    $storyPoints = Get-FieldValue $fields 'Microsoft.VSTS.Scheduling.StoryPoints'
    if ($null -eq $storyPoints) { Add-WorkItemWarning 'story_points_unsupported' }

    $assigned = Get-FieldValue $fields 'System.AssignedTo'
    if ($assigned -and $assigned.PSObject.Properties.Name -contains 'displayName') {
        $assigned = $assigned.displayName
    }

    $description = Get-FieldValue $fields 'System.Description'
    $acceptance = Get-FieldValue $fields 'Microsoft.VSTS.Common.AcceptanceCriteria'
    if ($WrapText) {
        $description = Protect-RetrievedText $description -FieldName 'description'
        $acceptance = Protect-RetrievedText $acceptance -FieldName 'acceptance_criteria'
    }

    $url = ''
    if ($Context -and $Context.Organization -and $id) {
        $url = (Get-OrganizationUrl $Context.Organization) + "/$($Context.Project)/_workitems/edit/$id"
    }

    $map = [ordered]@{
        id                  = $id
        rev                 = if ($null -ne $rev) { [int]$rev } else { $null }
        type                = Get-FieldValue $fields 'System.WorkItemType'
        title               = Get-FieldValue $fields 'System.Title'
        state               = Get-FieldValue $fields 'System.State'
        area_path           = $areaPath
        iteration_path      = $iteration
        tags                = Get-FieldValue $fields 'System.Tags'
        description         = $description
        acceptance_criteria = $acceptance
        story_points        = $storyPoints
        assigned_to         = $assigned
        url                 = $url
    }
    $frag = [ordered]@{}
    foreach ($key in $map.Keys) { $frag[$key] = ConvertTo-JsonScalar $map[$key] }
    $frag['truncated'] = ConvertTo-JsonArray $script:TruncatedFields
    return (ConvertTo-JsonObjectFromFragments $frag)
}

# ---------------------------------------------------------------------------
# Field parsing
# ---------------------------------------------------------------------------

function ConvertFrom-SetArguments {
    # Under -File every argument arrives as one string, so -Field a,b never binds as an array.
    # Splitting is safe for names, which are identifiers, and unsafe for values, which may
    # legitimately contain a comma -- so a caller changing more than one field at a time makes
    # one call per field, and the count check below is what tells them so.
    $names = @($Field | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })

    if ($names.Count -ne $Value.Count) {
        Exit-Usage "each -Field needs one -Value; got $($names.Count) field(s) and $($Value.Count) value(s)"
    }
    $parsed = [ordered]@{}
    for ($i = 0; $i -lt $names.Count; $i++) {
        $name = $names[$i]
        if (-not $script:FieldMap.Contains($name)) {
            Exit-Usage "unknown field '$name'; supported: $(($script:FieldMap.Keys) -join ', ')"
        }
        $parsed[$name] = $Value[$i]
    }
    return $parsed
}

# A work-item URL names the organization, the project, and the item in one string, so
# accepting it removes three chances to pair an id with the wrong project by hand.
function Expand-UrlArgument {
    if (-not $Url) { return }
    $match = [regex]::Match(
        $Url,
        '^https?://(?:dev\.azure\.com/(?<org>[^/]+)|(?<org2>[^./]+)\.visualstudio\.com)/(?<project>[^/]+)/_workitems/edit/(?<id>\d+)'
    )
    if (-not $match.Success) { Exit-Usage "not an Azure DevOps work item URL: $Url" }

    $org = if ($match.Groups['org'].Success) { $match.Groups['org'].Value } else { $match.Groups['org2'].Value }
    $number = $match.Groups['id'].Value
    if ($Id -and $Id -ne $number) { Exit-Usage "-Id $Id and -Url disagree about which work item is meant" }

    $script:Id = $number
    if (-not $Organization) { $script:Organization = $org -replace '%20', ' ' }
    if (-not $Project) { $script:Project = $match.Groups['project'].Value -replace '%20', ' ' }
}

# Accepts the forms a person actually writes in a pull request: AB#123, #123, a bare number,
# or a full work-item URL. Anything else returns nothing, so the caller can reject it rather
# than guess at what was meant.
function Get-ReferenceId {
    param([string]$Raw)
    $fromUrl = [regex]::Match($Raw, '_workitems/edit/(\d+)')
    if ($fromUrl.Success) { return $fromUrl.Groups[1].Value }
    $bare = [regex]::Match($Raw, '^(?:AB#|ab#|#)?(\d+)$')
    if ($bare.Success) { return $bare.Groups[1].Value }
    return ''
}

# ---------------------------------------------------------------------------
# Writes
# ---------------------------------------------------------------------------

# Every write goes through the REST endpoint with a JSON Patch body carried in a file. The
# az boards commands expose no file-based payload and no acceptance-criteria parameter, so
# they cannot satisfy the transport rule for the fields this feature actually writes.
function Invoke-PatchWrite {
    param($Context, [string]$WorkItemId, [System.Collections.Specialized.OrderedDictionary]$Changes, [string]$WorkItemType)

    $ops = foreach ($name in $Changes.Keys) {
        $map = [ordered]@{
            op    = 'add'
            path  = '/fields/' + $script:FieldMap[$name]
            value = $Changes[$name]
        }
        ConvertTo-JsonObject $map
    }
    $payload = '[' + (@($ops) -join ', ') + ']'

    $payloadFile = New-PayloadFile $payload
    try {
        if ($script:FixtureData) {
            return [PSCustomObject]@{ ExitCode = 0; Stdout = '{}'; Stderr = ''; Recorded = $payload }
        }
        $orgUrl = Get-OrganizationUrl $Context.Organization
        $arguments = @(
            'devops', 'invoke',
            '--area', 'wit',
            '--resource', 'workitems',
            '--org', $orgUrl,
            '--in-file', $payloadFile,
            '--encoding', 'utf-8',
            '--media-type', 'application/json-patch+json',
            '--only-show-errors',
            '--output', 'json'
        )
        if ($WorkItemId) {
            $arguments += @('--route-parameters', "project=$($Context.Project)", "id=$WorkItemId", '--http-method', 'PATCH')
        } else {
            $arguments += @('--route-parameters', "project=$($Context.Project)", "type=$WorkItemType", '--http-method', 'POST')
        }
        $arguments += @('--api-version', '7.0')
        return Invoke-Az $arguments
    } finally {
        Remove-Item $payloadFile -Force -ErrorAction SilentlyContinue
    }
}

# A proposal is built against a revision. If the item moved since, the write is refused rather
# than resolved: silently overwriting someone else's edit is the failure this guards against.
function Test-RevisionCurrent {
    param($Context, [string]$WorkItemId)
    $current = Get-WorkItemSnapshot -Context $Context -WorkItemId $WorkItemId
    $fields = if ($current.PSObject.Properties.Name -contains 'fields') { $current.fields } else { $null }
    $currentRev = Get-FieldValue $fields 'System.Rev'
    if ($null -eq $currentRev -and $current.PSObject.Properties.Name -contains 'rev') { $currentRev = $current.rev }
    return [PSCustomObject]@{ Current = [int]$currentRev; Matches = ([int]$currentRev -eq $Rev) }
}

# The record of what a run changed names fields only. Values are deliberately absent: an audit
# trail that quotes the body it wrote becomes a second copy of the data it is auditing.
function New-AuditFragment {
    param([string]$Op, [string]$WorkItemId, [string[]]$FieldNames, $RevBefore, $RevAfter)
    $frag = [ordered]@{}
    $frag['operation'] = ConvertTo-JsonStringLiteral $Op
    $frag['id'] = ConvertTo-JsonScalar $WorkItemId
    $frag['fields'] = ConvertTo-JsonArray $FieldNames
    $frag['rev_before'] = ConvertTo-JsonScalar $RevBefore
    $frag['rev_after'] = ConvertTo-JsonScalar $RevAfter
    return ConvertTo-JsonObjectFromFragments $frag
}

# ---------------------------------------------------------------------------
# Operations
# ---------------------------------------------------------------------------

function Invoke-CheckOperation {
    $ctx = Resolve-Context
    $cap = Get-AzdoWorkItemCapability

    $authenticated = $false
    if ($cap.CliPresent) {
        $probe = Invoke-Az @('account', 'show', '--only-show-errors', '--output', 'none')
        $authenticated = ($probe.ExitCode -eq 0)
    }

    $map = [ordered]@{
        cli_present              = $cap.CliPresent
        extension_present        = ($cap.ExtensionPresent -and $cap.MeetsMinimum)
        authenticated            = $authenticated
        organization             = if ($ctx.Organization) { $ctx.Organization } else { $null }
        project                  = if ($ctx.Project) { $ctx.Project } else { $null }
        work_tracking_configured = $ctx.Configured
        default_area_path        = if ($ctx.AreaPath) { $ctx.AreaPath } else { $null }
    }
    Write-Result -DataFragment (ConvertTo-JsonObject $map)
}

function Invoke-ReadOperation {
    if (-not $Id) { Exit-Usage 'read requires -Id' }
    $ctx = Resolve-Context
    Assert-Context $ctx
    Assert-Capability
    Assert-Authenticated
    $item = Get-WorkItemSnapshot -Context $ctx -WorkItemId $Id
    Write-Result -DataFragment (ConvertTo-SnapshotFragment -Item $item -Context $ctx -WrapText)
}

function Invoke-ProposeUpdateOperation {
    if (-not $Id) { Exit-Usage 'propose-update requires -Id' }
    $changes = ConvertFrom-SetArguments
    if ($changes.Count -eq 0) { Exit-Usage 'propose-update requires at least one -Set name=value' }

    $ctx = Resolve-Context
    Assert-Context $ctx
    Assert-Capability
    Assert-Authenticated

    $item = Get-WorkItemSnapshot -Context $ctx -WorkItemId $Id
    $fields = if ($item.PSObject.Properties.Name -contains 'fields') { $item.fields } else { $null }
    $rev = Get-FieldValue $fields 'System.Rev'

    $entries = foreach ($name in $changes.Keys) {
        $map = [ordered]@{
            field    = $name
            current  = Protect-RetrievedText ([string](Get-FieldValue $fields $script:FieldMap[$name])) -FieldName $name
            proposed = $changes[$name]
        }
        ConvertTo-JsonObject $map
    }

    $frag = [ordered]@{}
    $frag['proposed'] = '[' + (@($entries) -join ', ') + ']'
    $frag['rev'] = ConvertTo-JsonScalar ([int]$rev)
    $frag['truncated'] = ConvertTo-JsonArray $script:TruncatedFields
    Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
}

function Invoke-ApplyUpdateOperation {
    if (-not $Id) { Exit-Usage 'apply-update requires -Id' }
    Assert-Rev
    $changes = ConvertFrom-SetArguments
    if ($changes.Count -eq 0) { Exit-Usage 'apply-update requires at least one -Set name=value' }

    $ctx = Resolve-Context
    Assert-Context $ctx
    Assert-Confirmed
    Assert-Capability
    Assert-Authenticated

    $revCheck = Test-RevisionCurrent -Context $ctx -WorkItemId $Id
    if (-not $revCheck.Matches) {
        $frag = [ordered]@{}
        $frag['written'] = 'false'
        $frag['verified'] = 'null'
        $frag['divergence'] = '[]'
        $frag['refused'] = ConvertTo-JsonStringLiteral 'stale_revision'
        $frag['audit'] = 'null'
        Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
    }

    $write = Invoke-PatchWrite -Context $ctx -WorkItemId $Id -Changes $changes
    if ($write.ExitCode -ne 0) {
        Write-Failure 'item_unresolvable' `
            "the update to work item $Id was rejected: $($write.Stderr)" `
            'confirm the field names are valid for this work item type and that the account can edit it'
    }

    # A write is not finished until it has been read back. The caller is told what the item
    # actually holds now, not what was sent.
    $after = Get-WorkItemSnapshot -Context $ctx -WorkItemId $Id
    $afterFields = if ($after.PSObject.Properties.Name -contains 'fields') { $after.fields } else { $null }

    $divergent = @()
    foreach ($name in $changes.Keys) {
        $actual = [string](Get-FieldValue $afterFields $script:FieldMap[$name])
        if ($actual -ne [string]$changes[$name]) { $divergent += $name }
    }
    $afterRev = Get-FieldValue $afterFields 'System.Rev'

    $frag = [ordered]@{}
    $frag['written'] = 'true'
    $frag['verified'] = ConvertTo-SnapshotFragment -Item $after -Context $ctx -WrapText
    $frag['divergence'] = ConvertTo-JsonArray $divergent
    $frag['refused'] = 'null'
    $frag['audit'] = New-AuditFragment -Op 'apply-update' -WorkItemId $Id `
        -FieldNames @($changes.Keys) -RevBefore $Rev -RevAfter ([int]$afterRev)
    Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
}

function Invoke-CommentOperation {
    if (-not $Id) { Exit-Usage 'comment requires -Id' }
    if (-not $Text) { Exit-Usage 'comment requires -Text' }
    Assert-Rev

    $ctx = Resolve-Context
    Assert-Context $ctx
    Assert-Confirmed
    Assert-Capability
    Assert-Authenticated

    $revCheck = Test-RevisionCurrent -Context $ctx -WorkItemId $Id
    if (-not $revCheck.Matches) {
        $frag = [ordered]@{}
        $frag['comment_id'] = 'null'
        $frag['verified'] = 'null'
        $frag['divergence'] = '[]'
        $frag['refused'] = ConvertTo-JsonStringLiteral 'stale_revision'
        $frag['audit'] = 'null'
        Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
    }

    $before = Get-WorkItemSnapshot -Context $ctx -WorkItemId $Id
    $changes = [ordered]@{}
    $payloadFile = New-PayloadFile (ConvertTo-JsonObject ([ordered]@{ text = $Text }))
    try {
        if ($script:FixtureData) {
            $result = [PSCustomObject]@{ ExitCode = 0; Stdout = '{"id": 0}'; Stderr = '' }
        } else {
            $orgUrl = Get-OrganizationUrl $ctx.Organization
            $result = Invoke-Az @(
                'devops', 'invoke',
                '--area', 'wit', '--resource', 'comments',
                '--org', $orgUrl,
                '--route-parameters', "project=$($ctx.Project)", "workItemId=$Id",
                '--http-method', 'POST',
                '--in-file', $payloadFile,
                '--encoding', 'utf-8',
                # A three-part preview revision (e.g. 7.0-preview.3) crashes the installed
                # azure-devops extension's own client-side apiVersionToFloat() check; the bare
                # '-preview' form negotiates the same server-side preview revision without it.
                '--api-version', '7.0-preview',
                '--only-show-errors',
                '--output', 'json'
            )
        }
    } finally {
        Remove-Item $payloadFile -Force -ErrorAction SilentlyContinue
    }

    if ($result.ExitCode -ne 0) {
        Write-Failure 'item_unresolvable' `
            "the comment on work item $Id was rejected: $($result.Stderr)" `
            'confirm the id exists and that the signed-in account can comment on it'
    }

    $commentId = $null
    try {
        $parsed = $result.Stdout | ConvertFrom-Json
        if ($parsed.PSObject.Properties.Name -contains 'id') { $commentId = [int]$parsed.id }
    } catch { }

    # A comment must not disturb the item's own fields. The verification read exists to prove it.
    $after = Get-WorkItemSnapshot -Context $ctx -WorkItemId $Id
    $beforeFields = if ($before.PSObject.Properties.Name -contains 'fields') { $before.fields } else { $null }
    $afterFields = if ($after.PSObject.Properties.Name -contains 'fields') { $after.fields } else { $null }
    $divergent = @()
    foreach ($name in $script:FieldMap.Keys) {
        $b = [string](Get-FieldValue $beforeFields $script:FieldMap[$name])
        $a = [string](Get-FieldValue $afterFields $script:FieldMap[$name])
        if ($a -ne $b) { $divergent += $name }
    }
    $afterRev = Get-FieldValue $afterFields 'System.Rev'

    $frag = [ordered]@{}
    $frag['comment_id'] = ConvertTo-JsonScalar $commentId
    $frag['verified'] = ConvertTo-SnapshotFragment -Item $after -Context $ctx -WrapText
    $frag['divergence'] = ConvertTo-JsonArray $divergent
    $frag['refused'] = 'null'
    $frag['audit'] = New-AuditFragment -Op 'comment' -WorkItemId $Id `
        -FieldNames @() -RevBefore $Rev -RevAfter ([int]$afterRev)
    Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
}

function Resolve-CreateType($ctx) {
    $type = if ($Type) { $Type } elseif ($ctx.WorkItemType) { $ctx.WorkItemType } else { 'Task' }
    if ($type.Trim() -ieq 'User Story') {
        Write-Failure 'type_unsupported' `
            'creating a User Story is prohibited; DevSpark never creates one' `
            'set work_tracking.default_work_item_type to a type other than User Story, or name another work item type'
    }
    return $type
}

function Invoke-ProposeCreateOperation {
    $changes = ConvertFrom-SetArguments
    if (-not $Title) { Exit-Usage 'propose-create requires -Title' }
    if (-not $DescriptionHtml) { Exit-Usage 'propose-create requires -DescriptionHtml' }
    if (-not $AcceptanceCriteriaHtml) { Exit-Usage 'propose-create requires -AcceptanceCriteriaHtml' }

    $ctx = Resolve-Context
    Assert-Context $ctx
    $type = Resolve-CreateType $ctx

    if (-not $ctx.AreaPath) {
        Write-Failure 'context_unresolved' `
            'no default_area_path is configured, so a created item would land wherever the project defaults put it' `
            'add work_tracking.default_area_path to .devspark.work/devspark.json'
    }

    $draft = [ordered]@{
        work_item_type      = $type
        title               = $Title
        description         = $DescriptionHtml
        acceptance_criteria = $AcceptanceCriteriaHtml
        area_path           = $ctx.AreaPath
        iteration_path      = if ($ctx.IterationPath) { $ctx.IterationPath } else { $null }
        tags                = (Get-CreateTags $ctx)
    }

    # Story points only exist on some process templates. Saying so before the write is what
    # keeps a missing field from becoming a failed create.
    $storyPointsSupported = $changes.Contains('story_points')
    if (-not $storyPointsSupported) { Add-WorkItemWarning 'story_points_unsupported' }
    if (-not $ctx.IterationPath) { Add-WorkItemWarning 'iteration_missing' }

    $frag = [ordered]@{}
    $frag['draft'] = ConvertTo-JsonObject $draft
    $frag['story_points_supported'] = ConvertTo-JsonScalar $storyPointsSupported
    Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
}

function Invoke-ApplyCreateOperation {
    $changes = ConvertFrom-SetArguments
    if (-not $Title) { Exit-Usage 'apply-create requires -Title' }
    if (-not $DescriptionHtml) { Exit-Usage 'apply-create requires -DescriptionHtml' }
    if (-not $AcceptanceCriteriaHtml) { Exit-Usage 'apply-create requires -AcceptanceCriteriaHtml' }

    $ctx = Resolve-Context
    Assert-Context $ctx
    $type = Resolve-CreateType $ctx
    Assert-Confirmed
    Assert-Capability
    Assert-Authenticated

    if (-not $ctx.AreaPath) {
        Write-Failure 'context_unresolved' `
            'no default_area_path is configured, so a created item would land wherever the project defaults put it' `
            'add work_tracking.default_area_path to .devspark.work/devspark.json'
    }

    $payload = [ordered]@{
        title               = $Title
        description         = $DescriptionHtml
        acceptance_criteria = $AcceptanceCriteriaHtml
    }
    foreach ($name in $changes.Keys) { $payload[$name] = $changes[$name] }
    $payload['area_path'] = $ctx.AreaPath
    if ($ctx.IterationPath) { $payload['iteration_path'] = $ctx.IterationPath }
    $payload['tags'] = (Get-CreateTags $ctx)

    $write = Invoke-PatchWrite -Context $ctx -WorkItemId '' -Changes $payload -WorkItemType $type
    if ($write.ExitCode -ne 0) {
        Write-Failure 'type_unsupported' `
            "a '$type' work item could not be created: $($write.Stderr)" `
            'confirm the type exists in this project''s process template and set work_tracking.default_work_item_type accordingly'
    }

    $newId = $null
    try {
        $parsed = $write.Stdout | ConvertFrom-Json
        if ($parsed.PSObject.Properties.Name -contains 'id') { $newId = [int]$parsed.id }
    } catch { }

    if (-not $newId) {
        Write-Failure 'item_unresolvable' `
            'the create call returned no work item id, so the result could not be verified' `
            'check the project in the browser before retrying, so a duplicate is not created'
    }

    $after = Get-WorkItemSnapshot -Context $ctx -WorkItemId "$newId"
    $afterFields = if ($after.PSObject.Properties.Name -contains 'fields') { $after.fields } else { $null }
    $divergent = @()
    foreach ($name in $payload.Keys) {
        $actual = [string](Get-FieldValue $afterFields $script:FieldMap[$name])
        if ($actual -ne [string]$payload[$name]) { $divergent += $name }
    }
    $afterRev = Get-FieldValue $afterFields 'System.Rev'

    $frag = [ordered]@{}
    $frag['id'] = ConvertTo-JsonScalar $newId
    $frag['url'] = ConvertTo-JsonStringLiteral ((Get-OrganizationUrl $ctx.Organization) + "/$($ctx.Project)/_workitems/edit/$newId")
    $frag['verified'] = ConvertTo-SnapshotFragment -Item $after -Context $ctx -WrapText
    $frag['divergence'] = ConvertTo-JsonArray $divergent
    $frag['audit'] = New-AuditFragment -Op 'apply-create' -WorkItemId "$newId" `
        -FieldNames @($payload.Keys) -RevBefore $null -RevAfter ([int]$afterRev)
    Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
}

function Invoke-VerifyLinkOperation {
    if ($Reference.Count -eq 0) { Exit-Usage 'verify-link requires at least one -Reference' }

    # Invoked through -File, PowerShell hands every argument over as one string, so the array
    # a caller wrote as -Reference a,b arrives unsplit. Splitting here is what makes the
    # documented array form behave the same as the Bash twin's repeated --reference.
    $references = @($Reference | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() })

    $ctx = Resolve-Context
    Assert-Context $ctx

    # Only an explicit reference counts. Nothing here searches for a plausible match, because a
    # wrong guess writes to somebody else's work item.
    $seen = [System.Collections.Generic.HashSet[string]]::new()
    $entries = @()

    foreach ($raw in $references) {
        if (-not $raw) { continue }
        $number = Get-ReferenceId $raw
        if (-not $number) { Exit-Usage "not a work item reference: $raw" }
        if (-not $seen.Add($number)) { continue }

        $resolves = $false
        $inArea = $false
        $url = $null

        if ($script:FixtureData -or (Get-Command az -ErrorAction SilentlyContinue)) {
            $item = $null
            $fixture = Get-FixtureResponse "read:$number"
            if ($fixture) {
                $item = $fixture
            } elseif (-not $script:FixtureData) {
                $orgUrl = Get-OrganizationUrl $ctx.Organization
                $probe = Invoke-Az @('boards', 'work-item', 'show', '--id', $number, '--org', $orgUrl, '--only-show-errors', '--output', 'json')
                if ($probe.ExitCode -eq 0) { try { $item = $probe.Stdout | ConvertFrom-Json } catch { } }
            }
            if ($item) {
                $resolves = $true
                $fields = if ($item.PSObject.Properties.Name -contains 'fields') { $item.fields } else { $null }
                $area = [string](Get-FieldValue $fields 'System.AreaPath')
                $inArea = ($ctx.AreaPath -and $area.StartsWith("$($ctx.AreaPath)", [StringComparison]::OrdinalIgnoreCase))
                $url = (Get-OrganizationUrl $ctx.Organization) + "/$($ctx.Project)/_workitems/edit/$number"
            }
        }

        $map = [ordered]@{
            raw                = $raw
            id                 = [int]$number
            resolves           = $resolves
            in_configured_area = $inArea
            url                = $url
        }
        $entries += (ConvertTo-JsonObject $map)
    }

    $frag = [ordered]@{}
    $frag['references'] = '[' + (@($entries) -join ', ') + ']'
    Write-Result -DataFragment (ConvertTo-JsonObjectFromFragments $frag)
}

# ---------------------------------------------------------------------------
# Dispatch
# ---------------------------------------------------------------------------

if (-not $Json) { Exit-Usage 'the -Json flag is required; the canonical JSON line is the only supported output' }

Expand-UrlArgument
Initialize-Fixture

switch ($Operation) {
    'check'          { Invoke-CheckOperation }
    'read'           { Invoke-ReadOperation }
    'propose-update' { Invoke-ProposeUpdateOperation }
    'apply-update'   { Invoke-ApplyUpdateOperation }
    'comment'        { Invoke-CommentOperation }
    'propose-create' { Invoke-ProposeCreateOperation }
    'apply-create'   { Invoke-ApplyCreateOperation }
    'verify-link'    { Invoke-VerifyLinkOperation }
}

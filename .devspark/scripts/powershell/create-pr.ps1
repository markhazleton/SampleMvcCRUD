#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
[CmdletBinding()]
param(
    [ValidateSet('Preflight','Create','Update')]
    [string]$Mode = 'Preflight',
    [switch]$Json,
    [string]$Title,
    [string]$Body,
    [string]$BodyFile,
    [string]$Base,
    [int]$PrNumber,
    [switch]$Draft,
    [string[]]$Reviewer = @(),
    [string[]]$Label = @(),
    [string[]]$Assignee = @(),
    [string[]]$Issue = @()
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"
. "$PSScriptRoot/platform.ps1"

function Get-JsonError {
    param([string]$Message, [string]$Details = '')
    [PSCustomObject]@{
        error = $true
        message = $Message
        details = $Details
    } | ConvertTo-Json -Depth 10
}

function Get-DefaultBaseBranch {
    if ($DevSparkPlatform.Name -eq 'github' -and (Get-Command gh -ErrorAction SilentlyContinue)) {
        try {
            if (& $DevSparkPlatform.AuthCheck) {
                $branch = gh repo view --json defaultBranchRef --jq '.defaultBranchRef.name' 2>$null
                if ($branch) { return $branch }
            }
        } catch { }
    }
    try {
        $symbolic = git symbolic-ref refs/remotes/origin/HEAD 2>$null
        if ($symbolic) { return ($symbolic -replace '^refs/remotes/origin/', '') }
    } catch { }
    return 'main'
}

function Resolve-PrBody {
    $bodyValue = $Body
    if ($BodyFile -and (Test-Path $BodyFile)) {
        $bodyValue = Get-Content -LiteralPath $BodyFile -Raw -Encoding utf8
    }
    if ($Issue.Count -gt 0) {
        $issueLine = 'Refs: ' + ($Issue -join ', ')
        if ($bodyValue) { $bodyValue += [Environment]::NewLine + [Environment]::NewLine }
        $bodyValue += $issueLine
    }
    return $bodyValue
}

function Get-MarkdownSectionText {
    param(
        [string]$Path,
        [string]$SectionName
    )

    if (-not (Test-Path $Path)) {
        return ''
    }

    $content = Get-Content -LiteralPath $Path -Encoding utf8
    $capture = $false
    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($line in $content) {
        $sectionMatch = [regex]::Match($line, '^##\s+(.+)$')
        if ($sectionMatch.Success) {
            $heading = $sectionMatch.Groups[1].Value.Trim()
            if ($capture -and $heading -ne $SectionName) {
                break
            }
            $capture = $heading -eq $SectionName
            continue
        }
        if ($capture) {
            $lines.Add($line)
        }
    }

    return (($lines -join [Environment]::NewLine).Trim())
}

function Get-GateAcknowledgements {
    param([string]$TasksPath)

    if (-not (Test-Path $TasksPath)) {
        return @()
    }

    $section = Get-MarkdownSectionText -Path $TasksPath -SectionName 'Gate Acknowledgements'
    if (-not $section) {
        return @()
    }

    $entries = New-Object System.Collections.Generic.List[string]
    $current = New-Object System.Collections.Generic.List[string]
    foreach ($line in ($section -split "`r?`n")) {
        $trimmed = $line.Trim()
        if (-not $trimmed) {
            if ($current.Count -gt 0) {
                $entries.Add(($current -join [Environment]::NewLine).Trim())
                $current.Clear()
            }
            continue
        }
        if ($trimmed.StartsWith('- Gate:') -and $current.Count -gt 0) {
            $entries.Add(($current -join [Environment]::NewLine).Trim())
            $current.Clear()
        }
        $current.Add($trimmed)
    }
    if ($current.Count -gt 0) {
        $entries.Add(($current -join [Environment]::NewLine).Trim())
    }
    return @($entries)
}

function Find-QuickfixRecordForBranch {
    param(
        [string]$RepoRoot,
        [string]$BranchName
    )

    $quickfixDir = Join-Path $RepoRoot '.devspark.work/quickfixes'
    if (-not (Test-Path $quickfixDir)) {
        return $null
    }

    $quickfixFiles = Get-ChildItem -Path $quickfixDir -Filter *.md -File -ErrorAction SilentlyContinue |
        Where-Object {
            $content = Get-Content -LiteralPath $_.FullName -Encoding utf8
            $branchLine = $content | Where-Object { $_ -match '^- \*\*Branch\*\*:\s*(.+)$' } | Select-Object -First 1
            $branchLine -and (($branchLine -replace '^- \*\*Branch\*\*:\s*', '').Trim() -eq $BranchName)
        } |
        Sort-Object Name

    if ($quickfixFiles) {
        return ($quickfixFiles | Select-Object -Last 1).FullName
    }
    return $null
}

function Get-QuickfixRecordSummary {
    param([string]$QuickfixPath)

    if (-not $QuickfixPath -or -not (Test-Path $QuickfixPath)) {
        return $null
    }

    $heading = Get-Content -LiteralPath $QuickfixPath -Encoding utf8 | Where-Object { $_ -match '^# ' } | Select-Object -First 1
    $idLine = Get-Content -LiteralPath $QuickfixPath -Encoding utf8 | Where-Object { $_ -match '^- \*\*ID\*\*:\s*(.+)$' } | Select-Object -First 1
    return [PSCustomObject]@{
        path = $QuickfixPath
        id = if ($idLine) { ($idLine -replace '^- \*\*ID\*\*:\s*', '').Trim() } else { [IO.Path]::GetFileNameWithoutExtension($QuickfixPath) }
        title = if ($heading) { $heading -replace '^#\s*', '' } else { '' }
        classification = Get-MarkdownFrontmatterValue -Path $QuickfixPath -Key 'classification'
        risk_level = Get-MarkdownFrontmatterValue -Path $QuickfixPath -Key 'risk_level'
        required_gates = Get-MarkdownFrontmatterValue -Path $QuickfixPath -Key 'required_gates'
        recommended_next_step = Get-MarkdownFrontmatterValue -Path $QuickfixPath -Key 'recommended_next_step'
        problem_statement = Get-MarkdownSectionText -Path $QuickfixPath -SectionName 'Problem Statement'
        gate_acknowledgements = Get-MarkdownSectionText -Path $QuickfixPath -SectionName 'Gate Acknowledgements'
    }
}

function Get-TaskCounts {
    param([string]$TasksPath)
    if (-not (Test-Path $TasksPath)) {
        return [PSCustomObject]@{ total = 0; completed = 0; incomplete = 0 }
    }
    $content = Get-Content -LiteralPath $TasksPath -Encoding utf8
    $taskLines = @($content | Where-Object { $_ -match '^\s*- \[([ xX])\]' })
    $completedLines = @($content | Where-Object { $_ -match '^\s*- \[[xX]\]' })
    return [PSCustomObject]@{
        total = $taskLines.Count
        completed = $completedLines.Count
        incomplete = $taskLines.Count - $completedLines.Count
    }
}

function Get-ChecklistSummary {
    param([string]$ChecklistDir)
    if (-not (Test-Path $ChecklistDir)) {
        return @()
    }
    $results = @()
    Get-ChildItem -Path $ChecklistDir -Filter *.md -File -ErrorAction SilentlyContinue | ForEach-Object {
        $content = Get-Content -LiteralPath $_.FullName -Encoding utf8
        $taskLines = @($content | Where-Object { $_ -match '^\s*- \[([ xX])\]' })
        $completed = @($content | Where-Object { $_ -match '^\s*- \[[xX]\]' })
        $results += [PSCustomObject]@{
            name = $_.Name
            total = $taskLines.Count
            completed = $completed.Count
            incomplete = $taskLines.Count - $completed.Count
            status = if (($taskLines.Count - $completed.Count) -eq 0) { 'pass' } else { 'fail' }
        }
    }
    return $results
}

function Get-GateArtifacts {
    param([string]$FeatureDir)
    $results = @()
    foreach ($gate in @('analyze','critic','checklist')) {
        $candidatePaths = @(
            (Join-Path $FeatureDir "gates/$gate.md"),
            (Join-Path $FeatureDir "$gate.md")
        )
        $path = $candidatePaths | Where-Object { Test-Path $_ } | Select-Object -First 1
        if ($path) {
            $content = Get-Content -LiteralPath $path -Encoding utf8
            $status = (($content | Where-Object { $_ -match '^status:\s*' } | Select-Object -First 1) -replace '^status:\s*', '')
            $blockingRaw = (($content | Where-Object { $_ -match '^blocking:\s*' } | Select-Object -First 1) -replace '^blocking:\s*', '')
            $severity = (($content | Where-Object { $_ -match '^severity:\s*' } | Select-Object -First 1) -replace '^severity:\s*', '')
            $summary = (($content | Where-Object { $_ -match '^summary:\s*' } | Select-Object -First 1) -replace '^summary:\s*', '')
            $results += [PSCustomObject]@{
                gate = $gate
                path = $path
                status = if ($status) { $status } else { 'unknown' }
                severity = if ($severity) { $severity } else { 'info' }
                summary = $summary
                blocking = ($blockingRaw -eq 'true')
            }
        }
    }
    return $results
}

# The engine needs PyYAML and Python 3.11+, so an interpreter that cannot run it is "unavailable",
# never a reason to report the Knowledge as broken.
function Resolve-KnowledgePython {
    foreach ($name in @('python3', 'python')) {
        $command = Get-Command $name -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $command) { continue }
        & $command.Source -c 'import sys, yaml; sys.exit(0 if sys.version_info >= (3, 11) else 1)' 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { return $command.Source }
    }
    return $null
}

function New-KnowledgeBlocker {
    param([string]$Kind, $Path, $Source, $SourceDate, $LastVerified, $Message)
    [PSCustomObject]@{
        kind = $Kind
        path = $Path
        source = $Source
        source_date = $SourceDate
        last_verified = $LastVerified
        message = $Message
    }
}

function New-KnowledgeResult {
    param(
        [string]$Readiness,
        $IndexFresh,
        [string]$Summary,
        [string]$Head,
        [object[]]$Blockers = @(),
        [int]$StaleUntouchedCount = 0,
        [string]$CheckOutput
    )
    [PSCustomObject]@{
        index_fresh = $IndexFresh
        gap_summary = if ($Summary) { $Summary } else { $null }
        readiness = $Readiness
        blockers = @($Blockers)
        stale_untouched_count = $StaleUntouchedCount
        checked_head = $Head
        check_output = if ($CheckOutput) { $CheckOutput } else { $null }
    }
}

# ConvertFrom-Json turns an ISO date string into [datetime]; blockers carry the original text.
function ConvertTo-DateText {
    param($Value)
    if ($Value -is [datetime]) { return $Value.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture) }
    return [string]$Value
}

# Committed-state Knowledge readiness for the HEAD being submitted. Read-only: it never
# regenerates, edits, or commits anything.
function Get-KnowledgeConsistency {
    param([string]$RepoRoot, [string]$Head, [string[]]$ChangedPaths = @())

    $engine = Resolve-KnowledgeEngine -RepoRoot $RepoRoot
    if (-not $engine) {
        return New-KnowledgeResult -Readiness 'unknown' -IndexFresh $null -Summary 'build_knowledge_index.py not found' -Head $Head
    }

    $tracked = @(git -C $RepoRoot ls-tree -r --name-only HEAD -- .knowledge/index.json 2>$null | Where-Object { $_ })
    if ($tracked.Count -eq 0) {
        return New-KnowledgeResult -Readiness 'not-applicable' -IndexFresh $null -Summary '.knowledge/index.json is not tracked at HEAD' -Head $Head
    }

    $python = Resolve-KnowledgePython
    if (-not $python) {
        return New-KnowledgeResult -Readiness 'unknown' -IndexFresh $null -Summary 'no usable Python interpreter (python3 or python with PyYAML, 3.11+) found' -Head $Head
    }

    $output = & $python $engine --repo-root $RepoRoot --check 2>&1
    $exitCode = $LASTEXITCODE
    $lines = @($output | ForEach-Object { "$_".TrimEnd() } | Where-Object { $_ })
    $text = $lines -join "`n"

    if ($exitCode -eq 0) {
        $index = Get-Content -LiteralPath (Join-Path $RepoRoot '.knowledge/index.json') -Raw -Encoding utf8 | ConvertFrom-Json
        $stale = @($index.stale_entities | Where-Object { $_ })
        $changed = [System.Collections.Generic.HashSet[string]]::new([string[]]@($ChangedPaths))
        $blockers = @()
        $untouched = 0
        foreach ($entry in $stale) {
            $path = [string]$entry.path
            $source = ([string]$entry.source).TrimEnd('/')
            $beneath = @($ChangedPaths | Where-Object { $source -and $_.StartsWith("$source/", [StringComparison]::Ordinal) }).Count -gt 0
            if ($changed.Contains($path) -or ($source -and $changed.Contains($source)) -or $beneath) {
                $blockers += New-KnowledgeBlocker -Kind 'stale-entity' -Path $path -Source ([string]$entry.source) `
                    -SourceDate (ConvertTo-DateText $entry.source_date) -LastVerified (ConvertTo-DateText $entry.last_verified) -Message $null
            } else {
                $untouched++
            }
        }
        if ($blockers.Count -gt 0) {
            return New-KnowledgeResult -Readiness 'blocked' -IndexFresh $true -Summary "$($blockers.Count) touched Knowledge document(s) are stale" `
                -Head $Head -Blockers $blockers -StaleUntouchedCount $untouched -CheckOutput $text
        }
        return New-KnowledgeResult -Readiness 'ready' -IndexFresh $true -Head $Head -StaleUntouchedCount $untouched -CheckOutput $text
    }

    if ($exitCode -eq 1) {
        $summary = if ($lines.Count -gt 0) { $lines[0] } else { 'knowledge check failed' }
        $drift = @($lines | Where-Object { $_.StartsWith('DRIFT:') })
        if ($drift.Count -gt 0) {
            $blockers = @($drift | ForEach-Object {
                $artifact = if ($_ -match 'coverage\.json') { '.knowledge/ontology/coverage.json' } else { '.knowledge/index.json' }
                New-KnowledgeBlocker -Kind 'drift' -Path $artifact -Source $null -SourceDate $null -LastVerified $null `
                    -Message 'Generated Knowledge does not match a fresh build of the committed HEAD'
            })
        } else {
            $blockers = @(New-KnowledgeBlocker -Kind 'build-error' -Path $null -Source $null -SourceDate $null -LastVerified $null -Message $lines[-1])
        }
        return New-KnowledgeResult -Readiness 'blocked' -IndexFresh $false -Summary $summary -Head $Head -Blockers $blockers -CheckOutput $text
    }

    return New-KnowledgeResult -Readiness 'unknown' -IndexFresh $null -Summary "knowledge engine returned an unsupported result (exit $exitCode)" -Head $Head -CheckOutput $text
}

function Get-KnowledgeBlockedError {
    param($Preflight)

    $knowledge = $Preflight.knowledge
    $dash = [string][char]0x2014
    $kinds = @($knowledge.blockers | ForEach-Object { $_.kind })
    $engineRelative = if (Test-Path (Join-Path $Preflight.repo_root '.devspark/scripts/build_knowledge_index.py')) { '.devspark/scripts/build_knowledge_index.py' } else { 'scripts/build_knowledge_index.py' }
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("Committed HEAD: $($knowledge.checked_head)")

    if ($kinds -contains 'build-error') {
        $message = "CREATE PR BLOCKED $dash KNOWLEDGE INDEX CANNOT BE BUILT"
        $lines.Add('Knowledge index: BUILD ERROR')
        foreach ($blocker in @($knowledge.blockers | Where-Object { $_.kind -eq 'build-error' })) { $lines.Add("Error: $($blocker.message)") }
        $lines.Add('Required action:')
        $lines.Add('1. Fix the reported source or frontmatter problem.')
        $lines.Add("2. Regenerate the Knowledge index: python $engineRelative --repo-root .")
        $lines.Add('3. Commit the .knowledge/** changes.')
        $lines.Add('4. Push.')
        $lines.Add('5. Rerun /devspark.create-pr.')
    } elseif ($kinds -contains 'drift') {
        $message = "CREATE PR BLOCKED $dash COMMITTED KNOWLEDGE FINALIZATION REQUIRED"
        $lines.Add('Knowledge index: DRIFT')
        foreach ($line in @($knowledge.check_output -split "`n" | Where-Object { $_.StartsWith('STALE:') })) { $lines.Add($line) }
        $lines.Add('Required action:')
        $lines.Add('1. Re-verify any genuinely stale Knowledge before changing last_verified.')
        $lines.Add("2. Regenerate the Knowledge index: python $engineRelative --repo-root .")
        $lines.Add('3. Review the resulting derived files.')
        $lines.Add('4. Commit the .knowledge/** changes.')
        $lines.Add('5. Push.')
        $lines.Add('6. Rerun /devspark.create-pr.')
    } else {
        $message = "CREATE PR BLOCKED $dash KNOWLEDGE RE-VERIFICATION REQUIRED"
        $lines.Add('Stale Knowledge touched by this PR:')
        foreach ($blocker in @($knowledge.blockers)) {
            $lines.Add("- $($blocker.path)")
            $lines.Add("  source: $($blocker.source)")
            $lines.Add("  source date: $($blocker.source_date)")
            $lines.Add("  last_verified: $($blocker.last_verified)")
        }
        $lines.Add('Required action:')
        $lines.Add('1. Confirm current truth, using /devspark-explain <topic> where appropriate.')
        $lines.Add('2. Change last_verified only after genuine verification.')
        $lines.Add("3. Regenerate: python $engineRelative --repo-root .")
        $lines.Add('4. Commit.')
        $lines.Add('5. Push.')
        $lines.Add('6. Rerun /devspark.create-pr.')
    }

    [PSCustomObject]@{
        error = $true
        message = $message
        details = ($lines -join "`n")
        checked_head = $knowledge.checked_head
        blockers = @($knowledge.blockers)
    } | ConvertTo-Json -Depth 10 -EscapeHandling EscapeNonAscii
}

function Get-CreatePrPreflight {
    $repoRoot = Get-RepoRoot
    $currentBranch = Get-CurrentBranch
    $targetBranch = if ($Base) { $Base } else { Get-DefaultBaseBranch }
    $dirty = [bool](git status --porcelain 2>$null)
    $localHead = ''
    $remoteHead = ''
    $originExists = $false
    $remoteBranchExists = $false
    $branchPushedToRemote = $false
    $branchPushDetails = ''

    try {
        $localHead = (git rev-parse HEAD 2>$null | Select-Object -First 1).Trim()
    } catch { }

    try {
        git remote get-url origin 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $originExists = $true
        }
    } catch { }

    if ($originExists) {
        try {
            $remoteLine = (git ls-remote --heads origin "refs/heads/$currentBranch" 2>$null | Select-Object -First 1)
            if ($remoteLine) {
                $remoteHead = (($remoteLine -split "`t")[0]).Trim()
                $remoteBranchExists = [bool]$remoteHead
            }
        } catch { }
    }

    if (-not $originExists) {
        $branchPushDetails = 'Remote ''origin'' is not configured'
    } elseif (-not $remoteBranchExists) {
        $branchPushDetails = "Branch '$currentBranch' has not been pushed to origin"
    } elseif ($localHead -and $remoteHead -and $localHead -eq $remoteHead) {
        $branchPushedToRemote = $true
        $branchPushDetails = "Branch '$currentBranch' is pushed to origin"
    } else {
        $branchPushDetails = "Push the latest commits from '$currentBranch' to origin before creating a PR"
    }

    $prCli = $DevSparkPlatform.PrCli
    $cliAvailable = [bool](Get-Command $prCli -ErrorAction SilentlyContinue)
    $authOk = $false
    if ($cliAvailable) {
        try { $authOk = & $DevSparkPlatform.AuthCheck } catch { $authOk = $false }
    }
    $creationSupported = $cliAvailable -and ($DevSparkPlatform.Name -in @('github', 'azdo'))

    $featureDir = Find-FeatureDirByPrefix -RepoRoot $repoRoot -BranchName $currentBranch
    $specPath = Join-Path $featureDir 'spec.md'
    $planPath = Join-Path $featureDir 'plan.md'
    $tasksPath = Join-Path $featureDir 'tasks.md'
    $checklistDir = Join-Path $featureDir 'checklists'

    $specExists = Test-Path $specPath
    $planExists = Test-Path $planPath
    $tasksExists = Test-Path $tasksPath

    $specTitle = ''
    $classification = ''
    $riskLevel = ''
    $requiredGates = ''
    $recommendedNextStep = ''
    if ($specExists) {
        $heading = Get-Content -LiteralPath $specPath -Encoding utf8 | Where-Object { $_ -match '^# ' } | Select-Object -First 1
        $specTitle = $heading -replace '^#\s*', ''
        $classification = Get-MarkdownFrontmatterValue -Path $specPath -Key 'classification'
        $riskLevel = Get-MarkdownFrontmatterValue -Path $specPath -Key 'risk_level'
        $requiredGates = Get-MarkdownFrontmatterValue -Path $specPath -Key 'required_gates'
        $recommendedNextStep = Get-MarkdownFrontmatterValue -Path $specPath -Key 'recommended_next_step'
    }

    $taskCounts = Get-TaskCounts -TasksPath $tasksPath
    $checklists = Get-ChecklistSummary -ChecklistDir $checklistDir
    $gateArtifacts = Get-GateArtifacts -FeatureDir $featureDir
    $gateAcknowledgements = Get-GateAcknowledgements -TasksPath $tasksPath
    $quickfixRecordPath = Find-QuickfixRecordForBranch -RepoRoot $repoRoot -BranchName $currentBranch
    $quickfixRecord = Get-QuickfixRecordSummary -QuickfixPath $quickfixRecordPath

    if (-not $specExists -and $quickfixRecord) {
        $specTitle = $quickfixRecord.title
        $classification = $quickfixRecord.classification
        $riskLevel = $quickfixRecord.risk_level
        $requiredGates = $quickfixRecord.required_gates
        $recommendedNextStep = $quickfixRecord.recommended_next_step
        if ($quickfixRecord.gate_acknowledgements) {
            $gateAcknowledgements = @($quickfixRecord.gate_acknowledgements.Trim())
        }
    }

    $diffRef = 'HEAD~1...HEAD'
    try {
        git rev-parse --verify "origin/$targetBranch" 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            git fetch origin $targetBranch 2>$null | Out-Null
            $diffRef = "origin/$targetBranch...HEAD"
        }
    } catch { }
    $linesSummary = (git diff --shortstat $diffRef 2>$null) -join ''
    $fileChanges = @(
        git diff --name-status $diffRef 2>$null |
        ForEach-Object {
            if ($_ -and $_ -match "`t") {
                $parts = $_ -split "`t"
                if ($parts.Count -ge 2) {
                    $st = $parts[0].Substring(0, 1)
                    if ($parts.Count -ge 3) {
                        [PSCustomObject]@{ status = $st; path = $parts[2].Trim(); from = $parts[1].Trim() }
                    } else {
                        [PSCustomObject]@{ status = $st; path = $parts[1].Trim() }
                    }
                }
            }
        } | Where-Object { $_ -ne $null }
    )
    $commitLog = @(
        git log --format="%H`u{001f}%s`u{001f}%an`u{001f}%ai" -n 20 $diffRef 2>$null |
        ForEach-Object {
            $parts = $_ -split "`u{001f}"
            if ($parts.Count -ge 4) {
                [PSCustomObject]@{
                    hash    = $parts[0].Trim()
                    subject = $parts[1].Trim()
                    author  = $parts[2].Trim()
                    date    = $parts[3].Trim()
                }
            }
        } | Where-Object { $_ -ne $null }
    )

    $changedPaths = @($fileChanges | ForEach-Object { $_.path; $_.from } | Where-Object { $_ })
    $knowledge = Get-KnowledgeConsistency -RepoRoot $repoRoot -Head $localHead -ChangedPaths $changedPaths

    $existingPr = [PSCustomObject]@{ exists = $false; number = $null; url = ''; title = ''; state = ''; draft = $false }
    if ($cliAvailable -and $authOk) {
        try {
            if ($DevSparkPlatform.Name -eq 'azdo') {
                $azctx = Get-AzdoRepoContext
                $orgUrl = "https://dev.azure.com/$($azctx.Org)"
                $azList = az repos pr list --organization $orgUrl --project $azctx.Project --repository $azctx.Repo --source-branch $currentBranch --status active --top 1 --output json 2>$null | ConvertFrom-Json
                $pr = @($azList)[0]
                if ($pr -and $pr.pullRequestId) {
                    $existingPr = [PSCustomObject]@{
                        exists = $true
                        number = $pr.pullRequestId
                        url    = "https://dev.azure.com/$($azctx.Org)/$($azctx.Project)/_git/$($azctx.Repo)/pullrequest/$($pr.pullRequestId)"
                        title  = $pr.title
                        state  = $pr.status
                        draft  = [bool]$pr.isDraft
                    }
                }
            } else {
                $existingRaw = gh pr list --head $currentBranch --json number,url,title,state,isDraft --limit 1 2>$null | ConvertFrom-Json
                $pr = @($existingRaw)[0]
                if ($pr) {
                    $existingPr = [PSCustomObject]@{
                        exists = $true
                        number = $pr.number
                        url = $pr.url
                        title = $pr.title
                        state = $pr.state
                        draft = [bool]$pr.isDraft
                    }
                }
            }
        } catch { }
    }

    return [PSCustomObject]@{
        repo_root = $repoRoot
        current_branch = $currentBranch
        target_branch = $targetBranch
        dirty_worktree = $dirty
        prerequisites = [PSCustomObject]@{
            clean_worktree = (-not $dirty)
            branch_pushed_to_remote = $branchPushedToRemote
        }
        remote = [PSCustomObject]@{
            name = 'origin'
            exists = $originExists
            branch_exists = $remoteBranchExists
            branch_pushed_to_remote = $branchPushedToRemote
            local_head = $localHead
            remote_head = $remoteHead
            details = $branchPushDetails
        }
        cli_available = $cliAvailable
        auth_ok = $authOk
        creation_supported = $creationSupported
        feature = [PSCustomObject]@{
            dir = $featureDir
            spec_path = $specPath
            plan_path = $planPath
            tasks_path = $tasksPath
            checklist_dir = $checklistDir
            spec_exists = $specExists
            plan_exists = $planExists
            tasks_exists = $tasksExists
            spec_title = $specTitle
            classification = $classification
            risk_level = $riskLevel
            required_gates = $requiredGates
            recommended_next_step = $recommendedNextStep
            tasks_total = $taskCounts.total
            tasks_completed = $taskCounts.completed
            tasks_incomplete = $taskCounts.incomplete
            checklists = $checklists
            gate_artifacts = $gateArtifacts
            gate_acknowledgements = @($gateAcknowledgements)
        }
        diff = [PSCustomObject]@{
            changed_files_count = $fileChanges.Count
            lines_summary       = $linesSummary
            commit_log          = $commitLog
            file_changes        = $fileChanges
        }
        quickfix_record = $quickfixRecord
        knowledge = $knowledge
        existing_pr = $existingPr
    }
}

function Invoke-CreateOrUpdatePr {
    param([ValidateSet('Create','Update')][string]$Action)

    $preflight = Get-CreatePrPreflight
    if (-not $preflight.prerequisites.clean_worktree) {
        return (Get-JsonError -Message 'Working tree has unmanaged changes' -Details 'Commit, stash, or discard all changes before creating or updating a PR')
    }
    if ($preflight.knowledge.readiness -eq 'blocked') {
        return (Get-KnowledgeBlockedError -Preflight $preflight)
    }
    if (-not $preflight.prerequisites.branch_pushed_to_remote) {
        return (Get-JsonError -Message 'Current branch is not pushed to remote' -Details $preflight.remote.details)
    }
    if (-not $preflight.creation_supported) {
        return (Get-JsonError -Message "Automated PR creation is not supported for platform '$($DevSparkPlatform.Name)'" -Details 'Supported: github (gh), azdo (az). Install and authenticate the platform CLI.')
    }
    if (-not $preflight.auth_ok) {
        $authHint = switch ($DevSparkPlatform.Name) { 'github' { 'Run: gh auth login' } 'azdo' { 'Run: az login' } default { "Authenticate the $($DevSparkPlatform.PrCli) CLI" } }
        return (Get-JsonError -Message "$($DevSparkPlatform.DisplayName) CLI is not authenticated" -Details $authHint)
    }

    $bodyValue = Resolve-PrBody
    if (-not $Title -or -not $bodyValue) {
        return (Get-JsonError -Message 'Title and body are required for create/update mode' -Details 'Pass -Title and -Body or -BodyFile')
    }

    if ($Action -eq 'Create' -and $preflight.existing_pr.exists) {
        return (Get-JsonError -Message 'A PR already exists for this branch' -Details 'Use update mode or pass -PrNumber')
    }

    $baseBranch = if ($Base) { $Base } else { $preflight.target_branch }

    if ($DevSparkPlatform.Name -eq 'azdo') {
        $azctx = Get-AzdoRepoContext
        $orgUrl = "https://dev.azure.com/$($azctx.Org)"
        if ($Action -eq 'Update') {
            $targetPr = if ($PrNumber) { $PrNumber } else { $preflight.existing_pr.number }
            if (-not $targetPr) { return (Get-JsonError -Message 'No PR found to update' -Details 'Create a PR first or pass -PrNumber') }
            az repos pr update --organization $orgUrl --id $targetPr --title $Title --description $bodyValue 2>$null | Out-Null
            foreach ($item in $Reviewer) { az repos pr reviewer add --organization $orgUrl --id $targetPr --reviewers $item 2>$null | Out-Null }
            $show = az repos pr show --organization $orgUrl --id $targetPr --output json 2>$null | ConvertFrom-Json
        } else {
            $azArgs = @('repos','pr','create','--organization',$orgUrl,'--project',$azctx.Project,'--repository',$azctx.Repo,'--source-branch',$preflight.current_branch,'--target-branch',$baseBranch,'--title',$Title,'--description',$bodyValue,'--output','json')
            if ($Draft) { $azArgs += @('--draft','true') }
            foreach ($item in $Reviewer) { $azArgs += @('--reviewers',$item) }
            $show = & az @azArgs 2>$null | ConvertFrom-Json
            if (-not $show -or -not $show.pullRequestId) { return (Get-JsonError -Message 'Failed to create pull request' -Details 'az repos pr create did not return a pull request id') }
        }
        $prId = $show.pullRequestId
        $view = [PSCustomObject]@{
            number  = $prId
            url     = "https://dev.azure.com/$($azctx.Org)/$($azctx.Project)/_git/$($azctx.Repo)/pullrequest/$prId"
            title   = $show.title
            state   = $show.status
            isDraft = [bool]$show.isDraft
        }
    } else {
        if ($Action -eq 'Update') {
            $targetPr = if ($PrNumber) { $PrNumber } else { $preflight.existing_pr.number }
            if (-not $targetPr) {
                return (Get-JsonError -Message 'No PR found to update' -Details 'Create a PR first or pass -PrNumber')
            }
            gh pr edit $targetPr --title $Title --body $bodyValue --base $baseBranch 2>$null | Out-Null
            foreach ($item in $Reviewer) { gh pr edit $targetPr --add-reviewer $item 2>$null | Out-Null }
            foreach ($item in $Label) { gh pr edit $targetPr --add-label $item 2>$null | Out-Null }
            foreach ($item in $Assignee) { gh pr edit $targetPr --add-assignee $item 2>$null | Out-Null }
            $view = gh pr view $targetPr --json number,url,title,state,isDraft 2>$null | ConvertFrom-Json
        } else {
            $ghCreateCommand = @('pr','create','--title',$Title,'--body',$bodyValue,'--base',$baseBranch)
            if ($Draft) { $ghCreateCommand += '--draft' }
            foreach ($item in $Reviewer) { $ghCreateCommand += @('--reviewer',$item) }
            foreach ($item in $Label) { $ghCreateCommand += @('--label',$item) }
            foreach ($item in $Assignee) { $ghCreateCommand += @('--assignee',$item) }
            $created = & gh @ghCreateCommand 2>$null
            $url = ($created | Select-Object -Last 1)
            if (-not $url) {
                return (Get-JsonError -Message 'Failed to create pull request' -Details 'gh pr create did not return a URL')
            }
            $view = gh pr view $url --json number,url,title,state,isDraft 2>$null | ConvertFrom-Json
        }
    }

    return ([PSCustomObject]@{
        status = 'ok'
        action = $Action.ToLower()
        checked_head = $preflight.remote.local_head
        pr_number = $view.number
        url = $view.url
        title = $view.title
        state = $view.state
        draft = [bool]$view.isDraft
    } | ConvertTo-Json -Depth 10)
}

switch ($Mode) {
    'Preflight' {
        $output = Get-CreatePrPreflight | ConvertTo-Json -Depth 10
        Write-Output $output
    }
    'Create' {
        Write-Output (Invoke-CreateOrUpdatePr -Action Create)
    }
    'Update' {
        Write-Output (Invoke-CreateOrUpdatePr -Action Update)
    }
}
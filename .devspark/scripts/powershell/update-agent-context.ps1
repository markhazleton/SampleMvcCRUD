#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
<#!
.SYNOPSIS
Update agent context files with information from plan.md (PowerShell version)

.DESCRIPTION
Mirrors the behavior of scripts/bash/update-agent-context.sh:
 1. Environment Validation
 2. Plan Data Extraction
 3. Agent File Management (create from template or update existing)
 4. Content Generation (technology stack, recent changes, timestamp)
 5. Multi-Agent Support (claude, gemini, copilot, cursor-agent, qwen, opencode, codex, windsurf, kilocode, auggie, roo, codebuddy, amp, shai, q, bob, qodercli, antigravity)

.PARAMETER AgentType
Optional agent key to update a single agent. If omitted, updates all existing agent files (creating a default Claude file if none exist).

.EXAMPLE
./update-agent-context.ps1 -AgentType claude

.EXAMPLE
./update-agent-context.ps1   # Updates all existing agent files

.NOTES
Relies on common helper functions in common.ps1
#>
param(
    [Parameter(Position=0)]
    [ValidateSet('claude','gemini','copilot','cursor-agent','qwen','opencode','codex','windsurf','kilocode','auggie','roo','codebuddy','amp','shai','q','bob','qodercli','antigravity')]
    [string]$AgentType
)

$ErrorActionPreference = 'Stop'

# Import common helpers
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $ScriptDir 'common.ps1')

# Multi-app support (T095d)
if (-not (Get-Command Detect-DevSparkMode -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/common.ps1"
}

# Acquire environment paths
$envData = Get-FeaturePathsEnv
$REPO_ROOT     = $envData.REPO_ROOT
$CURRENT_BRANCH = $envData.CURRENT_BRANCH
$HAS_GIT       = $envData.HAS_GIT
$IMPL_PLAN     = $envData.IMPL_PLAN
$NEW_PLAN = $IMPL_PLAN
$AGENT_REGISTRY_FILE = Join-Path $REPO_ROOT 'agents-registry.json'
$SHARED_AGENT_CONTEXT_FILE = Join-Path $REPO_ROOT 'AGENTS.md'
$SHARED_CONTEXT_START = '<!-- DEVSPARK SHARED CONTEXT:START -->'
$SHARED_CONTEXT_END = '<!-- DEVSPARK SHARED CONTEXT:END -->'

$TEMPLATE_FILE = Join-Path $REPO_ROOT '.devspark.work/templates/agent-file-template.md'
$LEGACY_TEMPLATE_FILE = Join-Path $REPO_ROOT 'templates/agent-file-template.md'

# Parsed plan data placeholders
$script:NEW_LANG = ''
$script:NEW_FRAMEWORK = ''
$script:NEW_DB = ''
$script:NEW_PROJECT_TYPE = ''

function Write-Info { 
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message
    )
    Write-Host "INFO: $Message" 
}

function Write-Success { 
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message
    )
    Write-Host "$([char]0x2713) $Message" 
}

function Write-WarningMsg { 
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message
    )
    Write-Warning $Message 
}

function Write-Err { 
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message
    )
    Write-Host "ERROR: $Message" -ForegroundColor Red 
}

function Get-AgentRegistry {
    if (-not (Test-Path $AGENT_REGISTRY_FILE)) {
        throw "Agent registry not found at $AGENT_REGISTRY_FILE"
    }
    return Get-Content -LiteralPath $AGENT_REGISTRY_FILE -Raw -Encoding utf8 | ConvertFrom-Json
}

function Get-AgentKeys {
    (Get-AgentRegistry).agents | ForEach-Object { $_.key }
}

function Get-AgentMetadata {
    param(
        [Parameter(Mandatory=$true)]
        [string]$AgentKey
    )

    return (Get-AgentRegistry).agents | Where-Object { $_.key -eq $AgentKey } | Select-Object -First 1
}

function Get-AgentTargetFile {
    param(
        [Parameter(Mandatory=$true)]
        [string]$AgentKey
    )

    $agent = Get-AgentMetadata -AgentKey $AgentKey
    if (-not $agent) {
        throw "Unknown agent type '$AgentKey'"
    }
    return Join-Path $REPO_ROOT $agent.context_file
}

function Update-SharedContextBlock {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TargetFile
    )

    if (-not (Test-Path $SHARED_AGENT_CONTEXT_FILE) -or $TargetFile -eq $SHARED_AGENT_CONTEXT_FILE) {
        return $true
    }

    $sharedContent = Get-Content -LiteralPath $SHARED_AGENT_CONTEXT_FILE -Raw -Encoding utf8
    $existingContent = Get-Content -LiteralPath $TargetFile -Raw -Encoding utf8
    $pattern = [Regex]::Escape($SHARED_CONTEXT_START) + '.*?' + [Regex]::Escape($SHARED_CONTEXT_END)
    $sanitized = [Regex]::Replace($existingContent, $pattern, '', [System.Text.RegularExpressions.RegexOptions]::Singleline).TrimEnd()
    $newContent = ($sanitized + [Environment]::NewLine + [Environment]::NewLine + $SHARED_CONTEXT_START + [Environment]::NewLine + $sharedContent.Trim() + [Environment]::NewLine + $SHARED_CONTEXT_END + [Environment]::NewLine)
    Set-Content -LiteralPath $TargetFile -Value $newContent -Encoding utf8
    return $true
}

function Validate-Environment {
    if (-not $CURRENT_BRANCH) {
        Write-Err 'Unable to determine current feature'
        if ($HAS_GIT) { Write-Info "Make sure you're on a feature branch" } else { Write-Info 'Set DEVSPARK_FEATURE environment variable or create a feature first' }
        exit 1
    }
    if (-not (Test-Path $NEW_PLAN)) {
        Write-Err "No plan.md found at $NEW_PLAN"
        Write-Info 'Ensure you are working on a feature with a corresponding spec directory'
        if (-not $HAS_GIT) { Write-Info 'Use: $env:DEVSPARK_FEATURE=your-feature-name or create a new feature first' }
        exit 1
    }
    if (-not (Test-Path $AGENT_REGISTRY_FILE)) {
        Write-Err "Agent registry not found at $AGENT_REGISTRY_FILE"
        exit 1
    }
    if (-not (Test-Path $TEMPLATE_FILE)) {
        if (Test-Path $LEGACY_TEMPLATE_FILE) {
            Write-WarningMsg "Primary template not found at $TEMPLATE_FILE; using legacy template at $LEGACY_TEMPLATE_FILE"
            $script:TEMPLATE_FILE = $LEGACY_TEMPLATE_FILE
        } else {
            Write-Err "Template file not found at $TEMPLATE_FILE"
            Write-Info "Legacy fallback also not found at $LEGACY_TEMPLATE_FILE"
            Write-Info 'Run the quickstart guide for your agent to scaffold .devspark.work/templates, or add agent-file-template.md there.'
            exit 1
        }
    }
    if (-not (Test-Path $SHARED_AGENT_CONTEXT_FILE)) {
        Write-WarningMsg "Shared agent context not found at $SHARED_AGENT_CONTEXT_FILE"
    }
}

function Extract-PlanField {
    param(
        [Parameter(Mandatory=$true)]
        [string]$FieldPattern,
        [Parameter(Mandatory=$true)]
        [string]$PlanFile
    )
    if (-not (Test-Path $PlanFile)) { return '' }
    # Lines like **Language/Version**: Python 3.12. The value may be hard-wrapped
    # onto following lines, so continuation lines are joined back onto one line.
    $regex = "^\*\*$([Regex]::Escape($FieldPattern))\*\*: (.+)$"
    $lines = @(Get-Content -LiteralPath $PlanFile -Encoding utf8)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -notmatch $regex) { continue }
        $val = $Matches[1].Trim()
        for ($j = $i + 1; $j -lt $lines.Count; $j++) {
            $next = $lines[$j]
            if ([string]::IsNullOrWhiteSpace($next)) { break }
            if ($next -match '^\s*(\*\*|#{1,6}\s|[-*+]\s|\d+\.\s|\||>)') { break }
            $val = "$val $($next.Trim())"
        }
        if ($val -in @('NEEDS CLARIFICATION','N/A')) { continue }
        return $val
    }
    return ''
}

function Parse-PlanData {
    param(
        [Parameter(Mandatory=$true)]
        [string]$PlanFile
    )
    if (-not (Test-Path $PlanFile)) { Write-Err "Plan file not found: $PlanFile"; return $false }
    Write-Info "Parsing plan data from $PlanFile"
    $script:NEW_LANG        = Extract-PlanField -FieldPattern 'Language/Version' -PlanFile $PlanFile
    $script:NEW_FRAMEWORK   = Extract-PlanField -FieldPattern 'Primary Dependencies' -PlanFile $PlanFile
    $script:NEW_DB          = Extract-PlanField -FieldPattern 'Storage' -PlanFile $PlanFile
    $script:NEW_PROJECT_TYPE = Extract-PlanField -FieldPattern 'Project Type' -PlanFile $PlanFile

    if ($NEW_LANG) { Write-Info "Found language: $NEW_LANG" } else { Write-WarningMsg 'No language information found in plan' }
    if ($NEW_FRAMEWORK) { Write-Info "Found framework: $NEW_FRAMEWORK" }
    if ($NEW_DB -and $NEW_DB -ne 'N/A') { Write-Info "Found database: $NEW_DB" }
    if ($NEW_PROJECT_TYPE) { Write-Info "Found project type: $NEW_PROJECT_TYPE" }
    return $true
}

function Format-TechnologyStack {
    param(
        [Parameter(Mandatory=$false)]
        [string]$Lang,
        [Parameter(Mandatory=$false)]
        [string]$Framework
    )
    $parts = @()
    if ($Lang -and $Lang -ne 'NEEDS CLARIFICATION') { $parts += $Lang }
    if ($Framework -and $Framework -notin @('NEEDS CLARIFICATION','N/A')) { $parts += $Framework }
    if (-not $parts) { return '' }
    return ($parts -join ' + ')
}

function Get-ProjectStructure { 
    param(
        [Parameter(Mandatory=$false)]
        [string]$ProjectType
    )
    if ($ProjectType -match 'web') { return "backend/`nfrontend/`ntests/" } else { return "src/`ntests/" } 
}

function Get-CommandsForLanguage { 
    param(
        [Parameter(Mandatory=$false)]
        [string]$Lang
    )
    switch -Regex ($Lang) {
        'Python' { return "cd src; pytest; ruff check ." }
        'Rust' { return "cargo test; cargo clippy" }
        'JavaScript|TypeScript' { return "npm test; npm run lint" }
        default { return "# Add commands for $Lang" }
    }
}

function Get-LanguageConventions { 
    param(
        [Parameter(Mandatory=$false)]
        [string]$Lang
    )
    if ($Lang) { "${Lang}: Follow standard conventions" } else { 'General: Follow standard conventions' } 
}

function Get-SafeReplacement {
    param(
        [Parameter(Mandatory=$false)]
        [string]$Value
    )
    # On the replacement side of -replace, '$' is special ($1, $&, $$, ${name}).
    # Double literal '$' to '$$' so dynamic values can't corrupt the output.
    if ($null -eq $Value) { return '' }
    return $Value.Replace('$','$$')
}

function New-AgentFile {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TargetFile,
        [Parameter(Mandatory=$true)]
        [string]$ProjectName,
        [Parameter(Mandatory=$true)]
        [datetime]$Date
    )
    if (-not (Test-Path $TEMPLATE_FILE)) { Write-Err "Template not found at $TEMPLATE_FILE"; return $false }
    $temp = New-TemporaryFile
    Copy-Item -LiteralPath $TEMPLATE_FILE -Destination $temp -Force

    $projectStructure = Get-ProjectStructure -ProjectType $NEW_PROJECT_TYPE
    $commands = Get-CommandsForLanguage -Lang $NEW_LANG
    $languageConventions = Get-LanguageConventions -Lang $NEW_LANG

    $escaped_lang = $NEW_LANG
    $escaped_framework = $NEW_FRAMEWORK
    $escaped_branch = $CURRENT_BRANCH

    $content = Get-Content -LiteralPath $temp -Raw -Encoding utf8
    $content = $content -replace '\[PROJECT NAME\]',(Get-SafeReplacement $ProjectName)
    $content = $content -replace '\[DATE\]',$Date.ToString('yyyy-MM-dd')
    
    # Build the technology stack string safely
    $techStackForTemplate = ""
    if ($escaped_lang -and $escaped_framework) {
        $techStackForTemplate = "- $escaped_lang + $escaped_framework ($escaped_branch)"
    } elseif ($escaped_lang) {
        $techStackForTemplate = "- $escaped_lang ($escaped_branch)"
    } elseif ($escaped_framework) {
        $techStackForTemplate = "- $escaped_framework ($escaped_branch)"
    }
    
    $content = $content -replace '\[EXTRACTED FROM ALL PLAN.MD FILES\]',(Get-SafeReplacement $techStackForTemplate)
    # For project structure we manually embed (keep newlines). [Regex]::Escape is a
    # *pattern*-side escaper (backslash-prefixes ".", "(", "$", spaces, etc.), so using
    # it here leaked stray backslashes into the rendered output. Instead: double '$'
    # for -replace safety, then turn real newlines into a literal "\n" marker that the
    # later '\\n' -> newline pass converts back — no other characters are touched.
    $escapedStructure = (Get-SafeReplacement $projectStructure) -replace "`n", '\n'
    $content = $content -replace '\[ACTUAL STRUCTURE FROM PLANS\]',$escapedStructure
    # Replace escaped newlines placeholder after all replacements
    $content = $content -replace '\[ONLY COMMANDS FOR ACTIVE TECHNOLOGIES\]',(Get-SafeReplacement $commands)
    $content = $content -replace '\[LANGUAGE-SPECIFIC, ONLY FOR LANGUAGES IN USE\]',(Get-SafeReplacement $languageConventions)
    
    # Build the recent changes string safely
    $recentChangesForTemplate = ""
    if ($escaped_lang -and $escaped_framework) {
        $recentChangesForTemplate = "- ${escaped_branch}: Added ${escaped_lang} + ${escaped_framework}"
    } elseif ($escaped_lang) {
        $recentChangesForTemplate = "- ${escaped_branch}: Added ${escaped_lang}"
    } elseif ($escaped_framework) {
        $recentChangesForTemplate = "- ${escaped_branch}: Added ${escaped_framework}"
    }
    
    $content = $content -replace '\[LAST 3 FEATURES AND WHAT THEY ADDED\]',(Get-SafeReplacement $recentChangesForTemplate)
    # Convert literal \n sequences introduced by Escape to real newlines
    $content = $content -replace '\\n',[Environment]::NewLine

    $parent = Split-Path -Parent $TargetFile
    if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent | Out-Null }
    Set-Content -LiteralPath $TargetFile -Value $content -NoNewline -Encoding utf8
    Remove-Item $temp -Force
    return $true
}

function Update-ExistingAgentFile {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TargetFile,
        [Parameter(Mandatory=$true)]
        [datetime]$Date
    )
    if (-not (Test-Path $TargetFile)) { return (New-AgentFile -TargetFile $TargetFile -ProjectName (Split-Path $REPO_ROOT -Leaf) -Date $Date) }

    $techStack = Format-TechnologyStack -Lang $NEW_LANG -Framework $NEW_FRAMEWORK
    $newTechEntries = @()
    if ($techStack) {
        $escapedTechStack = [Regex]::Escape($techStack)
        if (-not (Select-String -Pattern $escapedTechStack -Path $TargetFile -Quiet)) { 
            $newTechEntries += "- $techStack ($CURRENT_BRANCH)" 
        }
    }
    if ($NEW_DB -and $NEW_DB -notin @('N/A','NEEDS CLARIFICATION')) {
        $escapedDB = [Regex]::Escape($NEW_DB)
        if (-not (Select-String -Pattern $escapedDB -Path $TargetFile -Quiet)) { 
            $newTechEntries += "- $NEW_DB ($CURRENT_BRANCH)" 
        }
    }
    $newChangeEntry = ''
    if ($techStack) { $newChangeEntry = "- ${CURRENT_BRANCH}: Added ${techStack}" }
    elseif ($NEW_DB -and $NEW_DB -notin @('N/A','NEEDS CLARIFICATION')) { $newChangeEntry = "- ${CURRENT_BRANCH}: Added ${NEW_DB}" }

    $lines = Get-Content -LiteralPath $TargetFile -Encoding utf8
    $output = New-Object System.Collections.Generic.List[string]
    $inTech = $false; $inChanges = $false; $techAdded = $false; $existingChanges = 0

    # Appends a blank line unless the buffer already ends with one, so generated
    # headings and lists always stay surrounded by blanks (MD022/MD032/MD012).
    $addSeparator = {
        if ($output.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($output[$output.Count - 1])) { $output.Add('') }
    }

    for ($i=0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -eq '## Active Technologies') {
            $output.Add($line)
            $inTech = $true
            continue
        }
        if ($inTech -and $line -match '^##\s') {
            if (-not $techAdded -and $newTechEntries.Count -gt 0) {
                & $addSeparator
                $newTechEntries | ForEach-Object { $output.Add($_) }
                $techAdded = $true
            }
            & $addSeparator
            $output.Add($line); $inTech = $false; continue
        }
        if ($inTech -and [string]::IsNullOrWhiteSpace($line)) {
            if (-not $techAdded -and $newTechEntries.Count -gt 0) {
                & $addSeparator
                $newTechEntries | ForEach-Object { $output.Add($_) }
                $techAdded = $true
                continue
            }
            & $addSeparator
            continue
        }
        if ($inTech -and $line -notmatch '^- ') {
            if (-not $techAdded -and $newTechEntries.Count -gt 0) {
                & $addSeparator
                $newTechEntries | ForEach-Object { $output.Add($_) }
                $techAdded = $true
            }
            & $addSeparator
            $output.Add($line); $inTech = $false; continue
        }
        if ($line -eq '## Recent Changes') {
            $output.Add($line)
            if ($newChangeEntry) {
                $output.Add('')
                $output.Add($newChangeEntry)
            }
            $inChanges = $true
            continue
        }
        if ($inChanges -and [string]::IsNullOrWhiteSpace($line)) { continue }
        if ($inChanges -and $line -match '^- ') {
            if ($existingChanges -lt 2) {
                & $addSeparator
                $output.Add($line); $existingChanges++
            }
            continue
        }
        if ($inChanges) {
            & $addSeparator
            $output.Add($line); $inChanges = $false; continue
        }
        if ($line -match '\*\*Last updated\*\*: .*\d{4}-\d{2}-\d{2}') {
            $output.Add(($line -replace '\d{4}-\d{2}-\d{2}',$Date.ToString('yyyy-MM-dd')))
            continue
        }
        $output.Add($line)
    }

    # Post-loop check: if we're still in the Active Technologies section and haven't added new entries
    if ($inTech -and -not $techAdded -and $newTechEntries.Count -gt 0) {
        & $addSeparator
        $newTechEntries | ForEach-Object { $output.Add($_) }
    }

    Set-Content -LiteralPath $TargetFile -Value ($output -join [Environment]::NewLine) -Encoding utf8
    return $true
}

function Update-AgentFile {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TargetFile,
        [Parameter(Mandatory=$true)]
        [string]$AgentName
    )
    if (-not $TargetFile -or -not $AgentName) { Write-Err 'Update-AgentFile requires TargetFile and AgentName'; return $false }
    Write-Info "Updating $AgentName context file: $TargetFile"
    $projectName = Split-Path $REPO_ROOT -Leaf
    $date = Get-Date

    $dir = Split-Path -Parent $TargetFile
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }

    if (-not (Test-Path $TargetFile)) {
        if (New-AgentFile -TargetFile $TargetFile -ProjectName $projectName -Date $date) { Write-Success "Created new $AgentName context file" } else { Write-Err 'Failed to create new agent file'; return $false }
    } else {
        try {
            if (Update-ExistingAgentFile -TargetFile $TargetFile -Date $date) { Write-Success "Updated existing $AgentName context file" } else { Write-Err 'Failed to update agent file'; return $false }
        } catch {
            Write-Err "Cannot access or update existing file: $TargetFile. $_"
            return $false
        }
    }
    if (-not (Update-SharedContextBlock -TargetFile $TargetFile)) {
        Write-Err "Failed to refresh shared context block in $TargetFile"
        return $false
    }
    return $true
}

function Update-SpecificAgent {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Type
    )
    $metadata = Get-AgentMetadata -AgentKey $Type
    if (-not $metadata) {
        Write-Err "Unknown agent type '$Type'"
        Write-Err "Expected: $((Get-AgentKeys) -join '|')"
        return $false
    }
    return (Update-AgentFile -TargetFile (Get-AgentTargetFile -AgentKey $Type) -AgentName $metadata.name)
}

function Update-AllExistingAgents {
    $found = $false
    $ok = $true
    $seenTargets = @{}
    foreach ($agentKey in Get-AgentKeys) {
        $metadata = Get-AgentMetadata -AgentKey $agentKey
        $targetFile = Get-AgentTargetFile -AgentKey $agentKey
        if ($seenTargets.ContainsKey($targetFile)) {
            continue
        }
        $seenTargets[$targetFile] = $true
        if (Test-Path $targetFile) {
            if (-not (Update-AgentFile -TargetFile $targetFile -AgentName $metadata.name)) { $ok = $false }
            $found = $true
        }
    }
    if (-not $found) {
        Write-Info 'No existing agent files found, creating default Claude file...'
        if (-not (Update-AgentFile -TargetFile (Join-Path $REPO_ROOT 'CLAUDE.md') -AgentName 'Claude Code')) { $ok = $false }
    }
    return $ok
}

function Print-Summary {
    Write-Host ''
    Write-Info 'Summary of changes:'
    if ($NEW_LANG) { Write-Host "  - Added language: $NEW_LANG" }
    if ($NEW_FRAMEWORK) { Write-Host "  - Added framework: $NEW_FRAMEWORK" }
    if ($NEW_DB -and $NEW_DB -ne 'N/A') { Write-Host "  - Added database: $NEW_DB" }
    Write-Host ''
    Write-Info "Usage: ./update-agent-context.ps1 [-AgentType $((Get-AgentKeys) -join '|')]"
}

function Main {
    Validate-Environment
    Write-Info "=== Updating agent context files for feature $CURRENT_BRANCH ==="
    if (-not (Parse-PlanData -PlanFile $NEW_PLAN)) { Write-Err 'Failed to parse plan data'; exit 1 }
    $success = $true
    if ($AgentType) {
        Write-Info "Updating specific agent: $AgentType"
        if (-not (Update-SpecificAgent -Type $AgentType)) { $success = $false }
    }
    else {
        Write-Info 'No agent specified, updating all existing agent files...'
        if (-not (Update-AllExistingAgents)) { $success = $false }
    }
    Print-Summary
    if ($success) { Write-Success 'Agent context update completed successfully'; exit 0 } else { Write-Err 'Agent context update completed with errors'; exit 1 }
}

Main


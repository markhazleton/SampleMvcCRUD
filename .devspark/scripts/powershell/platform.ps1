#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Platform detection and adapter for BSW.DevSpark scripts
# Detects GitHub, Azure DevOps, or GitLab and exports platform-specific values.
#
# Usage: . "$PSScriptRoot/platform.ps1"
#        Then use $DevSparkPlatform.Name, $DevSparkPlatform.PrCli, etc.
#
# Override: Set DEVSPARK_PLATFORM env var to force a platform (github|azdo|gitlab)
# Config:   Or set "platform" in .devspark.work/devspark.json

# Load common if not already loaded
if (-not (Get-Command Get-RepoRoot -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/common.ps1"
}

function Detect-Platform {
    # 1. Explicit env var override
    if ($env:DEVSPARK_PLATFORM) {
        return $env:DEVSPARK_PLATFORM.ToLower()
    }

    # 2. Config file override
    $repoRoot = Get-RepoRoot
    $configFile = Join-Path $repoRoot '.devspark.work/devspark.json'
    if (Test-Path $configFile) {
        try {
            $config = Get-Content $configFile -Raw | ConvertFrom-Json
            if ($config.platform) {
                return $config.platform.ToLower()
            }
        } catch {
            # Invalid JSON, continue detection
        }
    }

    # 3. CI environment variable detection
    if ($env:GITHUB_ACTIONS -or $env:GITHUB_REPOSITORY) {
        return 'github'
    }
    if ($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI -or $env:BUILD_REPOSITORY_PROVIDER) {
        return 'azdo'
    }
    if ($env:GITLAB_CI -or $env:CI_PROJECT_ID) {
        return 'gitlab'
    }

    # 4. Remote URL detection (authoritative host signal; beats local folder layout).
    #    A .github/ folder is common even on Azure DevOps repos (Copilot shims), so
    #    the remote must be checked before repository-structure heuristics.
    try {
        $remoteUrl = git remote get-url origin 2>$null
        if ($LASTEXITCODE -eq 0 -and $remoteUrl) {
            if ($remoteUrl -match 'github\.com') { return 'github' }
            if ($remoteUrl -match 'dev\.azure\.com|visualstudio\.com') { return 'azdo' }
            if ($remoteUrl -match 'gitlab\.com|gitlab\.' ) { return 'gitlab' }
        }
    } catch { }

    # 5. Repository structure detection (fallback when no remote is configured).
    if (Test-Path (Join-Path $repoRoot '.github')) {
        return 'github'
    }
    if (Test-Path (Join-Path $repoRoot 'azure-pipelines.yml')) {
        return 'azdo'
    }
    if (Test-Path (Join-Path $repoRoot '.gitlab-ci.yml')) {
        return 'gitlab'
    }

    return 'github'  # Default fallback
}

function Get-PlatformConfig {
    param([string]$PlatformName)

    switch ($PlatformName) {
        'github' {
            [PSCustomObject]@{
                Name             = 'github'
                DisplayName      = 'GitHub'
                PrCli            = 'gh'
                PrCliInstallUrl  = 'https://cli.github.com/'
                CiDir            = '.github/workflows'
                CiFilePattern    = '*.yml'
                AgentConfigPath  = 'AGENTS.md'
                BranchNameLimit  = 244
                PrEnvVar         = 'GITHUB_PR_NUMBER'
                AuthCheck        = { gh auth status 2>$null; $LASTEXITCODE -eq 0 }
            }
        }
        'azdo' {
            [PSCustomObject]@{
                Name             = 'azdo'
                DisplayName      = 'Azure DevOps'
                PrCli            = 'az'
                PrCliInstallUrl  = 'https://learn.microsoft.com/en-us/cli/azure/install-azure-cli'
                CiDir            = '.'
                CiFilePattern    = 'azure-pipelines*.yml'
                AgentConfigPath  = 'AGENTS.md'
                BranchNameLimit  = 250
                PrEnvVar         = 'SYSTEM_PULLREQUEST_PULLREQUESTID'
                AuthCheck        = { az account show 2>$null | Out-Null; $LASTEXITCODE -eq 0 }
            }
        }
        'gitlab' {
            [PSCustomObject]@{
                Name             = 'gitlab'
                DisplayName      = 'GitLab'
                PrCli            = 'glab'
                PrCliInstallUrl  = 'https://gitlab.com/gitlab-org/cli#installation'
                CiDir            = '.'
                CiFilePattern    = '.gitlab-ci.yml'
                AgentConfigPath  = 'AGENTS.md'
                BranchNameLimit  = 255
                PrEnvVar         = 'CI_MERGE_REQUEST_IID'
                AuthCheck        = { glab auth status 2>$null; $LASTEXITCODE -eq 0 }
            }
        }
        default {
            throw "Unknown platform: $PlatformName. Supported: github, azdo, gitlab"
        }
    }
}

# The lowest azure-devops CLI extension version this repository has verified against a real
# project for every field the work-item helper writes. Its Bash twin declares the same value
# and the script parity test fails when the two drift.
$script:DevSparkWorkItemMinExtensionVersion = '1.0.5'

# Compare two dotted numeric versions. Returns -1, 0 or 1. Missing components count as zero,
# so '1.0' and '1.0.0' compare equal. Non-numeric components sort as zero rather than throwing,
# because a preview suffix must not turn a capability probe into a crash.
function Compare-DevSparkVersion {
    param([string]$Left, [string]$Right)
    $l = @($Left -split '[.\-+]')
    $r = @($Right -split '[.\-+]')
    $count = [Math]::Max($l.Count, $r.Count)
    for ($i = 0; $i -lt $count; $i++) {
        $lv = 0; $rv = 0
        if ($i -lt $l.Count) { [void][int]::TryParse($l[$i], [ref]$lv) }
        if ($i -lt $r.Count) { [void][int]::TryParse($r[$i], [ref]$rv) }
        if ($lv -lt $rv) { return -1 }
        if ($lv -gt $rv) { return 1 }
    }
    return 0
}

# Probe the Azure DevOps work-item capability. Reports three facts independently so a caller
# can tell "no CLI" from "no extension" from "extension too old" and name the right remedy.
function Get-AzdoWorkItemCapability {
    $cliPresent = $null -ne (Get-Command az -ErrorAction SilentlyContinue)
    $version = ''
    $extensionPresent = $false

    if ($cliPresent) {
        $raw = az extension show --name azure-devops --query version -o tsv 2>$null
        if ($LASTEXITCODE -eq 0 -and $raw) {
            $version = "$raw".Trim()
            $extensionPresent = -not [string]::IsNullOrWhiteSpace($version)
        }
    }

    $meetsMinimum = $false
    if ($extensionPresent) {
        $meetsMinimum = (Compare-DevSparkVersion -Left $version -Right $script:DevSparkWorkItemMinExtensionVersion) -ge 0
    }

    return [PSCustomObject]@{
        CliPresent       = $cliPresent
        ExtensionPresent = $extensionPresent
        ExtensionVersion = $version
        MinimumVersion   = $script:DevSparkWorkItemMinExtensionVersion
        MeetsMinimum     = $meetsMinimum
    }
}

# Auto-detect and export on source
$script:DevSparkPlatformName = Detect-Platform
$script:DevSparkPlatform = Get-PlatformConfig -PlatformName $script:DevSparkPlatformName

# Resolve script path: team override in .devspark.work/scripts/ takes priority
function Resolve-DevSparkScript {
    param(
        [Parameter(Mandatory)]
        [string]$ScriptName,
        [ValidateSet('powershell', 'bash')]
        [string]$Shell = 'powershell'
    )

    $repoRoot = Get-RepoRoot
    $teamPath = Join-Path $repoRoot ".devspark.work/scripts/$Shell/$ScriptName"
    $stockPath = Join-Path $repoRoot ".devspark/scripts/$Shell/$ScriptName"
    $devPath = Join-Path $repoRoot "scripts/$Shell/$ScriptName"

    if (Test-Path $teamPath)  { return $teamPath }
    if (Test-Path $stockPath) { return $stockPath }
    if (Test-Path $devPath)   { return $devPath }

    return $null
}

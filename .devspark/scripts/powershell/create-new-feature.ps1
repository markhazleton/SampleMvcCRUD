#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Create a new feature
[CmdletBinding(PositionalBinding = $false)]
param(
    [switch]$Json,
    [string]$ShortName,
    [int]$Number = 0,
    [switch]$Help,
    [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
    [string[]]$FeatureDescription
)
$ErrorActionPreference = 'Stop'

# Show help if requested
if ($Help) {
    Write-Host "Usage: ./create-new-feature.ps1 [-Json] [-ShortName <name>] [-Number N] <feature description>"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Json               Output in JSON format"
    Write-Host "  -ShortName <name>   Provide a custom short name (2-4 words) for the branch"
    Write-Host "  -Number N           Specify branch number manually (overrides auto-detection)"
    Write-Host "  -Help               Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  ./create-new-feature.ps1 'Add user authentication system' -ShortName 'user-auth'"
    Write-Host "  ./create-new-feature.ps1 'Implement OAuth2 integration for API'"
    exit 0
}

# Check if feature description provided
if ((-not $FeatureDescription -or $FeatureDescription.Count -eq 0) -and -not $ShortName) {
    Write-Error "Usage: ./create-new-feature.ps1 [-Json] [-ShortName <name>] <feature description>"
    exit 1
}

$scriptStart = Get-Date

. "$PSScriptRoot/common.ps1"

# Delegate to the single branch-creation choke point. Branch Safety is confirmed
# by the caller (for example, /devspark-specify) before this script runs, so -Yes avoids a double-prompt
# for a decision the developer already made.
# Splat a hashtable, not an array: array splatting binds positionally, and
# new-branch.ps1 sets PositionalBinding=$false, so every token would be
# swallowed by its ValueFromRemainingArguments -Description parameter.
$newBranchParams = @{ Type = 'spec'; Json = $true; Yes = $true }
if ($ShortName) { $newBranchParams['ShortName'] = $ShortName }
if ($Number -gt 0) { $newBranchParams['Number'] = $Number }
if ($FeatureDescription) { $newBranchParams['Description'] = $FeatureDescription }

$newBranchOutput = & "$PSScriptRoot/new-branch.ps1" @newBranchParams
$exitCode = $LASTEXITCODE
if ($exitCode -ne 0) {
    Write-Error ($newBranchOutput -join "`n")
    exit $exitCode
}

$result = $newBranchOutput | ConvertFrom-Json

# Knowledge document:
# only the feature-overview doc is emitted here -- spec.md is still the empty
# template stub at this point (the agent drafts real content afterward), so
# Requirement-document emission is deferred to setup-plan (see plan.md Implementation Notes).
# Failure here must never affect this script's JSON contract or exit code.
if ($result.ARTIFACT_PATH) {
    New-KnowledgeDocument -FeatureDir (Split-Path $result.ARTIFACT_PATH -Parent) `
        -Id 'feature' -Type 'feature' -Title $result.BRANCH_NAME -Status 'draft' `
        -Links @{ requirements = @() } -GeneratedBy 'create-new-feature' | Out-Null
}

# Preserve the original JSON contract (BRANCH_NAME, SPEC_FILE, FEATURE_NUM, HAS_GIT).
if ($Json) {
    $obj = [PSCustomObject]@{
        contract    = 1
        BRANCH_NAME = $result.BRANCH_NAME
        SPEC_FILE   = $result.ARTIFACT_PATH
        FEATURE_NUM = $result.NUMBER
        HAS_GIT     = $result.HAS_GIT
    }
    $obj | ConvertTo-Json
} else {
    Write-Output "BRANCH_NAME: $($result.BRANCH_NAME)"
    Write-Output "SPEC_FILE: $($result.ARTIFACT_PATH)"
    Write-Output "FEATURE_NUM: $($result.NUMBER)"
    Write-Output "HAS_GIT: $($result.HAS_GIT)"
    Write-Output "DEVSPARK_FEATURE environment variable set to: $($result.BRANCH_NAME)"
}

$durationMs = [int]((Get-Date) - $scriptStart).TotalMilliseconds
Add-DevSparkMetric -Command 'create-new-feature' -Status 'success' -DurationMs $durationMs


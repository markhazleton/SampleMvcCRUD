#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Validates requirement<->task coverage across a feature's OKF knowledge
# documents . See
# contracts/coverage-report-contract.md in that spec for the JSON shape.

[CmdletBinding()]
param(
    [switch]$Json,
    [string]$Feature,
    [switch]$Help
)

$ErrorActionPreference = 'Stop'

if ($Help) {
    Write-Output "Usage: validate-knowledge-coverage.ps1 [-Json] [-Feature <feature-dir>]"
    Write-Output "  -Json              Output in JSON format"
    Write-Output "  -Feature <path>    Feature directory to check (defaults to the current feature)"
    exit 0
}

. "$PSScriptRoot/common.ps1"

if (-not $Feature) {
    $paths = Get-FeaturePathsEnv
    $Feature = $paths.FEATURE_DIR
}

function Test-LinksArrayEmpty {
    param([string]$Frontmatter, [string]$Key)
    if ($Frontmatter -match "(?m)^  ${Key}:\s*\[\]\s*$") { return $true }
    if ($Frontmatter -match "(?m)^  ${Key}:\s*$") {
        $idx = $Frontmatter.IndexOf("  ${Key}:")
        $after = $Frontmatter.Substring($idx)
        if ($after -match '(?m)^    - \S+') { return $false }
        return $true
    }
    return $true
}

$knowledgeDir = Join-Path $Feature 'knowledge'
$orphanTasks = @()
$unimplementedRequirements = @()
$requirementIds = @()
$taskCount = 0

if (Test-Path $knowledgeDir) {
    Get-ChildItem -Path $knowledgeDir -Filter '*.md' | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        if ($content -notmatch '(?s)^---\r?\n(.*?)\r?\n---') { return }
        $frontmatter = $Matches[1]

        $typeMatch = [regex]::Match($frontmatter, '(?m)^type:\s*(\S+)')
        if (-not $typeMatch.Success) { return }
        $type = $typeMatch.Groups[1].Value

        $idMatch = [regex]::Match($frontmatter, '(?m)^id:\s*(\S+)')
        $id = if ($idMatch.Success) { $idMatch.Groups[1].Value } else { $_.BaseName }

        if ($type -eq 'requirement') {
            $requirementIds += $id
            if (Test-LinksArrayEmpty -Frontmatter $frontmatter -Key 'implemented_by') {
                $unimplementedRequirements += $id
            }
        } elseif ($type -eq 'task') {
            $taskCount++
            if (Test-LinksArrayEmpty -Frontmatter $frontmatter -Key 'implements') {
                $orphanTasks += $id
            }
        }
    }
}

# An unimplemented requirement is only a finding once at
# least one task document exists for the feature.
if ($taskCount -eq 0) { $unimplementedRequirements = @() }

$result = [PSCustomObject]@{
    contract                    = 1
    feature_id                  = (Split-Path $Feature -Leaf)
    orphan_tasks                = @($orphanTasks)
    unimplemented_requirements  = @($unimplementedRequirements)
    ok                          = ($orphanTasks.Count -eq 0 -and $unimplementedRequirements.Count -eq 0)
}

if ($Json) {
    $result | ConvertTo-Json -Compress
} else {
    Write-Output "feature_id: $($result.feature_id)"
    Write-Output "orphan_tasks: $($result.orphan_tasks -join ', ')"
    Write-Output "unimplemented_requirements: $($result.unimplemented_requirements -join ', ')"
    Write-Output "ok: $($result.ok)"
}

exit 0

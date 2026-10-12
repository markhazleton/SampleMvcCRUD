#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
#requires -Version 7.0
# Helper script for /devspark.address-pr-review
# -PrId: parse review file and emit open findings JSON
# -Gate: enforce staged-path isolation for code-only or review-only commits

param(
    [Parameter()]
    [string]$PrId,

    [Parameter()]
    [ValidateSet('code-only', 'review-only')]
    [string]$Gate,

    [Parameter()]
    [switch]$Json
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command Get-RepoRoot -ErrorAction SilentlyContinue)) {
    . (Join-Path $PSScriptRoot 'common.ps1')
}

function Get-StagedPaths {
    try {
        $paths = @(git diff --cached --name-only 2>$null | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        return $paths
    }
    catch {
        return @()
    }
}

function Test-IsReviewPath {
    param([string]$Path)
    return $Path -match '^\.devspark.work/pr-review/pr-.*\.md$'
}

function Write-GateFailure {
    param(
        [string]$Message,
        [string[]]$OffendingPaths
    )

    Write-Error "BSW.DevSpark: $Message"
    if ($OffendingPaths.Count -gt 0) {
        Write-Error "Offending staged paths:"
        foreach ($item in $OffendingPaths) {
            Write-Error "  - $item"
        }
    }
    exit 1
}

if ($Gate) {
    $staged = Get-StagedPaths

    if ($Gate -eq 'code-only') {
        $reviewStaged = @($staged | Where-Object { Test-IsReviewPath $_ })
        if ($reviewStaged.Count -gt 0) {
            Write-GateFailure -Message 'Code commit gate failed. Review files must not be staged for code-only commits.' -OffendingPaths $reviewStaged
        }
    }

    if ($Gate -eq 'review-only') {
        $nonReviewStaged = @($staged | Where-Object { -not (Test-IsReviewPath $_) })
        if ($nonReviewStaged.Count -gt 0) {
            Write-GateFailure -Message 'Review commit gate failed. Only PR review markdown files may be staged.' -OffendingPaths $nonReviewStaged
        }
    }

    if (-not $Json) {
        Write-Output "Gate '$Gate' passed."
    }
    exit 0
}

if (-not $PrId) {
    Write-Error 'BSW.DevSpark: Provide -PrId <N> or -Gate <code-only|review-only>.'
    exit 1
}

$normalizedPrId = ($PrId -replace '^#', '').Trim()
if ($normalizedPrId -notmatch '^\d+$') {
    Write-Error "BSW.DevSpark: Invalid PR id '$PrId'. Expected a positive integer."
    exit 1
}

$repoRoot = Get-RepoRoot
$reviewFile = Join-Path $repoRoot ".devspark.work/pr-review/pr-$normalizedPrId.md"

if (-not (Test-Path -LiteralPath $reviewFile)) {
    Write-Error "BSW.DevSpark: Review file not found: $reviewFile"
    exit 1
}

$lines = Get-Content -LiteralPath $reviewFile -Encoding utf8
$openFindings = New-Object System.Collections.Generic.List[object]
$section = ''
$contrary = "(accepted|deferred|waived|withdrawn|wontfix|won't fix)"

# Auto eligibility comes from status, severity and blocking evidence; an ID prefix alone never grants it.
for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -match '^#+\s+(.*)$') {
        $section = $matches[1]
        continue
    }
    if ($line -match '^\s*-\s*\[\s\]\s+\*\*((C|H|M|L|CON|PRD\d+)-\d{2,})\*\*') {
        $findingId = $matches[1]
        $severity = if ($matches[2] -like 'PRD*') { 'PRD' } else { $matches[2] }

        $status = 'open'
        $row = $lines | Where-Object { $_ -match ('^\|\s*' + [regex]::Escape($findingId) + '\s*\|') } | Select-Object -Last 1
        if ($row) {
            $statusCell = ($row -split '\|')[2].ToLowerInvariant()
            if ($statusCell -match 'resolved|withdrawn') { $status = 'closed' }
            elseif ($statusCell -match 'partial') { $status = 'partial' }
        }

        $sectionLc = $section.ToLowerInvariant()
        $blocking = ($severity -eq 'C') -or ($sectionLc -match 'blocking' -and $sectionLc -notmatch 'non-blocking')

        $route = 'report'
        $eligible = $false
        if ($severity -eq 'CON') {
            $route = 'evolve-constitution'
            $reason = 'Constitution findings route to /devspark-evolve-constitution, not to product code.'
        }
        elseif ($status -ne 'open') {
            $reason = "Status is $status; a human decides whether it still needs work."
        }
        elseif ($line.ToLowerInvariant() -match $contrary) {
            $reason = 'The finding records an accepted, deferred or withdrawn disposition.'
        }
        elseif ($severity -in @('C', 'H')) {
            $route = 'auto'; $eligible = $true; $reason = "Open $severity finding."
        }
        elseif ($severity -eq 'PRD' -and $blocking) {
            $route = 'auto'; $eligible = $true; $reason = 'Open blocking PR-delta finding.'
        }
        elseif ($severity -eq 'PRD') {
            $reason = 'PR-delta finding without blocking evidence.'
        }
        else {
            $reason = 'Ordinary Medium or Low findings are report-only.'
        }

        $openFindings.Add([PSCustomObject]@{
            id = $findingId
            severity = $severity
            line_number = $i + 1
            line = $line.Trim()
            section = $section
            status = $status
            blocking = [bool]$blocking
            auto_eligible = $eligible
            route = $route
            reason = $reason
        })
    }
}

$autoCount = @($openFindings | Where-Object { $_.auto_eligible }).Count
$result = [PSCustomObject]@{
    pr_id = [int]$normalizedPrId
    review_file = $reviewFile
    open_findings = $openFindings
    open_count = $openFindings.Count
    auto_eligible_count = $autoCount
}

if ($Json) {
    $result | ConvertTo-Json -Depth 4
}
else {
    Write-Output "Review file: $reviewFile"
    Write-Output "Open findings: $($openFindings.Count)"
    Write-Output "Auto-eligible findings: $autoCount"
    foreach ($finding in $openFindings) {
        Write-Output "- $($finding.id) [$($finding.route)]"
    }
}

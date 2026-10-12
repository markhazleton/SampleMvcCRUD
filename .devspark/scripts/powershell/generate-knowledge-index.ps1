#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com

param(
    [string]$RepoRoot = '',
    [string]$Output = '',
    [string]$GeneratedAt = '',
    [switch]$Stdout,
    [switch]$FailOnStale,
    [switch]$DetectDrift,
    [switch]$FullInventory,
    [string]$Base = '',
    [string]$Head = '',
    [string]$ReportOutput = '',
    [int]$BreadthGuidance = 0,
    [switch]$MigrateSourceClaims,
    [switch]$PruneBaselines,
    [switch]$Write,
    [switch]$CurrencyReport,
    [int]$CurrencyDays = 0
)

. (Join-Path $PSScriptRoot 'common.ps1')

if (-not $RepoRoot) { $RepoRoot = Get-RepoRoot }
$engine = Join-Path $PSScriptRoot '../build_knowledge_index.py'
$arguments = @($engine, '--repo-root', $RepoRoot)
if ($Output) { $arguments += @('--output', $Output) }
if ($GeneratedAt) { $arguments += @('--generated-at', $GeneratedAt) }
if ($Stdout) { $arguments += '--stdout' }
if ($FailOnStale) { $arguments += '--fail-on-stale' }
if ($DetectDrift) { $arguments += '--detect-drift' }
if ($FullInventory) { $arguments += '--full-inventory' }
if ($Base) { $arguments += @('--base', $Base) }
if ($Head) { $arguments += @('--head', $Head) }
if ($ReportOutput) { $arguments += @('--report-output', $ReportOutput) }
if ($BreadthGuidance -gt 0) { $arguments += @('--breadth-guidance', $BreadthGuidance) }
if ($MigrateSourceClaims) { $arguments += '--migrate-source-claims' }
if ($PruneBaselines) { $arguments += '--prune-baselines' }
if ($Write) { $arguments += '--write' }
if ($CurrencyReport) { $arguments += '--currency-report' }
if ($CurrencyDays -gt 0) { $arguments += @('--currency-days', $CurrencyDays) }

& python @arguments
exit $LASTEXITCODE

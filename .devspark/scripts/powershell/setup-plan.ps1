#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Setup implementation plan for a feature

[CmdletBinding()]
param(
    [switch]$Json,
    [switch]$Reset,
    [switch]$Help
)

$ErrorActionPreference = 'Stop'

# Show help if requested
if ($Help) {
    Write-Output "Usage: ./setup-plan.ps1 [-Json] [-Reset] [-Help]"
    Write-Output "  -Json     Output results in JSON format"
    Write-Output "  -Reset    Explicitly replace the existing plan with the resolved template"
    Write-Output "  -Help     Show this help message"
    exit 0
}

# Load common functions
. "$PSScriptRoot/common.ps1"

# Multi-app support
if (-not (Get-Command Detect-DevSparkMode -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/common.ps1"
}

# Get all paths and variables from common functions
$paths = Get-FeaturePathsEnv

# Check if we're on a proper feature branch (only for git repos)
if (-not (Test-FeatureBranch -Branch $paths.CURRENT_BRANCH -HasGit $paths.HAS_GIT)) {
    exit 1
}

# Ensure the feature directory exists
New-Item -ItemType Directory -Path $paths.FEATURE_DIR -Force | Out-Null

# Copy plan template from override/stock fallback chain if it exists
$template = Resolve-TemplatePath -RepoRoot $paths.REPO_ROOT -TemplateName 'plan-template.md'
$planExists = Test-Path $paths.IMPL_PLAN
$planMode = if ($Reset) { 'reset' } elseif ($planExists) { 'revise' } else { 'create' }
if ($planExists -and -not $Reset) {
    if (-not $Json) { Write-Output "Preserved existing plan at $($paths.IMPL_PLAN)" }
} elseif ($template -and (Test-Path $template)) {
    Copy-Item $template $paths.IMPL_PLAN -Force
    Write-Output "Copied plan template to $($paths.IMPL_PLAN)"
} else {
    Write-Warning '[devspark] No plan-template.md found in fallback chain; creating empty plan.md'
    # Create a basic plan file if template doesn't exist
    New-Item -ItemType File -Path $paths.IMPL_PLAN -Force | Out-Null
}

# Knowledge documents: parse
# functional requirements out of the now-populated spec.md and emit/refresh
# knowledge/fr-###.md + knowledge/feature.md's links.requirements. Gated by a
# git hash-object staleness check (research.md) stored as an internal comment
# in feature.md -- never part of the OKF frontmatter contract. Best-effort:
# any failure here must not affect this script's JSON output or exit code.
try {
    if (Test-Path $paths.FEATURE_SPEC) {
        $specHash = $null
        try { $specHash = (git hash-object $paths.FEATURE_SPEC 2>$null) } catch {}
        $featureDocPath = Join-Path $paths.FEATURE_DIR 'knowledge/feature.md'
        $priorHash = $null
        if (Test-Path $featureDocPath) {
            $existing = Get-Content $featureDocPath -Raw
            if ($existing -match '(?m)^<!-- spec-hash: (\S+) -->') { $priorHash = $Matches[1] }
        }
        if (-not $specHash -or $priorHash -ne $specHash) {
            $specContent = Get-Content $paths.FEATURE_SPEC -Raw
            $frIds = @()
            [regex]::Matches($specContent, '(?m)^-\s+\*\*(FR-\d+)\*\*:\s*(.+)$') | ForEach-Object {
                $frId = $_.Groups[1].Value.ToLower()
                # First-sentence heuristic: split on ". " followed by an uppercase letter,
                # but not when preceded by a common abbreviation (e.g., i.e., etc.) so
                # titles aren't truncated mid-abbreviation (PR #61822 M-02).
                $frTitle = ([regex]::Split($_.Groups[2].Value.Trim(), '(?<!e\.g)(?<!i\.e)(?<!etc)\.\s+(?=[A-Z])'))[0].TrimEnd('.')
                $frIds += $frId
                New-KnowledgeDocument -FeatureDir $paths.FEATURE_DIR -Id $frId -Type 'requirement' `
                    -Title $frTitle -Status 'draft' -Links @{ feature = 'feature'; implemented_by = @() } `
                    -GeneratedBy 'setup-plan' | Out-Null
            }
            $featureTitle = $paths.CURRENT_BRANCH
            if ($specContent -match '(?m)^# Feature Specification:\s*(.+)$') { $featureTitle = $Matches[1].Trim() }
            # Reference (never copy) the spec sections that carry consumed spec knowledge; a
            # section the spec lacks is recorded as absent so no reference is left dangling.
            $sectionPresent = @()
            $sectionAbsent = @()
            foreach ($section in @('Requirements', 'Key Entities', 'Clarifications', 'Success Criteria', 'Assumptions', 'Out of Scope', 'Minimum Sufficient Change')) {
                if ($specContent -match ('(?mi)^#{2,3}\s+' + [regex]::Escape($section) + '(\s|$|\*)')) { $sectionPresent += $section } else { $sectionAbsent += $section }
            }
            $wrote = New-KnowledgeDocument -FeatureDir $paths.FEATURE_DIR -Id 'feature' -Type 'feature' `
                -Title $featureTitle -Status 'draft' -GeneratedBy 'setup-plan' `
                -Links @{ requirements = $frIds; spec_sections = $sectionPresent; spec_sections_absent = $sectionAbsent }
            if ($wrote -and $specHash -and (Test-Path $featureDocPath)) {
                # Below the closing frontmatter delimiter: above it, the YAML block stops being
                # recognised and markdownlint reads the delimiters as setext headings.
                $lines = @(Get-Content $featureDocPath)
                $close = -1
                for ($i = 1; $i -lt $lines.Count; $i++) {
                    if ($lines[$i].TrimEnd() -eq '---') { $close = $i; break }
                }
                if ($close -ge 0) {
                    $out = @($lines[0..$close]) + @('', "<!-- spec-hash: $specHash -->")
                    if ($close + 1 -lt $lines.Count) { $out += $lines[($close + 1)..($lines.Count - 1)] }
                    Set-Content -Path $featureDocPath -Value $out -Encoding UTF8
                }
            }
        }
    }
} catch {
    Write-Warning "DevSpark: knowledge document refresh failed: $($_.Exception.Message)"
}

# Output results
if ($Json) {
    $result = [PSCustomObject]@{
        FEATURE_SPEC = $paths.FEATURE_SPEC
        IMPL_PLAN = $paths.IMPL_PLAN
        SPECS_DIR = $paths.FEATURE_DIR
        BRANCH = $paths.CURRENT_BRANCH
        HAS_GIT = $paths.HAS_GIT
        PLAN_EXISTS = $planExists
        PLAN_MODE = $planMode
    }
    $result | ConvertTo-Json
} else {
    Write-Output "FEATURE_SPEC: $($paths.FEATURE_SPEC)"
    Write-Output "IMPL_PLAN: $($paths.IMPL_PLAN)"
    Write-Output "SPECS_DIR: $($paths.FEATURE_DIR)"
    Write-Output "BRANCH: $($paths.CURRENT_BRANCH)"
    Write-Output "HAS_GIT: $($paths.HAS_GIT)"
    Write-Output "PLAN_EXISTS: $planExists"
    Write-Output "PLAN_MODE: $planMode"
}

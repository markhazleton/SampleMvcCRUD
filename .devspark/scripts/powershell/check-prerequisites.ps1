#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com

# Consolidated prerequisite checking script (PowerShell)
#
# This script provides unified prerequisite checking for Spec-Driven Development workflow.
# It replaces the functionality previously spread across multiple scripts.
#
# Usage: ./check-prerequisites.ps1 [OPTIONS]
#
# OPTIONS:
#   -Json               Output in JSON format
#   -RequirePlan        Require plan.md to exist (full-spec routes only; quick-spec/quickfix have no plan.md)
#   -RequireTasks       Require tasks.md to exist (for implementation phase)
#   -IncludeTasks       Include tasks.md in AVAILABLE_DOCS list
#   -PathsOnly          Only output path variables (no validation)
#   -Help, -h           Show help message

[CmdletBinding()]
param(
    [switch]$Json,
    [switch]$RequirePlan,
    [switch]$RequireTasks,
    [switch]$IncludeTasks,
    [switch]$PathsOnly,
    [int]$TimeoutSeconds = 300,
    [switch]$Help
)

$ErrorActionPreference = 'Stop'
$scriptStart = Get-Date

# Show help if requested
if ($Help) {
    Write-Output @"
Usage: check-prerequisites.ps1 [OPTIONS]

Consolidated prerequisite checking for Spec-Driven Development workflow.

OPTIONS:
  -Json               Output in JSON format
  -RequirePlan        Require plan.md to exist (full-spec routes only)
  -RequireTasks       Require tasks.md to exist (for implementation phase)
  -IncludeTasks       Include tasks.md in AVAILABLE_DOCS list
  -PathsOnly          Only output path variables (no prerequisite validation)
    -TimeoutSeconds     Timeout value reported in diagnostics (default: 300)
  -Help, -h           Show this help message

EXAMPLES:
  # Check task prerequisites (plan.md required, full-spec route)
  .\check-prerequisites.ps1 -Json -RequirePlan
  
  # Check implementation prerequisites (plan.md + tasks.md required)
  .\check-prerequisites.ps1 -Json -RequirePlan -RequireTasks -IncludeTasks
  
  # Degrade gracefully for quick-spec/quickfix routes (no plan.md/tasks.md required)
  .\check-prerequisites.ps1 -Json -IncludeTasks
  
  # Get feature paths only (no validation)
  .\check-prerequisites.ps1 -PathsOnly

"@
    exit 0
}

# Source common functions
. "$PSScriptRoot/common.ps1"

# Multi-app support (T095f)
if (-not (Get-Command Detect-DevSparkMode -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/common.ps1"
}

# Get feature paths and validate branch
$paths = Get-FeaturePathsEnv

if (-not (Test-FeatureBranch -Branch $paths.CURRENT_BRANCH -HasGit:$paths.HAS_GIT)) { 
    exit 1 
}

# If paths-only mode, output paths and exit (support combined -Json -PathsOnly)
if ($PathsOnly) {
    if ($Json) {
        [PSCustomObject]@{
            REPO_ROOT    = $paths.REPO_ROOT
            BRANCH       = $paths.CURRENT_BRANCH
            FEATURE_DIR  = $paths.FEATURE_DIR
            FEATURE_SPEC = $paths.FEATURE_SPEC
            IMPL_PLAN    = $paths.IMPL_PLAN
            TASKS        = $paths.TASKS
        } | ConvertTo-Json
    } else {
        Write-Output "REPO_ROOT: $($paths.REPO_ROOT)"
        Write-Output "BRANCH: $($paths.CURRENT_BRANCH)"
        Write-Output "FEATURE_DIR: $($paths.FEATURE_DIR)"
        Write-Output "FEATURE_SPEC: $($paths.FEATURE_SPEC)"
        Write-Output "IMPL_PLAN: $($paths.IMPL_PLAN)"
        Write-Output "TASKS: $($paths.TASKS)"
    }
    exit 0
}

# Validate required directories and files
if (-not (Test-Path $paths.FEATURE_DIR -PathType Container)) {
    Write-Output "ERROR: Feature directory not found: $($paths.FEATURE_DIR)"
    Write-Output "Run /devspark-specify first to create the feature structure."
    exit 1
}

# Check for plan.md if required (full-spec routes only; quick-spec/quickfix never produce plan.md)
if ($RequirePlan -and -not (Test-Path $paths.IMPL_PLAN -PathType Leaf)) {
    Write-Output "ERROR: plan.md not found in $($paths.FEATURE_DIR)"
    Write-Output "Run /devspark-plan first to create the implementation plan."
    exit 1
}

# Check for tasks.md if required
if ($RequireTasks -and -not (Test-Path $paths.TASKS -PathType Leaf)) {
    Write-Output "ERROR: tasks.md not found in $($paths.FEATURE_DIR)"
    Write-Output "Run /devspark-tasks first to create the task list."
    exit 1
}

# Build list of available documents
$docs = @()

# Always check these optional docs
if (Test-Path $paths.RESEARCH) { $docs += 'research.md' }
if (Test-Path $paths.DATA_MODEL) { $docs += 'data-model.md' }

# Check contracts directory (only if it exists and has files)
if ((Test-Path $paths.CONTRACTS_DIR) -and (Get-ChildItem -Path $paths.CONTRACTS_DIR -ErrorAction SilentlyContinue | Select-Object -First 1)) { 
    $docs += 'contracts/' 
}

if (Test-Path $paths.QUICKSTART) { $docs += 'quickstart.md' }

# Include tasks.md if requested and it exists
if ($IncludeTasks -and (Test-Path $paths.TASKS)) { 
    $docs += 'tasks.md' 
}

# Checklist status (M-03: scripts compute, models judge -- exact counts, not LLM arithmetic)
$checklistSummary = @()
$checklistsDir = Join-Path $paths.FEATURE_DIR 'checklists'
if (Test-Path $checklistsDir -PathType Container) {
    Get-ChildItem -Path $checklistsDir -Filter '*.md' -File -ErrorAction SilentlyContinue | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $total = ([regex]::Matches($content, '(?m)^\s*-\s*\[[xX ]\]')).Count
        $completed = ([regex]::Matches($content, '(?m)^\s*-\s*\[[xX]\]')).Count
        $checklistSummary += [PSCustomObject]@{
            name       = $_.Name
            total      = $total
            completed  = $completed
            incomplete = $total - $completed
            status     = if (($total - $completed) -eq 0) { 'PASS' } else { 'FAIL' }
        }
    }
}
$checklistsOverall = if (($checklistSummary | Where-Object { $_.incomplete -gt 0 }).Count -gt 0) { 'FAIL' } else { 'PASS' }

# Output results
if ($Json) {
    # JSON output
    [PSCustomObject]@{ 
        contract = 1
        FEATURE_DIR = $paths.FEATURE_DIR
        AVAILABLE_DOCS = $docs 
        CHECKLISTS = $checklistSummary
        CHECKLISTS_OVERALL = $checklistsOverall
    } | ConvertTo-Json -Depth 5
} else {
    # Text output
    Write-Output "FEATURE_DIR:$($paths.FEATURE_DIR)"
    Write-Output "AVAILABLE_DOCS:"
    
    # Show status of each potential document
    Test-FileExists -Path $paths.RESEARCH -Description 'research.md' | Out-Null
    Test-FileExists -Path $paths.DATA_MODEL -Description 'data-model.md' | Out-Null
    Test-DirHasFiles -Path $paths.CONTRACTS_DIR -Description 'contracts/' | Out-Null
    Test-FileExists -Path $paths.QUICKSTART -Description 'quickstart.md' | Out-Null
    
    if ($IncludeTasks) {
        Test-FileExists -Path $paths.TASKS -Description 'tasks.md' | Out-Null
    }

    if ($checklistSummary.Count -gt 0) {
        Write-Output "CHECKLISTS:"
        foreach ($c in $checklistSummary) {
            Write-Output "  $($c.name): total=$($c.total) completed=$($c.completed) incomplete=$($c.incomplete) status=$($c.status)"
        }
        Write-Output "CHECKLISTS_OVERALL: $checklistsOverall"
    }
}

$durationMs = [int]((Get-Date) - $scriptStart).TotalMilliseconds
Add-DevSparkMetric -Command 'check-prerequisites' -Status 'success' -DurationMs $durationMs -Extra @{
    include_tasks = [bool]$IncludeTasks
    require_tasks = [bool]$RequireTasks
}

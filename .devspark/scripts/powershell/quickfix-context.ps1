#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Quickfix context gathering script
# Supports rapid bug fixes and small features without full spec overhead

param(
    [Parameter(Position = 0, ValueFromRemainingArguments)]
    [string[]]$Arguments,
    [switch]$Json
)

. (Join-Path $PSScriptRoot 'common.ps1')

# Multi-app support
if (-not (Get-Command Detect-DevSparkMode -ErrorAction SilentlyContinue)) {
    . "$PSScriptRoot/common.ps1"
}

# Parse arguments
$action = "create"
$quickfixId = ""
$description = @()

foreach ($arg in $Arguments) {
    switch -Regex ($arg) {
        "^complete$" { $action = "complete" }
        "^list$" { $action = "list" }
        "^QF-" { $quickfixId = $arg }
        default { $description += $arg }
    }
}
$descriptionText = $description -join " "

# Get repository context
$repoRoot = Get-RepoRoot
$constitutionPath = Join-Path $repoRoot ".knowledge/governance/constitution.md"
$quickfixDir = Join-Path $repoRoot ".devspark.work/quickfixes"
$currentBranch = Get-CurrentBranch

# Check constitution exists
$constitutionExists = Test-Path $constitutionPath

# Quickfix branch and record naming flows
# through the unified allocator + slug normalizer (same ones new-branch.ps1
# uses), producing NNN-fix-<slug> instead of the legacy QF-YYYY-NNN. The year
# is retained only as metadata inside the record body. Legacy QF-YYYY-NNN records remain readable via the
# unified allocator's scan of both formats.
$year = Get-Date -Format "yyyy"
$slugSource = if ($descriptionText) { $descriptionText } else { 'quickfix' }
$slug = ConvertTo-CleanBranchName -Name $slugSource
$slugParts = @($slug -split '-' | Where-Object { $_ } | Select-Object -First 4)
$slug = ($slugParts -join '-')
if (-not $slug) { $slug = 'quickfix' }

$nextNumber = Get-NextUnifiedIndex
$featureNum = ('{0:000}' -f $nextNumber)
$branchName = "$featureNum-fix-$slug"
$nextId = $branchName

# Detect git availability (needed to create the branch and commit the record)
$hasGit = [bool](Test-HasGit)

# Detect a push target so the branch can be published for team visibility
$hasRemote = $false
if ($hasGit) {
    try {
        $remotes = git remote 2>$null
        $hasRemote = [bool]($remotes | Where-Object { $_ -and $_.Trim() })
    } catch {
        $hasRemote = $false
    }
}

# Get git user for attribution
$gitUser = "unknown"
if ($hasGit) {
    try {
        $gitUser = git config user.name 2>$null
        if (-not $gitUser) { $gitUser = "unknown" }
    } catch {
        $gitUser = "unknown"
    }
}

# Get timestamp
$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

# Auto-classify based on description keywords
$classification = "minor-feature"
$riskLevel = "LOW"
$maxEffort = "4 hours"

if ($descriptionText) {
    $descLower = $descriptionText.ToLower()

    if ($descLower -match "(urgent|critical|emergency|production|hotfix)") {
        $classification = "hotfix"
        $riskLevel = "HIGH"
        $maxEffort = "2 hours"
    }
    elseif ($descLower -match "(fix|bug|error|crash|broken|issue|null|exception)") {
        $classification = "bug-fix"
        $riskLevel = "MEDIUM"
        $maxEffort = "4 hours"
    }
    elseif ($descLower -match "(config|setting|environment|flag|env)") {
        $classification = "config-change"
        $riskLevel = "LOW"
        $maxEffort = "1 hour"
    }
    elseif ($descLower -match "(doc|readme|comment|documentation)") {
        $classification = "docs-update"
        $riskLevel = "LOW"
        $maxEffort = "2 hours"
    }
}

# List existing quickfixes
$quickfixes = @()
if (Test-Path $quickfixDir) {
    $quickfixes = Get-ChildItem -Path $quickfixDir -Filter "*.md" -ErrorAction SilentlyContinue |
        ForEach-Object { $_.BaseName }
}

# Output
if ($Json) {
    @{
        REPO_ROOT           = $repoRoot
        CONSTITUTION_PATH   = $constitutionPath
        CONSTITUTION_EXISTS = $constitutionExists
        QUICKFIX_DIR        = $quickfixDir
        CURRENT_BRANCH      = $currentBranch
        NEXT_ID             = $nextId
        BRANCH_NAME         = $branchName
        NUMBER              = $featureNum
        SLUG                = $slug
        HAS_GIT             = $hasGit
        HAS_REMOTE          = $hasRemote
        GIT_USER            = $gitUser
        TIMESTAMP           = $timestamp
        ACTION              = $action
        QUICKFIX_ID         = $quickfixId
        DESCRIPTION         = $descriptionText
        CLASSIFICATION      = $classification
        RISK_LEVEL          = $riskLevel
        MAX_EFFORT          = $maxEffort
        QUICKFIXES          = $quickfixes
    } | ConvertTo-Json
}
else {
    Write-Output "Quickfix Context"
    Write-Output "================"
    Write-Output "Repository: $repoRoot"
    Write-Output "Constitution: $constitutionPath (exists: $constitutionExists)"
    Write-Output "Quickfix Directory: $quickfixDir"
    Write-Output "Current Branch: $currentBranch"
    Write-Output "Next ID: $nextId"
    Write-Output "Branch Name: $branchName"
    Write-Output "Number: $featureNum"
    Write-Output "Slug: $slug"
    Write-Output "Action: $action"
    if ($descriptionText) {
        Write-Output "Description: $descriptionText"
        Write-Output "Classification: $classification"
        Write-Output "Risk Level: $riskLevel"
        Write-Output "Max Effort: $maxEffort"
    }
}

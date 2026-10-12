#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Common PowerShell functions analogous to common.sh

function Get-RepoRoot {
    try {
        $result = git rev-parse --show-toplevel 2>$null
        if ($LASTEXITCODE -eq 0) {
            return $result
        }
    } catch {
        # Git command failed
    }

    # Fall back to script location for non-git repos
    return (Resolve-Path (Join-Path $PSScriptRoot "../../..")).Path
}

# Single canonical resolver for build_knowledge_index.py. The framework-managed copy under
# .devspark/scripts/ always wins when present, so every caller (CI, site-audit, create-pr,
# get-pr-context, release-context, /devspark-explain) agrees on one implementation. The repo-root
# copy is used only when .devspark/scripts/ has none -- BSW.DevSpark's own self-hosted repo, where
# scripts/ IS the canonical source packaged into .devspark/scripts/ for every installed repository.
function Resolve-KnowledgeEngine {
    param([string]$RepoRoot)
    $devsparkCopy = Join-Path $RepoRoot '.devspark/scripts/build_knowledge_index.py'
    if (Test-Path $devsparkCopy) { return $devsparkCopy }
    $rootCopy = Join-Path $RepoRoot 'scripts/build_knowledge_index.py'
    if (Test-Path $rootCopy) { return $rootCopy }
    return $null
}

# Canonical installed-version lookup. Every artifact that records its own DevSpark provenance
# resolves the version here instead of re-implementing stamp parsing.
function Get-DevSparkVersion {
    param(
        [string]$RepoRoot,
        [switch]$IncludeRevision
    )

    if (-not $RepoRoot) { $RepoRoot = Get-RepoRoot }

    $version = 'unknown'
    $stamp = Join-Path $RepoRoot '.devspark/BSW.DevSpark.version'
    $legacy = Join-Path $RepoRoot '.documentation/DEVSPARK_VERSION'

    foreach ($candidate in @($stamp, $legacy)) {
        if (-not (Test-Path $candidate)) { continue }
        foreach ($line in (Get-Content $candidate -ErrorAction SilentlyContinue)) {
            if ($line -match '^\s*version:\s*(\S+)') { $version = $Matches[1]; break }
        }
        if ($version -eq 'unknown') {
            # Legacy stamps may hold a bare semver with no key.
            $bare = (Get-Content $candidate -Raw -ErrorAction SilentlyContinue)
            if ($bare -and $bare.Trim() -match '^v?\d+\.\d+\.\d+') { $version = $bare.Trim() }
        }
        if ($version -ne 'unknown') { break }
    }

    $result = [ordered]@{ VERSION = $version }

    if ($IncludeRevision) {
        $revision = ''
        try {
            $revision = (git -C $RepoRoot rev-parse --short HEAD 2>$null)
            if ($LASTEXITCODE -ne 0) { $revision = '' }
        } catch {
            $revision = ''
        }
        if ($revision) { $result['REVISION'] = $revision.Trim() }
    }

    return [pscustomobject]$result
}

function Get-DefaultDocTaxon {
    param(
        [string]$RelativePath,
        [string]$Content
    )

    $normalizedPath = $RelativePath -replace '\\', '/'
    $deprecatedPattern = 'pydantic_agent|AGENT_REGISTRY|REPO_MODE_AGENTS|data_field|function_name|display_card_id'

    if (
        $normalizedPath -match '^docs/' -or
        ($normalizedPath -match '^\.documentation/' -and $Content -match $deprecatedPattern)
    ) {
        return 'STALE_REFERENCE'
    }

    switch -Regex ($normalizedPath) {
        '^\.knowledge/(reference|reference-data)/' { return 'REFERENCE_DATA' }
        '^\.knowledge/(integrations|operations|scripts)/' { return 'OPERATIONS_RUNBOOK' }
        '^\.knowledge/legal/' { return 'AUTHORITATIVE_REFERENCE' }
        '^\.knowledge/plans/' { return 'STALE_REFERENCE' }
        '^CHANGELOG\.md$' { return 'HISTORICAL_RECORD' }
        '^\.github/copilot-instructions\.md$' { return 'AUTHORITATIVE_REFERENCE' }
        '^\.knowledge/governance/' { return 'AUTHORITATIVE_REFERENCE' }
        '^\.knowledge/architecture/' { return 'ENGINEERING_PATTERN' }
        '^\.devspark\.work/releases/' { return 'RESEARCH_OR_CONTEXT' }
        '^\.devspark\.work/quickfixes/' { return 'RESEARCH_OR_CONTEXT' }
        '^\.devspark\.work/pr-review/' { return 'RESEARCH_OR_CONTEXT' }
        '^\.devspark\.work/audit/' { return 'RESEARCH_OR_CONTEXT' }
        '^\.devspark\.work/improvement-loop/' { return 'RESEARCH_OR_CONTEXT' }
        '^\.knowledge/reference/' { return 'REFERENCE_DATA' }
        '^\.devspark\.work/(templates|commands)/' { return 'ENGINEERING_PATTERN' }
        '^\.devspark\.work/scripts/' { return 'OPERATIONS_RUNBOOK' }
        '^\.documentation/' { return 'AUTHORITATIVE_REFERENCE' }
        default { return 'AUTHORITATIVE_REFERENCE' }
    }
}

function Resolve-DocTaxon {
    param(
        [string]$RepoRoot,
        [string]$RelativePath,
        [string]$Content
    )

    if (-not $RepoRoot) { $RepoRoot = Get-RepoRoot }
    $normalizedPath = $RelativePath -replace '\\', '/'

    if ($Content -match '(?m)^type:\s*["'']?([a-z][a-z0-9-]*)') {
        return $matches[1].ToUpperInvariant().Replace('-', '_')
    }

    $registryPath = Join-Path $RepoRoot '.knowledge/taxonomy-registry.json'
    if (Test-Path $registryPath) {
        try {
            $registry = Get-Content -Raw -Path $registryPath | ConvertFrom-Json
            foreach ($mapping in @($registry.mappings)) {
                $pattern = [string]$mapping.pathPattern
                if ($pattern -and $normalizedPath -like $pattern) {
                    if ($mapping.nodeType) {
                        return ([string]$mapping.nodeType).ToUpperInvariant().Replace('-', '_')
                    }
                    if ($mapping.workProductType) {
                        return ([string]$mapping.workProductType).ToUpperInvariant().Replace('-', '_')
                    }
                }
            }
        } catch {
            throw "Invalid taxonomy registry '$registryPath': $($_.Exception.Message)"
        }
    }

    return Get-DefaultDocTaxon -RelativePath $normalizedPath -Content $Content
}

function Get-CurrentBranch {
    # First check if DEVSPARK_FEATURE environment variable is set
    if ($env:DEVSPARK_FEATURE) {
        return $env:DEVSPARK_FEATURE
    }

    # Then check git if available
    try {
        $result = git rev-parse --abbrev-ref HEAD 2>$null
        if ($LASTEXITCODE -eq 0) {
            return $result
        }
    } catch {
        # Git command failed
    }

    # For non-git repos, try to find the latest feature directory
    $repoRoot = Get-RepoRoot
    $specsDir = Join-Path $repoRoot ".devspark.work/specs"

    if (Test-Path $specsDir) {
        $latestFeature = ""
        $highest = 0

        Get-ChildItem -Path $specsDir -Directory | ForEach-Object {
            if ($_.Name -match '^(\d{3})-') {
                $num = [int]$matches[1]
                if ($num -gt $highest) {
                    $highest = $num
                    $latestFeature = $_.Name
                }
            }
        }

        if ($latestFeature) {
            return $latestFeature
        }
    }

    # Final fallback
    return "main"
}

function Get-NextUnifiedIndex {
    # Unified branch index: the max integer across ALL numbered
    # branches, spec directories, AND quickfix records, plus 1. Derived live
    # from the repository -- never a stored counter. For any input set with no
    # quickfix records this returns the same value as the legacy
    # Get-NextBranchNumber (the verify:snapshot-neutral equivalence invariant).
    param([switch]$NoFetch)

    $repoRoot = Get-RepoRoot
    if (-not $NoFetch) {
        # Never block on a credential prompt (offline-safe); fail fast instead.
        $prev = $env:GIT_TERMINAL_PROMPT
        $env:GIT_TERMINAL_PROMPT = '0'
        try { git -c credential.interactive=false fetch --all --prune 2>$null | Out-Null }
        catch { }
        finally { $env:GIT_TERMINAL_PROMPT = $prev }
    }

    $highest = 0

    # 1. Branches (local + remote), pattern ^(\d+)-
    try {
        $branches = git branch -a 2>$null
        if ($LASTEXITCODE -eq 0) {
            foreach ($b in $branches) {
                $clean = $b.Trim() -replace '^\*?\s+', '' -replace '^remotes/[^/]+/', ''
                if ($clean -match '^(\d+)-') {
                    $n = [int]$matches[1]
                    if ($n -gt $highest) { $highest = $n }
                }
            }
        }
    } catch {
        Write-Verbose "Get-NextUnifiedIndex: branch scan failed: $_"
    }

    # 2. Spec directories, pattern ^(\d+)
    $specsDir = Join-Path $repoRoot '.devspark.work/specs'
    if (Test-Path $specsDir) {
        Get-ChildItem -Path $specsDir -Directory -ErrorAction SilentlyContinue | ForEach-Object {
            if ($_.Name -match '^(\d+)') {
                $n = [int]$matches[1]
                if ($n -gt $highest) { $highest = $n }
            }
        }
    }

    # 3. Quickfix records: legacy QF-YYYY-NNN and new NNN-fix-*
    $qfDir = Join-Path $repoRoot '.devspark.work/quickfixes'
    if (Test-Path $qfDir) {
        Get-ChildItem -Path $qfDir -Filter '*.md' -ErrorAction SilentlyContinue | ForEach-Object {
            if ($_.BaseName -match '^QF-\d{4}-(\d+)$') {
                $n = [int]$matches[1]
                if ($n -gt $highest) { $highest = $n }
            } elseif ($_.BaseName -match '^(\d+)-fix-') {
                $n = [int]$matches[1]
                if ($n -gt $highest) { $highest = $n }
            }
        }
    }

    return ($highest + 1)
}

function ConvertTo-CleanBranchName {
    # Canonical short-name normalizer: lowercase, non-alphanumerics to
    # hyphens, collapse repeats, trim leading/trailing hyphens.
    param([string]$Name)
    return $Name.ToLower() -replace '[^a-z0-9]', '-' -replace '-{2,}', '-' -replace '^-', '' -replace '-$', ''
}

function Test-IndexCollision {
    # Collision guard: true if this 3-digit index is already owned by a
    # branch (local/remote) or a spec directory.
    param([Parameter(Mandatory)][int]$Number, [string]$RepoRoot)
    if (-not $RepoRoot) { $RepoRoot = Get-RepoRoot }
    $prefix = ('{0:000}-' -f $Number)

    $specsDir = Join-Path $RepoRoot '.devspark.work/specs'
    if (Test-Path $specsDir) {
        if (@(Get-ChildItem -Path $specsDir -Directory -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -like "$prefix*" }).Count -gt 0) { return $true }
    }

    try {
        $branches = git branch -a 2>$null
        if ($LASTEXITCODE -eq 0) {
            foreach ($b in $branches) {
                $clean = $b.Trim() -replace '^\*?\s+', '' -replace '^remotes/[^/]+/', ''
                if ($clean -like "$prefix*") { return $true }
            }
        }
    } catch {
        Write-Verbose "Test-IndexCollision: branch scan failed: $_"
    }
    return $false
}

function Test-HasGit {
    try {
        git rev-parse --show-toplevel 2>$null | Out-Null
        return ($LASTEXITCODE -eq 0)
    } catch {
        return $false
    }
}

function Test-FeatureBranch {
    param(
        [string]$Branch,
        [bool]$HasGit = $true
    )

    # For non-git repos, we can't enforce branch naming but still provide output
    if (-not $HasGit) {
        Write-Warning "[devspark] Warning: Git repository not detected; skipped branch validation"
        return $true
    }

    if ($Branch -notmatch '^[0-9]{3}-') {
        Write-Output "ERROR: Not on a feature branch. Current branch: $Branch"
        Write-Output "Feature branches should be named like: 001-feature-name"
        return $false
    }
    return $true
}

function Get-FeatureDir {
    param([string]$RepoRoot, [string]$Branch)
    Join-Path $RepoRoot ".devspark.work/specs/$Branch"
}

function Resolve-TemplatePath {
    param(
        [string]$RepoRoot,
        [Parameter(Mandatory = $true)]
        [string]$TemplateName
    )

    $candidates = @(
        (Join-Path $RepoRoot ".devspark.work/templates/$TemplateName"),
        (Join-Path $RepoRoot ".devspark/templates/$TemplateName"),
        (Join-Path $RepoRoot "templates/$TemplateName")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

function Find-FeatureDirByPrefix {
    param(
        [string]$RepoRoot,
        [string]$BranchName
    )

    $specsDir = Join-Path $RepoRoot '.devspark.work/specs'
    if ($BranchName -notmatch '^(\d{3})-') {
        return (Join-Path $specsDir $BranchName)
    }

    #  (type-aware routing): a NNN-fix-<slug> branch is a quickfix, whose
    # record lives under .devspark.work/quickfixes/, not specs/. Resolve there so
    # FEATURE_DIR-based consumers (create-pr, next-context) do not misroute.
    if ($BranchName -match '^\d{3}-fix-') {
        return (Join-Path $RepoRoot '.devspark.work/quickfixes')
    }

    $prefixMatch = [regex]::Match($BranchName, '^(\d{3})-')
    if (-not $prefixMatch.Success) {
        return (Join-Path $specsDir $BranchName)
    }

    $prefix = $prefixMatch.Groups[1].Value
    if (-not (Test-Path $specsDir)) {
        return (Join-Path $specsDir $BranchName)
    }

    $matchesFound = @(Get-ChildItem -Path $specsDir -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "$prefix-*" } |
        Select-Object -ExpandProperty Name)

    if ($matchesFound.Count -eq 1) {
        return (Join-Path $specsDir $matchesFound[0])
    }

    return (Join-Path $specsDir $BranchName)
}

function Get-FeaturePathsEnv {
    $repoRoot = Get-RepoRoot
    $currentBranch = Get-CurrentBranch
    $hasGit = Test-HasGit
    $featureDir = Find-FeatureDirByPrefix -RepoRoot $repoRoot -BranchName $currentBranch

    # Quickfix routes resolve FEATURE_DIR to .devspark.work/quickfixes/, but the
    # record file is named after the branch (e.g. 042-fix-null-guard.md), not spec.md.
    $quickfixesDir = Join-Path $repoRoot '.devspark.work/quickfixes'
    if ($featureDir -eq $quickfixesDir) {
        $featureSpec = Join-Path $featureDir "$currentBranch.md"
    } else {
        $featureSpec = Join-Path $featureDir 'spec.md'
    }

    [PSCustomObject]@{
        REPO_ROOT     = $repoRoot
        CURRENT_BRANCH = $currentBranch
        HAS_GIT       = $hasGit
        FEATURE_DIR   = $featureDir
        FEATURE_SPEC  = $featureSpec
        IMPL_PLAN     = Join-Path $featureDir 'plan.md'
        TASKS         = Join-Path $featureDir 'tasks.md'
        RESEARCH      = Join-Path $featureDir 'research.md'
        DATA_MODEL    = Join-Path $featureDir 'data-model.md'
        QUICKSTART    = Join-Path $featureDir 'quickstart.md'
        CONTRACTS_DIR = Join-Path $featureDir 'contracts'
    }
}

function Get-MarkdownFrontmatter {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return $null
    }

    $lines = Get-Content -LiteralPath $Path -Encoding utf8
    if ($lines.Count -lt 3 -or $lines[0] -ne '---') {
        return $null
    }

    $frontmatter = New-Object System.Collections.Generic.List[string]
    for ($i = 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -eq '---') {
            return $frontmatter
        }
        $frontmatter.Add($lines[$i])
    }

    return $null
}

function Get-MarkdownFrontmatterValue {
    param(
        [string]$Path,
        [string]$Key
    )

    $frontmatter = Get-MarkdownFrontmatter -Path $Path
    if (-not $frontmatter) {
        return $null
    }

    foreach ($line in $frontmatter) {
        if ($line -match "^$([Regex]::Escape($Key)):\s*(.+)$") {
            return $matches[1].Trim()
        }
    }

    return $null
}

function Test-FileExists {
    param([string]$Path, [string]$Description)
    if (Test-Path -Path $Path -PathType Leaf) {
        Write-Output "  ✓ $Description"
        return $true
    } else {
        Write-Output "  ✗ $Description"
        return $false
    }
}

function Test-DirHasFiles {
    param([string]$Path, [string]$Description)
    if ((Test-Path -Path $Path -PathType Container) -and (Get-ChildItem -Path $Path -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer } | Select-Object -First 1)) {
        Write-Output "  ✓ $Description"
        return $true
    } else {
        Write-Output "  ✗ $Description"
        return $false
    }
}

function Add-DevSparkMetric {
    param(
        [string]$Command,
        [string]$Status = 'success',
        [int]$DurationMs = 0,
        [hashtable]$Findings = @{},
        [hashtable]$Extra = @{}
    )

    if ($env:DEVSPARK_METRICS_ENABLED -ne 'true') {
        return
    }

    try {
        $repoRoot = Get-RepoRoot
        $metricsDir = Join-Path $repoRoot '.devspark.work/metrics'
        if (-not (Test-Path $metricsDir)) {
            New-Item -ItemType Directory -Path $metricsDir -Force | Out-Null
        }

        $metricsFile = Join-Path $metricsDir 'devspark-metrics.jsonl'
        $feature = Get-FeaturePathsEnv
        $author = 'unknown'
        if (Test-HasGit) {
            try {
                $author = git config user.name 2>$null
                if (-not $author) { $author = 'unknown' }
            } catch {
                $author = 'unknown'
            }
        }

        $record = [ordered]@{
            ts_utc = (Get-Date).ToUniversalTime().ToString('o')
            command = $Command
            status = $Status
            duration_ms = $DurationMs
            branch = $feature.CURRENT_BRANCH
            feature_dir = $feature.FEATURE_DIR
            author = $author
            findings = @{
                showstopper = [int]($Findings.showstopper ?? 0)
                critical = [int]($Findings.critical ?? 0)
                high = [int]($Findings.high ?? 0)
                medium = [int]($Findings.medium ?? 0)
                low = [int]($Findings.low ?? 0)
            }
        }

        foreach ($k in $Extra.Keys) {
            $record[$k] = $Extra[$k]
        }

        ($record | ConvertTo-Json -Compress) + "`n" | Add-Content -Path $metricsFile -Encoding UTF8
    } catch {
        # Best-effort only; metrics must never block primary workflow.
    }
}

# ---------------------------------------------------------------------------
# OKF knowledge documents
# ---------------------------------------------------------------------------

# Writes one OKF knowledge document (feature/requirement/task) into
# <FeatureDir>/knowledge/<Id>.md. Atomic (temp file + rename); never throws --
# returns $true/$false and is always safe to call from a script whose own
# JSON/exit-code contract must stay unaffected by a generation failure .
function New-KnowledgeDocument {
    param(
        [Parameter(Mandatory)][string]$FeatureDir,
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][ValidateSet('feature', 'requirement', 'task')][string]$Type,
        [Parameter(Mandatory)][string]$Title,
        [ValidateSet('draft', 'active', 'done')][string]$Status = 'draft',
        [Parameter(Mandatory)][hashtable]$Links,
        [Parameter(Mandatory)][string]$GeneratedBy
    )

    $knowledgeDir = Join-Path $FeatureDir 'knowledge'
    $docPath = Join-Path $knowledgeDir "$Id.md"
    $tempPath = Join-Path $knowledgeDir ".$Id.md.tmp"

    try {
        if (-not (Test-Path $knowledgeDir)) {
            New-Item -ItemType Directory -Path $knowledgeDir -Force -ErrorAction Stop | Out-Null
        }

        $generatedAt = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
        $linkLines = foreach ($key in $Links.Keys) {
            $value = $Links[$key]
            if ($value -is [array]) {
                if ($value.Count -eq 0) {
                    "  ${key}: []"
                } else {
                    "  ${key}:"
                    foreach ($item in $value) { "    - $item" }
                }
            } else {
                "  ${key}: $value"
            }
        }

        $lines = @('---', "id: $Id", "type: $Type", "title: `"$($Title -replace '\\', '\\\\' -replace '"', '\"')`"", "status: $Status", 'links:') + `
            $linkLines + `
            @('generated:', "  by: $GeneratedBy", "  at: $generatedAt", '---', '')

        ($lines -join "`n") | Set-Content -Path $tempPath -Encoding UTF8 -NoNewline -ErrorAction Stop
        Move-Item -Path $tempPath -Destination $docPath -Force -ErrorAction Stop
        Add-DevSparkMetric -Command 'knowledge-document' -Status 'success' -Extra @{ knowledge_id = $Id; knowledge_type = $Type }
        return $true
    } catch {
        Write-Warning "DevSpark: failed to write knowledge document '$Id': $($_.Exception.Message)"
        if (Test-Path $tempPath) { Remove-Item $tempPath -Force -ErrorAction SilentlyContinue }
        try { Add-DevSparkMetric -Command 'knowledge-document' -Status 'failure' -Extra @{ knowledge_id = $Id; knowledge_type = $Type } } catch {}
        return $false
    }
}

# ---------------------------------------------------------------------------
# Repository configuration ("set once" facts: assistant name, platform, shell)
# ---------------------------------------------------------------------------

# Read the repository config object from .devspark.work/devspark.json (or $null).
function Get-DevSparkConfig {
    $repoRoot = Get-RepoRoot
    $configPath = Join-Path $repoRoot '.devspark.work/devspark.json'
    if (Test-Path $configPath) {
        try { return (Get-Content $configPath -Raw | ConvertFrom-Json) } catch {
            Write-Warning "DevSpark: failed to parse $configPath : $($_.Exception.Message)"
            return $null
        }
    }
    return $null
}

# Read the per-developer preferences from .devspark.work/devspark.user.json (gitignored; or $null).
function Get-DevSparkUserConfig {
    $repoRoot = Get-RepoRoot
    $configPath = Join-Path $repoRoot '.devspark.work/devspark.user.json'
    if (Test-Path $configPath) {
        try { return (Get-Content $configPath -Raw | ConvertFrom-Json) } catch {
            Write-Warning "DevSpark: failed to parse $configPath : $($_.Exception.Message)"
            return $null
        }
    }
    return $null
}

# Resolve the developer-chosen assistant name. Precedence:
# 1. DEVSPARK_ASSISTANT_NAME env var  2. user prefs  3. repo config  4. default 'Spark'.
function Get-AssistantName {
    if ($env:DEVSPARK_ASSISTANT_NAME) { return $env:DEVSPARK_ASSISTANT_NAME }
    $u = Get-DevSparkUserConfig
    if ($u -and $u.assistant -and $u.assistant.name) { return "$($u.assistant.name)" }
    $c = Get-DevSparkConfig
    if ($c -and $c.assistant -and $c.assistant.name) { return "$($c.assistant.name)" }
    return 'Spark'
}

# Read a preference/policy by name. Precedence: user prefs -> repo preferences -> repo policies -> $Default.
function Get-DevSparkPreference {
    param([string]$Name, $Default = $null)
    $u = Get-DevSparkUserConfig
    if ($u -and $u.preferences -and ($u.preferences.PSObject.Properties.Name -contains $Name)) { return $u.preferences.$Name }
    $c = Get-DevSparkConfig
    if ($c -and $c.preferences -and ($c.preferences.PSObject.Properties.Name -contains $Name)) { return $c.preferences.$Name }
    if ($c -and $c.policies -and ($c.policies.PSObject.Properties.Name -contains $Name)) { return $c.policies.$Name }
    return $Default
}

# Resolve the git user / DevSpark override key. Precedence:
# 1. DEVSPARK_GIT_USER env  2. user config git_user  3. git config user.name  4. 'unknown'.
function Get-GitUser {
    if ($env:DEVSPARK_GIT_USER) { return $env:DEVSPARK_GIT_USER }
    $u = Get-DevSparkUserConfig
    if ($u -and $u.git_user) { return "$($u.git_user)" }
    if (Test-HasGit) {
        try { $n = git config user.name 2>$null; if ($n) { return $n } } catch { }
    }
    return 'unknown'
}

# Resolve Azure DevOps org/project/repo from devspark.json, falling back to the origin remote URL.
function Get-AzdoRepoContext {
    $cfg = Get-DevSparkConfig
    $org = if ($cfg -and $cfg.organization) { "$($cfg.organization)" } else { '' }
    $project = if ($cfg -and $cfg.project) { "$($cfg.project)" } else { '' }
    $repo = if ($cfg -and $cfg.repository) { "$($cfg.repository)" } else { '' }
    if (-not $org -or -not $project -or -not $repo) {
        try {
            $u = git remote get-url origin 2>$null
            if ($u -match 'dev\.azure\.com/([^/]+)/([^/]+)/_git/([^/\s]+?)(\.git)?$') {
                if (-not $org) { $org = $matches[1] }
                if (-not $project) { $project = $matches[2] }
                if (-not $repo) { $repo = $matches[3] }
            }
        } catch { }
    }
    return [PSCustomObject]@{ Org = $org; Project = $project; Repo = $repo }
}

# Read a property from a parsed-JSON object, returning $null when it is absent. Direct
# property access on a missing key throws under Set-StrictMode, and callers of this helper
# run under strict mode.
function Get-DevSparkProperty {
    param([AllowNull()]$Object, [string]$Name)
    if ($null -eq $Object) { return $null }
    if ($Object.PSObject.Properties.Name -contains $Name) { return $Object.$Name }
    return $null
}

# Resolve work-tracking context in the declared precedence: explicit arguments, the
# work_tracking block, top-level config, then the origin remote for repository identity only.
# Area, iteration, type and tags are never inferred from the remote. Each resolved value
# carries the step that produced it so a write can display where its inputs came from.
function Get-WorkTrackingContext {
    param(
        [string]$Organization = '',
        [string]$Project = '',
        [string]$AreaPath = ''
    )

    $cfg = Get-DevSparkConfig
    $wt = Get-DevSparkProperty $cfg 'work_tracking'
    $sources = @{}

    function Resolve-Value {
        param($Explicit, $FromConfig, [string]$ConfigStep, [string]$Key, [hashtable]$Sources)
        if ($Explicit) { $Sources[$Key] = 'argument'; return "$Explicit" }
        if ($FromConfig) { $Sources[$Key] = $ConfigStep; return "$FromConfig" }
        $Sources[$Key] = 'unresolved'
        return ''
    }

    $org = Resolve-Value $Organization (Get-DevSparkProperty $cfg 'organization') 'config' 'organization' $sources
    $proj = Resolve-Value $Project (Get-DevSparkProperty $cfg 'project') 'config' 'project' $sources
    $repo = Resolve-Value '' (Get-DevSparkProperty $cfg 'repository') 'config' 'repository' $sources

    # Step 4 applies to repository identity only.
    if (-not $org -or -not $proj -or -not $repo) {
        try {
            $u = git remote get-url origin 2>$null
            if ($u -match 'dev\.azure\.com/([^/]+)/([^/]+)/_git/([^/\s]+?)(\.git)?$') {
                if (-not $org) { $org = $matches[1]; $sources['organization'] = 'origin_remote' }
                if (-not $proj) { $proj = $matches[2]; $sources['project'] = 'origin_remote' }
                if (-not $repo) { $repo = $matches[3]; $sources['repository'] = 'origin_remote' }
            }
        } catch { }
    }

    $area = Resolve-Value $AreaPath (Get-DevSparkProperty $wt 'default_area_path') 'work_tracking' 'default_area_path' $sources
    $iteration = Resolve-Value '' (Get-DevSparkProperty $wt 'default_iteration_path') 'work_tracking' 'default_iteration_path' $sources
    $itemType = Resolve-Value '' (Get-DevSparkProperty $wt 'default_work_item_type') 'work_tracking' 'default_work_item_type' $sources

    $tags = @()
    $cfgTags = Get-DevSparkProperty $wt 'tags'
    if ($cfgTags) { $tags = @($cfgTags) }

    $reviewOnSpecify = $true
    $cfgReview = Get-DevSparkProperty $wt 'review_on_specify'
    if ($null -ne $cfgReview) { $reviewOnSpecify = [bool]$cfgReview }

    $platform = Get-DevSparkProperty $cfg 'platform'

    return [PSCustomObject]@{
        Configured      = ($null -ne $wt)
        Platform        = if ($platform) { "$platform" } else { '' }
        Organization    = $org
        Project         = $proj
        Repository      = $repo
        AreaPath        = $area
        IterationPath   = $iteration
        WorkItemType    = $itemType
        Tags            = $tags
        ReviewOnSpecify = $reviewOnSpecify
        Sources         = $sources
    }
}

# ---------------------------------------------------------------------------
# Multi-app support helpers
# ---------------------------------------------------------------------------

# Detect whether the repository is operating in multi-app mode.
function Detect-DevSparkMode {
    $repoRoot = Get-RepoRoot
    $registryPath = Join-Path $repoRoot '.devspark.work/devspark.registry.json'

    if (Test-Path $registryPath) {
        try {
            $config = Get-Content $registryPath -Raw | ConvertFrom-Json
            if ($config.mode -eq 'multi-app') {
                return 'multi-app'
            }
        } catch {
            # Invalid JSON
        }
    }
    return 'single-app'
}

# Validate the registry structure before deeper processing.
function Test-RegistryJson {
    param([string]$RegistryPath)

    if (-not (Test-Path $RegistryPath)) {
        return [PSCustomObject]@{ valid = $false; error = "Registry file not found" }
    }

    try {
        $config = Get-Content $RegistryPath -Raw | ConvertFrom-Json
    } catch {
        return [PSCustomObject]@{ valid = $false; error = "Invalid JSON" }
    }

    # Check version
    if ($config.version -ne 1) {
        return [PSCustomObject]@{ valid = $false; error = "Unsupported version: $($config.version)" }
    }

    # Check unique IDs
    $ids = $config.apps | ForEach-Object { $_.id }
    $uniqueIds = $ids | Sort-Object -Unique
    if ($ids.Count -ne $uniqueIds.Count) {
        return [PSCustomObject]@{ valid = $false; error = "Duplicate app IDs detected" }
    }

    # Check profile references
    $profileKeys = $config.profiles.PSObject.Properties.Name
    $badProfiles = @()
    foreach ($app in $config.apps) {
        if ($app.inherits) {
            foreach ($prof in $app.inherits) {
                if ($prof -notin $profileKeys) {
                    $badProfiles += $prof
                }
            }
        }
    }
    if ($badProfiles.Count -gt 0) {
        return [PSCustomObject]@{ valid = $false; error = "Unknown profiles: $($badProfiles -join ', ')" }
    }

    return [PSCustomObject]@{
        valid    = $true
        apps     = $config.apps.Count
        profiles = $profileKeys.Count
    }
}

# Resolve app documentation root
function Resolve-AppDocRoot {
    param(
        [string]$RepoRoot,
        [string]$AppId
    )

    if (-not $AppId) {
        return Join-Path $RepoRoot '.devspark.work'
    }

    $registryPath = Join-Path $RepoRoot '.devspark.work/devspark.registry.json'
    if (-not (Test-Path $registryPath)) {
        throw "No multi-app registry found"
    }

    $config = Get-Content $registryPath -Raw | ConvertFrom-Json
    $app = $config.apps | Where-Object { $_.id -eq $AppId } | Select-Object -First 1

    if (-not $app) {
        $available = ($config.apps | ForEach-Object { $_.id }) -join ', '
        throw "Unknown application: $AppId. Available: $available"
    }

    return Join-Path $RepoRoot "$($app.path)/.devspark.work"
}

# Parse --app and --repo-scope arguments
function Parse-AppContext {
    param([string[]]$Arguments)

    $result = [PSCustomObject]@{
        AppId     = ''
        RepoScope = $false
        Remaining = @()
    }

    $i = 0
    while ($i -lt $Arguments.Count) {
        switch ($Arguments[$i]) {
            '--app' {
                $i++
                if ($i -ge $Arguments.Count -or $Arguments[$i].StartsWith('--')) {
                    throw "--app requires an application ID"
                }
                $result.AppId = $Arguments[$i]
            }
            '--repo-scope' {
                $result.RepoScope = $true
            }
            default {
                $result.Remaining += $Arguments[$i]
            }
        }
        $i++
    }

    return $result
}

# Resolve scope and validate
function Resolve-AppScope {
    param(
        [string]$AppId = '',
        [bool]$RepoScope = $false
    )

    $repoRoot = Get-RepoRoot
    $mode = Detect-DevSparkMode

    $result = [PSCustomObject]@{
        Scope   = ''
        DocRoot = ''
        AppId   = ''
        Error   = ''
    }

    if ($mode -eq 'single-app') {
        if ($AppId) {
            $result.Error = "No multi-app registry found. Cannot use --app."
            return $result
        }
        $result.Scope = 'repo'
        $result.DocRoot = Join-Path $repoRoot '.devspark.work'
        return $result
    }

    # Multi-app mode
    if ($RepoScope) {
        $result.Scope = 'repo'
        $result.DocRoot = Join-Path $repoRoot '.devspark.work'
        return $result
    }

    if ($AppId) {
        try {
            $docRoot = Resolve-AppDocRoot -RepoRoot $repoRoot -AppId $AppId
            $result.Scope = 'single-app'
            $result.DocRoot = $docRoot
            $result.AppId = $AppId
        } catch {
            $result.Error = $_.Exception.Message
        }
        return $result
    }

    # No explicit scope
    $registryPath = Join-Path $repoRoot '.devspark.work/devspark.registry.json'
    $config = Get-Content $registryPath -Raw | ConvertFrom-Json
    $appCount = $config.apps.Count

    if ($appCount -gt 1) {
        $available = ($config.apps | ForEach-Object { $_.id }) -join ', '
        $result.Error = "Multiple apps registered; specify --app <id> or use --repo-scope. Available: $available"
        return $result
    }

    if ($appCount -eq 1) {
        $app = $config.apps[0]
        $result.Scope = 'single-app'
        $result.AppId = $app.id
        $result.DocRoot = Join-Path $repoRoot "$($app.path)/.devspark.work"
        return $result
    }

    $result.Scope = 'repo'
    $result.DocRoot = Join-Path $repoRoot '.devspark.work'
    return $result
}

# Resolve constitution with app overlay
function Resolve-Constitution {
    param(
        [string]$RepoRoot,
        [string]$AppId = ''
    )

    $repoConst = Join-Path $RepoRoot '.knowledge/governance/constitution.md'
    if (-not (Test-Path $repoConst)) {
        throw "Repository constitution required at $repoConst"
    }

    $output = Get-Content $repoConst -Raw

    if ($AppId) {
        $config = Get-Content (Join-Path $RepoRoot '.devspark.work/devspark.json') -Raw | ConvertFrom-Json
        $app = $config.apps | Where-Object { $_.id -eq $AppId } | Select-Object -First 1
        $appConst = Join-Path $RepoRoot "$($app.path)/.knowledge/governance/constitution.md"

        if (Test-Path $appConst) {
            $appText = Get-Content $appConst -Raw
            $output = "$output`n`n---`n`n## Application Overlay: $AppId`n`n$appText"
        }
    }

    return $output
}

# Get direct downstream consumers of an app
function Get-DownstreamApps {
    param(
        [string]$RepoRoot,
        [string]$AppId
    )

    $registryPath = Join-Path $RepoRoot '.devspark.work/devspark.registry.json'
    if (-not (Test-Path $registryPath)) { return @() }

    $config = Get-Content $registryPath -Raw | ConvertFrom-Json
    $downstream = @()

    foreach ($app in $config.apps) {
        if ($app.dependsOn -contains $AppId) {
            $downstream += $app.id
        }
    }

    return $downstream
}

# Generate scope report
function Write-ScopeReport {
    param([PSCustomObject]$Scope)

    $repoRoot = Get-RepoRoot

    Write-Output "## BSW.DevSpark Scope Report"
    Write-Output ""
    Write-Output "**Scope type**: $($Scope.Scope)"
    Write-Output "**Documentation root**: $($Scope.DocRoot)"

    if ($Scope.AppId) {
        Write-Output "**Primary application**: $($Scope.AppId)"

        $downstream = Get-DownstreamApps -RepoRoot $repoRoot -AppId $Scope.AppId
        if ($downstream.Count -gt 0) {
            Write-Output ""
            Write-Output "### Declared downstream dependencies"
            foreach ($dep in $downstream) {
                Write-Output "- $dep"
            }
        }
    }
}

# Print scope summary
function Write-ScopeSummary {
    param([PSCustomObject]$Scope)

    Write-Output "--- BSW.DevSpark Scope ---"
    Write-Output "scope: $($Scope.Scope)"
    Write-Output "doc-root: $($Scope.DocRoot)"
    if ($Scope.AppId) {
        Write-Output "app: $($Scope.AppId)"
    }
    Write-Output "mode: $(Detect-DevSparkMode)"
    Write-Output "---"
}

# Resolve inherited profile chain for an app
function Resolve-AppProfiles {
    param(
        [string]$RepoRoot,
        [string]$AppId
    )

    $registryPath = Join-Path $RepoRoot '.devspark.work/devspark.registry.json'
    if (-not (Test-Path $registryPath)) {
        return [PSCustomObject]@{ tags = @{}; rules = @(); hints = @{} }
    }

    $config = Get-Content $registryPath -Raw | ConvertFrom-Json
    $app = $config.apps | Where-Object { $_.id -eq $AppId } | Select-Object -First 1
    if (-not $app) { throw "Unknown app: $AppId" }

    $tags = @{}; $rules = @(); $hints = @{}

    # Compose inherited profiles
    if ($app.inherits) {
        foreach ($profName in $app.inherits) {
            $prof = $config.profiles.$profName
            if ($prof) {
                if ($prof.tags) { $prof.tags.PSObject.Properties | ForEach-Object { $tags[$_.Name] = $_.Value } }
                if ($prof.rules) { foreach ($r in $prof.rules) { if ($r -notin $rules) { $rules += $r } } }
                if ($prof.hints) { $prof.hints.PSObject.Properties | ForEach-Object { $hints[$_.Name] = $_.Value } }
            }
        }
    }

    # Apply app overrides
    if ($app.overrides) {
        if ($app.overrides.tags) { $app.overrides.tags.PSObject.Properties | ForEach-Object { $tags[$_.Name] = $_.Value } }
        if ($app.overrides.rules) { foreach ($r in $app.overrides.rules) { if ($r -notin $rules) { $rules += $r } } }
        if ($app.overrides.hints) { $app.overrides.hints.PSObject.Properties | ForEach-Object { $hints[$_.Name] = $_.Value } }
    }

    # Apply app.json
    $appJson = Join-Path $RepoRoot "$($app.path)/app.json"
    if (Test-Path $appJson) {
        $manifest = Get-Content $appJson -Raw | ConvertFrom-Json
        if ($manifest.tags) { $manifest.tags.PSObject.Properties | ForEach-Object { $tags[$_.Name] = $_.Value } }
        if ($manifest.rules) { foreach ($r in $manifest.rules) { if ($r -notin $rules) { $rules += $r } } }
        if ($manifest.hints) { $manifest.hints.PSObject.Properties | ForEach-Object { $hints[$_.Name] = $_.Value } }
    }

    return [PSCustomObject]@{ tags = $tags; rules = $rules; hints = $hints }
}

# App-aware feature paths
function Get-FeaturePathsAppAware {
    param([PSCustomObject]$Scope)

    $repoRoot = Get-RepoRoot
    $currentBranch = Get-CurrentBranch
    $hasGit = Test-HasGit

    $docRoot = if ($Scope -and $Scope.DocRoot) { $Scope.DocRoot } else { Join-Path $repoRoot '.documentation' }
    $specsDir = Join-Path $docRoot 'specs'
    $featureDir = Join-Path $specsDir $currentBranch

    return [PSCustomObject]@{
        REPO_ROOT      = $repoRoot
        CURRENT_BRANCH = $currentBranch
        HAS_GIT        = $hasGit
        DEVSPARK_SCOPE = if ($Scope) { $Scope.Scope } else { 'repo' }
        DEVSPARK_APP   = if ($Scope) { $Scope.AppId } else { '' }
        DOC_ROOT       = $docRoot
        FEATURE_DIR    = $featureDir
        FEATURE_SPEC   = Join-Path $featureDir 'spec.md'
        IMPL_PLAN      = Join-Path $featureDir 'plan.md'
        TASKS          = Join-Path $featureDir 'tasks.md'
        RESEARCH       = Join-Path $featureDir 'research.md'
        DATA_MODEL     = Join-Path $featureDir 'data-model.md'
        QUICKSTART     = Join-Path $featureDir 'quickstart.md'
        CONTRACTS_DIR  = Join-Path $featureDir 'contracts'
    }
}



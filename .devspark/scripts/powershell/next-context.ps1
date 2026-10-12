#!/usr/bin/env pwsh
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Next-step router context script.
# Detects the current lifecycle state locally (git + filesystem, best-effort gh)
# and recommends the single next DevSpark command to run.

param(
    [Parameter(Position = 0, ValueFromRemainingArguments)]
    [string[]]$Arguments,
    [switch]$Json
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
$hasGit = [bool](Test-HasGit)
$currentBranch = Get-CurrentBranch
$assistantName = Get-AssistantName
$gitUser = Get-GitUser

# Orientation facts ("where am I"): repo name + host platform, from config or remote.
$devsparkConfig = Get-DevSparkConfig
if ($devsparkConfig -and $devsparkConfig.repository) { $repoName = "$($devsparkConfig.repository)" } else { $repoName = Split-Path $repoRoot -Leaf }
if ($devsparkConfig -and $devsparkConfig.platform) {
    $platform = "$($devsparkConfig.platform)"
} else {
    $platform = 'unknown'
    if ($hasGit) {
        try {
            $remoteUrl = git remote get-url origin 2>$null
            if ($remoteUrl -match 'dev\.azure\.com|visualstudio\.com') { $platform = 'azdo' }
            elseif ($remoteUrl -match 'github\.com') { $platform = 'github' }
            elseif ($remoteUrl -match 'gitlab') { $platform = 'gitlab' }
        } catch { $platform = 'unknown' }
    }
}

# Constitution
$constitutionPath = Join-Path $repoRoot ".knowledge/governance/constitution.md"
$constitutionExists = Test-Path $constitutionPath

# Default (target) branch detection
$defaultBranch = "main"
if ($hasGit) {
    try {
        $head = git symbolic-ref --quiet --short refs/remotes/origin/HEAD 2>$null
        if ($head) {
            $defaultBranch = ($head -replace '^origin/', '')
        }
        else {
            git show-ref --verify --quiet refs/heads/master 2>$null
            if ($LASTEXITCODE -eq 0) { $defaultBranch = "master" }
        }
    } catch { $defaultBranch = "main" }
}
$isDefaultBranch = ($currentBranch -eq $defaultBranch -or $currentBranch -eq "main" -or $currentBranch -eq "master")

# Git state
$dirty = $false
$hasRemote = $false
$branchPushed = $false
$behindTarget = $false
if ($hasGit) {
    try { $dirty = [bool]((git status --porcelain 2>$null) | Where-Object { $_ }) } catch { $dirty = $false }
    try { $hasRemote = [bool]((git remote 2>$null) | Where-Object { $_ -and $_.Trim() }) } catch { $hasRemote = $false }
    if ($hasRemote) {
        try {
            git rev-parse --verify --quiet "refs/remotes/origin/$currentBranch" 2>$null | Out-Null
            $branchPushed = ($LASTEXITCODE -eq 0)
        } catch { $branchPushed = $false }
        try {
            $behindCount = git rev-list --count "HEAD..origin/$defaultBranch" 2>$null
            $behindTarget = ($behindCount -and [int]$behindCount -gt 0)
        } catch { $behindTarget = $false }
    }
}

# Spec / quickfix detection for the current branch. The branch name may differ from the spec
# directory, so resolve by numeric prefix: an exact spec directory wins, a quickfix record for the
# branch means a quickfix (not a spec), otherwise exactly one spec directory with that prefix is the
# active spec. DEVSPARK_FEATURE overrides the branch. More than one match is reported, never guessed.
$specDir = Join-Path $repoRoot (".devspark.work/specs/" + $currentBranch)
$specAmbiguous = $false
$specAmbiguousMatches = ''
if (-not (Test-Path (Join-Path $specDir 'spec.md')) -and $currentBranch -match '^(\d{3})-' -and -not (Test-Path (Join-Path $repoRoot ".devspark.work/quickfixes/$currentBranch.md"))) {
    $specPrefix = $matches[1]
    $specsRoot = Join-Path $repoRoot '.devspark.work/specs'
    $prefixDirs = @()
    if (Test-Path $specsRoot) {
        $prefixDirs = @(Get-ChildItem -Path $specsRoot -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "$specPrefix-*" -and (Test-Path (Join-Path $_.FullName 'spec.md')) })
    }
    if ($prefixDirs.Count -eq 1) {
        $specDir = $prefixDirs[0].FullName
    } elseif ($prefixDirs.Count -gt 1) {
        $specAmbiguous = $true
        $specAmbiguousMatches = (($prefixDirs | Select-Object -ExpandProperty Name) -join ', ')
    }
}
$specExists = (-not $specAmbiguous) -and (Test-Path (Join-Path $specDir "spec.md"))
$specFeature = if ($specExists) { Split-Path $specDir -Leaf } else { '' }

# Spec authoring sub-state (plan -> tasks -> analyze -> critic -> implement -> verify)
$planExists = $false
$tasksExists = $false
$tasksTotal = 0
$tasksDone = 0
$tasksComplete = $false
$classification = ""
$specStatus = ""
$isFullSpec = $false
$requireAnalyze = $false
$requireCritic = $false
$requireChecklist = $false
$analyzeGate = $false
$criticGate = $false
$judgeGate = $false
$judgeVerdict = ""
$judgeNeedsRecheck = $false
$judgePendingDecisions = 0
$planRevisionCount = 0
$planChanged = $null
$tasksChanged = $false
$tasksStale = $true
$planSpecState = "unknown"
$tasksPlanState = "unknown"
$judgeInvalidated = $false
$remediationAppended = $false
$checklistGate = $false
$verifyRequired = $false
$verifyPassed = $false
$closeoutDecision = ""
if ($specExists) {
    $planExists = Test-Path (Join-Path $specDir "plan.md")
    $tasksPath = Join-Path $specDir "tasks.md"
    $tasksExists = Test-Path $tasksPath
    if ($tasksExists) {
        try {
            $tasksText = Get-Content $tasksPath -Raw -ErrorAction SilentlyContinue
            $tasksTotal = ([regex]::Matches($tasksText, '(?m)^\s*[-*]\s*\[[ xX]\]')).Count
            $tasksDone = ([regex]::Matches($tasksText, '(?m)^\s*[-*]\s*\[[xX]\]')).Count
            $tasksComplete = ($tasksTotal -gt 0 -and $tasksDone -ge $tasksTotal)
            $remediationAppended = [bool]([regex]::IsMatch($tasksText, '(?mi)^## Gate Remediation'))
        } catch { $tasksComplete = $false }
    }
    try {
        $specText = Get-Content (Join-Path $specDir "spec.md") -Raw -ErrorAction SilentlyContinue
        if ($specText -match '(?m)^classification:\s*(.+)$') { $classification = $matches[1].Trim() }
        if ($specText -match '(?m)^\*\*Status\*\*:\s*([^<\r\n]+)') { $specStatus = $matches[1].Trim() }
        $reqGates = ""
        if ($specText -match '(?m)^required_gates:\s*(.+)$') { $reqGates = $matches[1] }
        if ($reqGates -match 'verify:') { $verifyRequired = $true }
        if ($reqGates -match 'analyze') { $requireAnalyze = $true }
        if ($reqGates -match 'critic') { $requireCritic = $true }
        if ($reqGates -match 'checklist') { $requireChecklist = $true }
    } catch { }
    $isFullSpec = ($classification -match 'full-spec') -or ($classification -eq '' -and ($planExists -or $tasksExists))
    $analyzeGate = Test-Path (Join-Path $specDir "gates/analyze.md")
    $criticGate = Test-Path (Join-Path $specDir "gates/critic.md")
    $judgeFile = Join-Path $specDir "gates/judge.md"
    if (Test-Path $judgeFile) {
        $judgeGate = $true
        try {
            $jt = Get-Content $judgeFile -Raw -ErrorAction SilentlyContinue
            if ($jt -match '(?m)^verdict:\s*(\S+)') { $judgeVerdict = $matches[1].Trim() }
        } catch { $judgeVerdict = "" }
    }
    $checklistGate = Test-Path (Join-Path $specDir "gates/checklist.md")
    $verifyGate = Join-Path $specDir "gates/verify.md"
    if (Test-Path $verifyGate) {
        try {
            $vt = Get-Content $verifyGate -Raw -ErrorAction SilentlyContinue
            if ($vt -match '(?m)^status:\s*pass') { $verifyPassed = $true }
        } catch { $verifyPassed = $false }
    }
    $closeoutGate = Join-Path $specDir "gates/closeout.md"
    if (Test-Path $closeoutGate) {
        try {
            $ct = Get-Content $closeoutGate -Raw -ErrorAction SilentlyContinue
            if ($ct -match '(?m)^decision:\s*(\S+)') { $closeoutDecision = $matches[1].Trim() }
        } catch { $closeoutDecision = "" }
    }
    if ($verifyRequired) { $verifyPassed = $false }
    $judgePy = Join-Path $PSScriptRoot '../judge.py'
    if (Test-Path $judgePy) {
        foreach ($py in @('python3', 'python')) {
            if (-not (Get-Command $py -ErrorAction SilentlyContinue)) { continue }
            try {
                $reviewState = (& $py $judgePy review-state --feature-dir $specDir 2>$null) -join "`n"
                if ($LASTEXITCODE -eq 0 -and $reviewState) {
                    $reviewData = $reviewState | ConvertFrom-Json
                    $judgeNeedsRecheck = [bool]$reviewData.needs_recheck
                    $judgePendingDecisions = [int]$reviewData.pending_decisions
                    $planRevisionCount = [int]$reviewData.plan_revision_count
                    $planChanged = $reviewData.plan_changed
                    $tasksChanged = [bool]$reviewData.tasks_changed
                    $tasksStale = [bool]$reviewData.tasks_stale
                    if ($reviewData.plan_spec_state) { $planSpecState = [string]$reviewData.plan_spec_state }
                    if ($reviewData.tasks_plan_state) { $tasksPlanState = [string]$reviewData.tasks_plan_state }
                    $judgeInvalidated = [bool]$reviewData.judge_stale -or $reviewData.spec_changed -eq $true
                    $verifyRequired = [bool]$reviewData.verify_required
                    $verifyPassed = [bool]$reviewData.verify_passed
                    $closeoutDecision = [string]$reviewData.closeout_decision
                    break
                }
            } catch { }
        }
    }
}

# Edge routes yield to pending decisions, spec/Judge invalidation and STOP remediation.
$planningEdgesRoutable = (-not $judgeGate) -or (-not $judgeInvalidated -and $judgeVerdict -ne "STOP")

# Advisory gates that are required but whose artifact is missing/unmet (never block progress)
$pendingGates = @()
if ($specExists) {
    if ($requireChecklist -and -not $checklistGate) { $pendingGates += "checklist" }
    # Planning Review (judge.md) supersedes the separate analyze/critic artifacts: Critic may be not applicable.
    if (-not $judgeGate) {
        if ($requireAnalyze -and -not $analyzeGate) { $pendingGates += "analyze" }
        if ($requireCritic -and -not $criticGate) { $pendingGates += "critic" }
    }
    if ($verifyRequired -and -not $verifyPassed) { $pendingGates += "verify" }
    if ($closeoutDecision -eq "not-complete") { $pendingGates += "closeout (blocking defects open)" }
}
$pendingGatesStr = ($pendingGates -join ", ")

$quickfixDir = Join-Path $repoRoot ".devspark.work/quickfixes"
$quickfixExists = $false
$quickfixComplete = $false
if ($currentBranch -match '^QF-' -or $currentBranch -match '^\d{3}-fix-') {
    $quickfixRecord = Join-Path $quickfixDir ("$currentBranch.md")
    $quickfixExists = Test-Path $quickfixRecord
    if ($quickfixExists) {
        try {
            $qf = Get-Content $quickfixRecord -Raw -ErrorAction SilentlyContinue
            if ($qf -match '(?m)^\s*-\s*\*\*Completed\*\*:\s*\S') { $quickfixComplete = $true }
        } catch { $quickfixComplete = $false }
    }
}

# Best-effort PR + review detection (platform-aware)
$prExists = $false
$prNumber = ""
$prState = ""
if ($hasGit) {
    if ($platform -eq 'azdo' -and (Get-Command az -ErrorAction SilentlyContinue)) {
        try {
            $azctx = Get-AzdoRepoContext
            $azList = az repos pr list --organization "https://dev.azure.com/$($azctx.Org)" --project $azctx.Project --repository $azctx.Repo --source-branch $currentBranch --status active --top 1 --output json 2>$null | ConvertFrom-Json
            $pr = @($azList)[0]
            if ($pr -and $pr.pullRequestId) {
                $prExists = $true
                $prNumber = "$($pr.pullRequestId)"
                $prState = "$($pr.status)"
            }
        } catch { $prExists = $false }
    }
    elseif (Get-Command gh -ErrorAction SilentlyContinue) {
        try {
            $prJson = gh pr view --json number,state 2>$null | ConvertFrom-Json
            if ($prJson -and $prJson.number) {
                $prExists = $true
                $prNumber = "$($prJson.number)"
                $prState = "$($prJson.state)"
            }
        } catch { $prExists = $false }
    }
}

$reviewFileExists = $false
$reviewOpenFindings = $false
$reviewDir = Join-Path $repoRoot ".devspark.work/pr-review"
if (Test-Path $reviewDir) {
    $reviewFile = $null
    if ($prNumber) {
        $candidate = Join-Path $reviewDir ("pr-$prNumber.md")
        if (Test-Path $candidate) { $reviewFile = $candidate }
    }
    if (-not $reviewFile) {
        $reviewFile = (Get-ChildItem -Path $reviewDir -Filter "pr-*.md" -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1)?.FullName
    }
    if ($reviewFile -and (Test-Path $reviewFile)) {
        $reviewFileExists = $true
        try {
            $reviewText = Get-Content $reviewFile -Raw -ErrorAction SilentlyContinue
            $reviewOpenFindings = ($reviewText -match 'Open')
        } catch { $reviewOpenFindings = $false }
    }
}

# ---- Recommendation ladder ----
$recommended = "/devspark-specify"
$state = "unknown"
$reason = "Describe the work to /devspark-specify and it will route you."

if (-not $constitutionExists) {
    $recommended = "/devspark-constitution"
    $state = "no-constitution"
    $reason = "No project constitution found. Create your principles first so every command has a validation baseline."
}
elseif ($specAmbiguous) {
    $recommended = "resolve"
    $state = "ambiguous-spec"
    $reason = "Branch '$currentBranch' matches more than one spec directory ($specAmbiguousMatches). Set DEVSPARK_FEATURE to the intended directory name, or remove the duplicate, then re-run /devspark.next."
}
elseif ($specExists -and ($judgeVerdict -eq "CONDITIONAL" -or $judgePendingDecisions -gt 0)) {
    $recommended = "/devspark-planning-review"
    $state = "review-needs-decision"
    $reason = "A human planning decision is pending. Answer it before remediation or implementation, then use /devspark-planning-review --judge-only to record the disposition."
}
elseif ($isDefaultBranch -and -not $specExists -and -not $quickfixExists) {
    $recommended = "/devspark-specify"
    $state = "no-active-work"
    $reason = "You're on '$currentBranch' with no active spec or quickfix. Start with /devspark-specify (unsure of size) or /devspark-quickfix (small, contained fix)."
}
elseif ($specExists -and $isFullSpec -and -not $planExists) {
    $recommended = "/devspark-plan"
    $state = "spec-needs-plan"
    $reason = "Spec exists for '$currentBranch' but there's no plan yet. Run /devspark-plan to produce the technical design."
}
elseif ($specExists -and $isFullSpec -and $planExists -and $planSpecState -eq "stale" -and $planningEdgesRoutable) {
    $recommended = "/devspark-plan"
    $state = "plan-stale-against-spec"
    $reason = "The spec changed in meaning since the plan was produced or last checked. Run /devspark-plan: it checks the existing plan against the current spec and changes only what the spec requires."
}
elseif ($specExists -and $isFullSpec -and $planExists -and -not $tasksExists) {
    $recommended = "/devspark-tasks"
    $state = "plan-needs-tasks"
    $reason = "Plan exists but there's no task breakdown. Run /devspark-tasks to generate the ordered task list."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and $tasksPlanState -eq "stale" -and $planningEdgesRoutable) {
    $recommended = "/devspark-tasks"
    $state = "tasks-stale-against-plan"
    $reason = "The plan changed in meaning since the task list was projected. Run /devspark-tasks: it checks the task list against the current plan and re-projects only unchecked work."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and -not $judgeGate -and ($requireAnalyze -or $requireCritic)) {
    $recommended = "/devspark-planning-review"
    $state = "tasks-need-review"
    $reason = "Tasks are ready and this full-spec route requires planning assurance. Run /devspark-planning-review: it runs Analyze, Critic when applicable, and Judge, and returns one planning decision before implementing."
}
# Unresolved Planning Review outranks task progress: a finished task list never overrides open assurance.
elseif ($specExists -and $isFullSpec -and $tasksExists -and $judgeInvalidated) {
    $recommended = "/devspark-planning-review"
    $state = "review-needs-recheck"
    $reason = "The specification or adjudicated child evidence changed. Re-establish planning authority before applying corrections."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and $judgeVerdict -eq "STOP" -and $planRevisionCount -gt 0 -and $planChanged -eq $true -and $tasksStale) {
    $recommended = "/devspark-tasks"
    $state = "plan-needs-reprojection"
    $reason = "Plan semantics changed. Re-project unchecked Tasks from the approved Plan and protect completed work before focused review."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and $judgeVerdict -eq "STOP" -and $planRevisionCount -gt 0 -and $planChanged -eq $false) {
    $recommended = "/devspark-plan"
    $state = "review-needs-plan"
    $reason = "Judge assigned an approved Plan correction. Run /devspark-plan in focused revise mode; preserve unrelated decisions and never reset automatically."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and $judgeVerdict -eq "STOP" -and ($planChanged -eq $true -or $tasksChanged -or $remediationAppended -or ($planRevisionCount -gt 0 -and $null -eq $planChanged))) {
    $recommended = "/devspark-planning-review"
    $state = "review-needs-recheck"
    $reason = "Correction inputs changed or their provenance is unknown. Re-run /devspark-planning-review for a focused check; changed content does not resolve STOP by itself."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and $judgeVerdict -eq "STOP") {
    $recommended = "/devspark-tasks"
    $state = "review-blocked"
    $reason = "Planning review returned STOP. Run /devspark-tasks for validated task-owned corrections only; missing ownership or scope authority must stop for a decision."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and $judgeNeedsRecheck) {
    $recommended = "/devspark-planning-review"
    $state = "review-needs-recheck"
    $reason = "A reviewed artifact changed in a way that invalidates the Planning Review evidence. Re-run /devspark-planning-review: it reuses what is still fresh and rechecks only what changed."
}
elseif ($specExists -and $isFullSpec -and $tasksExists -and -not $tasksComplete) {
    $recommended = "/devspark-implement"
    $state = "tasks-incomplete"
    $reason = "Tasks are defined ($tasksDone/$tasksTotal complete) but not finished. Run /devspark-implement to execute the remaining tasks."
}
elseif ($specExists -and $isFullSpec -and $tasksComplete -and $verifyRequired -and -not $verifyPassed) {
    $recommended = "/devspark-verify"
    $state = "implementation-needs-verification"
    $reason = "Implementation tasks are complete; declared Verify modes still need passing evidence. Run /devspark-verify, including its Closeout phase."
}
elseif ($specExists -and $isFullSpec -and $tasksComplete -and $closeoutDecision -eq "not-complete") {
    $recommended = "/devspark-implement"
    $state = "closeout-not-complete"
    $reason = "Closeout records blocking defects. Resume approved defect handling; any semantic planning change must stop for Plan and explicit authority."
}
elseif ($specExists -and $isFullSpec -and $tasksComplete -and $verifyRequired -and $closeoutDecision -ne "complete") {
    $recommended = "/devspark-verify"
    $state = "verification-needs-closeout"
    $reason = "Required proof has passed but Closeout is missing or unresolved. Run /devspark-verify to complete the convergence phase; retain the planning bundle."
}
elseif ($specExists -and -not $isFullSpec -and $specStatus -ne "Complete") {
    $recommended = "/devspark-implement"
    $state = "quickspec-needs-implement"
    $statusLabel = if ($specStatus) { $specStatus } else { "Draft" }
    $reason = "A temporary quick-spec exists for '$currentBranch' (Status: $statusLabel). Run /devspark-implement to finish its action plan, verify the durable result, and retain the bundle."
}
elseif ($quickfixExists -and -not $quickfixComplete -and -not $prExists) {
    $recommended = "/devspark-implement"
    $state = "quickfix-open"
    $reason = "A temporary quickfix record exists for '$currentBranch'. Run /devspark-implement to finish the change, verify code, tests, and current knowledge, and retain the record."
}
elseif ($dirty) {
    $recommended = "commit"
    $state = "uncommitted-changes"
    $reason = "You have uncommitted changes on '$currentBranch'. Commit your work (then re-run /devspark.next). If this is a spec route mid-implementation, continue with /devspark-implement."
}
elseif (-not $prExists) {
    $recommended = "/devspark-create-pr"
    $state = "ready-for-pr"
    $reason = "Work is committed on '$currentBranch' and no PR exists yet. Open one with /devspark-create-pr (it will publish the branch and let you optionally run /devspark-verify)."
}
elseif ($prExists -and -not $reviewFileExists) {
    $recommended = "/devspark-pr-review"
    $state = "pr-needs-review"
    $reason = "PR #$prNumber is open but has no review yet. Run /devspark-pr-review to review it against the constitution."
}
elseif ($reviewFileExists -and $reviewOpenFindings) {
    $recommended = "/devspark-address-pr-review"
    $state = "review-has-open-findings"
    $reason = "PR #$prNumber has open review findings. Run /devspark-address-pr-review to fix them, then re-review with /devspark-pr-review #$prNumber re-review."
}
elseif ($reviewFileExists -and $behindTarget) {
    $recommended = "sync"
    $state = "behind-target"
    $reason = "Review looks clear but '$currentBranch' is behind '$defaultBranch'. Sync it (merge origin/$defaultBranch in) before merging the PR."
}
elseif ($reviewFileExists) {
    $recommended = "merge"
    $state = "ready-to-merge"
    $reason = "PR #$prNumber is reviewed and the branch is in sync. Approve and merge it in your Git portal (GitHub / Azure DevOps)."
}

# Surface still-open advisory gates without blocking the primary recommendation.
if ($pendingGatesStr -and $recommended -notmatch 'analyze|critic|checklist|verify|constitution|planning-review') {
    $reason = "$reason Open advisory gates still recommended before merge: $pendingGatesStr."
}

$result = [ordered]@{
    REPO_ROOT           = $repoRoot
    REPO_NAME           = $repoName
    PLATFORM            = $platform
    ASSISTANT_NAME      = $assistantName
    GIT_USER            = $gitUser
    HAS_GIT             = $hasGit
    CONSTITUTION_EXISTS = $constitutionExists
    CURRENT_BRANCH      = $currentBranch
    DEFAULT_BRANCH      = $defaultBranch
    IS_DEFAULT_BRANCH   = $isDefaultBranch
    DIRTY               = $dirty
    HAS_REMOTE          = $hasRemote
    BRANCH_PUSHED       = $branchPushed
    BEHIND_TARGET       = $behindTarget
    SPEC_EXISTS         = $specExists
    SPEC_FEATURE        = $specFeature
    SPEC_AMBIGUOUS      = $specAmbiguous
    IS_FULL_SPEC        = $isFullSpec
    SPEC_STATUS         = $specStatus
    PLAN_EXISTS         = $planExists
    TASKS_EXISTS        = $tasksExists
    TASKS_TOTAL         = $tasksTotal
    TASKS_DONE          = $tasksDone
    TASKS_COMPLETE      = $tasksComplete
    VERIFY_REQUIRED     = $verifyRequired
    VERIFY_PASSED       = $verifyPassed
    CLOSEOUT_DECISION   = $closeoutDecision
    JUDGE_VERDICT       = $judgeVerdict
    JUDGE_NEEDS_RECHECK = $judgeNeedsRecheck
    PLAN_SPEC_STATE     = $planSpecState
    TASKS_PLAN_STATE    = $tasksPlanState
    PENDING_GATES       = $pendingGatesStr
    QUICKFIX_EXISTS     = $quickfixExists
    QUICKFIX_COMPLETE   = $quickfixComplete
    PR_EXISTS           = $prExists
    PR_NUMBER           = $prNumber
    PR_STATE            = $prState
    REVIEW_FILE_EXISTS  = $reviewFileExists
    REVIEW_OPEN         = $reviewOpenFindings
    RECOMMENDED_COMMAND = $recommended
    STATE               = $state
    REASON              = $reason
}

if ($Json) {
    $result | ConvertTo-Json
}
else {
    Write-Output "Next-Step Router"
    Write-Output "================"
    Write-Output "Assistant: $assistantName"
    Write-Output "Repo: $repoName ($platform)"
    Write-Output "Branch: $currentBranch (default: $defaultBranch)"
    Write-Output "State: $state"
    Write-Output "Recommended: $recommended"
    Write-Output "Reason: $reason"
}

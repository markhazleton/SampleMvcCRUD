#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Next-step router context script.
# Detects the current lifecycle state locally (git + filesystem, best-effort gh)
# and recommends the single next DevSpark command to run.

set -u

SCRIPT_DIR="$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

JSON_MODE=false
for arg in "$@"; do
    case "$arg" in
        --json) JSON_MODE=true ;;
    esac
done

REPO_ROOT=$(get_repo_root)
CURRENT_BRANCH=$(get_current_branch)
ASSISTANT_NAME=$(get_assistant_name)
GIT_USER=$(get_git_user)

# Orientation facts ("where am I"): repo name + host platform, from config or remote.
REPO_NAME=$(get_devspark_config_value '.repository')
[[ -z "$REPO_NAME" ]] && REPO_NAME=$(basename "$REPO_ROOT")
PLATFORM=$(get_devspark_config_value '.platform')
if [[ -z "$PLATFORM" ]]; then
    PLATFORM="unknown"
    remote_url=$(git remote get-url origin 2>/dev/null || true)
    if [[ "$remote_url" == *dev.azure.com* || "$remote_url" == *visualstudio.com* ]]; then PLATFORM="azdo"
    elif [[ "$remote_url" == *github.com* ]]; then PLATFORM="github"
    elif [[ "$remote_url" == *gitlab* ]]; then PLATFORM="gitlab"
    fi
fi

HAS_GIT="false"
if has_git; then HAS_GIT="true"; fi

CONSTITUTION_PATH="$REPO_ROOT/.knowledge/governance/constitution.md"
CONSTITUTION_EXISTS="false"
[[ -f "$CONSTITUTION_PATH" ]] && CONSTITUTION_EXISTS="true"

# Default (target) branch detection
DEFAULT_BRANCH="main"
if [[ "$HAS_GIT" == "true" ]]; then
    HEAD_REF=$(git symbolic-ref --quiet --short refs/remotes/origin/HEAD 2>/dev/null || echo "")
    if [[ -n "$HEAD_REF" ]]; then
        DEFAULT_BRANCH="${HEAD_REF#origin/}"
    elif git show-ref --verify --quiet refs/heads/master 2>/dev/null; then
        DEFAULT_BRANCH="master"
    fi
fi

IS_DEFAULT_BRANCH="false"
if [[ "$CURRENT_BRANCH" == "$DEFAULT_BRANCH" || "$CURRENT_BRANCH" == "main" || "$CURRENT_BRANCH" == "master" ]]; then
    IS_DEFAULT_BRANCH="true"
fi

# Git state
DIRTY="false"
HAS_REMOTE="false"
BRANCH_PUSHED="false"
BEHIND_TARGET="false"
if [[ "$HAS_GIT" == "true" ]]; then
    [[ -n "$(git status --porcelain 2>/dev/null)" ]] && DIRTY="true"
    [[ -n "$(git remote 2>/dev/null)" ]] && HAS_REMOTE="true"
    if [[ "$HAS_REMOTE" == "true" ]]; then
        if git rev-parse --verify --quiet "refs/remotes/origin/$CURRENT_BRANCH" >/dev/null 2>&1; then
            BRANCH_PUSHED="true"
        fi
        BEHIND_COUNT=$(git rev-list --count "HEAD..origin/$DEFAULT_BRANCH" 2>/dev/null || echo "0")
        [[ "$BEHIND_COUNT" =~ ^[0-9]+$ && "$BEHIND_COUNT" -gt 0 ]] && BEHIND_TARGET="true"
    fi
fi

# Spec / quickfix detection for the current branch. The branch name may differ from the spec
# directory, so resolve by numeric prefix: an exact spec directory wins, a quickfix record for the
# branch means a quickfix (not a spec), otherwise exactly one spec directory with that prefix is the
# active spec. DEVSPARK_FEATURE overrides the branch. More than one match is reported, never guessed.
SPEC_DIR="$REPO_ROOT/.devspark.work/specs/$CURRENT_BRANCH"
SPEC_AMBIGUOUS="false"
SPEC_AMBIGUOUS_MATCHES=""
if [[ ! -f "$SPEC_DIR/spec.md" && "$CURRENT_BRANCH" =~ ^[0-9]{3}- && ! -f "$REPO_ROOT/.devspark.work/quickfixes/$CURRENT_BRANCH.md" ]]; then
    _prefix="${CURRENT_BRANCH:0:3}"
    _count=0
    _found=""
    for _d in "$REPO_ROOT/.devspark.work/specs/""$_prefix"-*; do
        if [[ -f "$_d/spec.md" ]]; then
            _count=$((_count + 1))
            _found="$_d"
            SPEC_AMBIGUOUS_MATCHES="${SPEC_AMBIGUOUS_MATCHES:+$SPEC_AMBIGUOUS_MATCHES, }$(basename "$_d")"
        fi
    done
    if [[ "$_count" -eq 1 ]]; then
        SPEC_DIR="${_found%/}"
    elif [[ "$_count" -gt 1 ]]; then
        SPEC_AMBIGUOUS="true"
    fi
fi
SPEC_EXISTS="false"
[[ "$SPEC_AMBIGUOUS" == "false" && -f "$SPEC_DIR/spec.md" ]] && SPEC_EXISTS="true"
SPEC_FEATURE=""
[[ "$SPEC_EXISTS" == "true" ]] && SPEC_FEATURE=$(basename "$SPEC_DIR")

# Spec authoring sub-state (plan -> tasks -> analyze -> critic -> implement -> verify)
PLAN_EXISTS="false"
TASKS_EXISTS="false"
TASKS_TOTAL=0
TASKS_DONE=0
TASKS_COMPLETE="false"
CLASSIFICATION=""
SPEC_STATUS=""
IS_FULL_SPEC="false"
REQUIRE_ANALYZE="false"
REQUIRE_CRITIC="false"
REQUIRE_CHECKLIST="false"
ANALYZE_GATE="false"
CRITIC_GATE="false"
JUDGE_GATE="false"
JUDGE_VERDICT=""
JUDGE_NEEDS_RECHECK="false"
JUDGE_PENDING_DECISIONS=0
PLAN_REVISION_COUNT=0
PLAN_CHANGED=null
TASKS_CHANGED=false
TASKS_STALE=true
PLAN_SPEC_STATE="unknown"
TASKS_PLAN_STATE="unknown"
JUDGE_INVALIDATED=false
REMEDIATION_APPENDED="false"
CHECKLIST_GATE="false"
VERIFY_REQUIRED="false"
VERIFY_PASSED="false"
CLOSEOUT_DECISION=""
if [[ "$SPEC_EXISTS" == "true" ]]; then
    [[ -f "$SPEC_DIR/plan.md" ]] && PLAN_EXISTS="true"
    if [[ -f "$SPEC_DIR/tasks.md" ]]; then
        TASKS_EXISTS="true"
        TASKS_TOTAL=$(grep -cE '^[[:space:]]*[-*][[:space:]]*\[[ xX]\]' "$SPEC_DIR/tasks.md" 2>/dev/null)
        TASKS_DONE=$(grep -cE '^[[:space:]]*[-*][[:space:]]*\[[xX]\]' "$SPEC_DIR/tasks.md" 2>/dev/null)
        [[ -z "$TASKS_TOTAL" ]] && TASKS_TOTAL=0
        [[ -z "$TASKS_DONE" ]] && TASKS_DONE=0
        if [[ "$TASKS_TOTAL" -gt 0 && "$TASKS_DONE" -ge "$TASKS_TOTAL" ]]; then TASKS_COMPLETE="true"; fi
        grep -qiE '^## Gate Remediation' "$SPEC_DIR/tasks.md" 2>/dev/null && REMEDIATION_APPENDED="true"
    fi
    CLASSIFICATION=$(grep -m1 -E '^classification:' "$SPEC_DIR/spec.md" 2>/dev/null | sed -E 's/^classification:[[:space:]]*//; s/[[:space:]]*$//')
    SPEC_STATUS=$(grep -m1 -E '^\*\*Status\*\*:' "$SPEC_DIR/spec.md" 2>/dev/null | sed -E 's/^\*\*Status\*\*:[[:space:]]*//; s/[[:space:]]*<!--.*//; s/[[:space:]]*$//')
    REQ_GATES=$(grep -m1 -E '^required_gates:' "$SPEC_DIR/spec.md" 2>/dev/null || echo "")
    [[ "$REQ_GATES" == *"verify:"* ]] && VERIFY_REQUIRED="true"
    [[ "$REQ_GATES" == *"analyze"* ]] && REQUIRE_ANALYZE="true"
    [[ "$REQ_GATES" == *"critic"* ]] && REQUIRE_CRITIC="true"
    [[ "$REQ_GATES" == *"checklist"* ]] && REQUIRE_CHECKLIST="true"
    if [[ "$CLASSIFICATION" == *"full-spec"* ]]; then
        IS_FULL_SPEC="true"
    elif [[ -z "$CLASSIFICATION" && ( "$PLAN_EXISTS" == "true" || "$TASKS_EXISTS" == "true" ) ]]; then
        IS_FULL_SPEC="true"
    fi
    [[ -f "$SPEC_DIR/gates/analyze.md" ]] && ANALYZE_GATE="true"
    [[ -f "$SPEC_DIR/gates/critic.md" ]] && CRITIC_GATE="true"
    if [[ -f "$SPEC_DIR/gates/judge.md" ]]; then
        JUDGE_GATE="true"
        JUDGE_VERDICT=$(grep -m1 -E '^verdict:' "$SPEC_DIR/gates/judge.md" 2>/dev/null | sed -E 's/^verdict:[[:space:]]*//; s/[[:space:]]*$//')
    fi
    [[ -f "$SPEC_DIR/gates/checklist.md" ]] && CHECKLIST_GATE="true"
    if [[ -f "$SPEC_DIR/gates/verify.md" ]] && grep -qE '^status:[[:space:]]*pass' "$SPEC_DIR/gates/verify.md" 2>/dev/null; then VERIFY_PASSED="true"; fi
    if [[ -f "$SPEC_DIR/gates/closeout.md" ]]; then
        CLOSEOUT_DECISION=$(grep -m1 -E '^decision:' "$SPEC_DIR/gates/closeout.md" 2>/dev/null | sed -E 's/^decision:[[:space:]]*//; s/[[:space:]]*$//')
    fi
    [[ "$VERIFY_REQUIRED" == "true" ]] && VERIFY_PASSED="false"
    _judge_py="$SCRIPT_DIR/../judge.py"
    if [[ -f "$_judge_py" ]] && command -v jq >/dev/null 2>&1; then
        _review_state=""
        for _py in python3 python; do
            _review_state=$("$_py" "$_judge_py" review-state --feature-dir "$SPEC_DIR" 2>/dev/null) && [[ -n "$_review_state" ]] && break
            _review_state=""
        done
        if [[ -n "$_review_state" ]] && jq -e 'type == "object"' <<<"$_review_state" >/dev/null 2>&1; then
            JUDGE_NEEDS_RECHECK=$(jq -r '.needs_recheck // false' <<<"$_review_state")
            JUDGE_PENDING_DECISIONS=$(jq -r '.pending_decisions // 0' <<<"$_review_state")
            PLAN_REVISION_COUNT=$(jq -r '.plan_revision_count // 0' <<<"$_review_state")
            PLAN_CHANGED=$(jq -r '.plan_changed' <<<"$_review_state")
            TASKS_CHANGED=$(jq -r '.tasks_changed // false' <<<"$_review_state")
            TASKS_STALE=$(jq -r '.tasks_stale' <<<"$_review_state")
            PLAN_SPEC_STATE=$(jq -r '.plan_spec_state // "unknown"' <<<"$_review_state")
            TASKS_PLAN_STATE=$(jq -r '.tasks_plan_state // "unknown"' <<<"$_review_state")
            JUDGE_INVALIDATED=$(jq -r '(.judge_stale == true) or (.spec_changed == true)' <<<"$_review_state")
            VERIFY_REQUIRED=$(jq -r '.verify_required // false' <<<"$_review_state")
            VERIFY_PASSED=$(jq -r '.verify_passed // false' <<<"$_review_state")
            CLOSEOUT_DECISION=$(jq -r '.closeout_decision // ""' <<<"$_review_state")
        fi
    fi
fi

# Edge routes yield to pending decisions, spec/Judge invalidation and STOP remediation.
PLANNING_EDGES_ROUTABLE="false"
if [[ "$JUDGE_GATE" == "false" || ( "$JUDGE_INVALIDATED" != "true" && "$JUDGE_VERDICT" != "STOP" ) ]]; then PLANNING_EDGES_ROUTABLE="true"; fi

# Advisory gates that are required but whose artifact is missing/unmet (never block progress)
PENDING_GATES=""
if [[ "$SPEC_EXISTS" == "true" ]]; then
    [[ "$REQUIRE_CHECKLIST" == "true" && "$CHECKLIST_GATE" == "false" ]] && PENDING_GATES="${PENDING_GATES:+$PENDING_GATES, }checklist"
    # Planning Review (judge.md) supersedes the separate analyze/critic artifacts: Critic may be not applicable.
    if [[ "$JUDGE_GATE" == "false" ]]; then
        [[ "$REQUIRE_ANALYZE" == "true" && "$ANALYZE_GATE" == "false" ]] && PENDING_GATES="${PENDING_GATES:+$PENDING_GATES, }analyze"
        [[ "$REQUIRE_CRITIC" == "true" && "$CRITIC_GATE" == "false" ]] && PENDING_GATES="${PENDING_GATES:+$PENDING_GATES, }critic"
    fi
    [[ "$VERIFY_REQUIRED" == "true" && "$VERIFY_PASSED" == "false" ]] && PENDING_GATES="${PENDING_GATES:+$PENDING_GATES, }verify"
    [[ "$CLOSEOUT_DECISION" == "not-complete" ]] && PENDING_GATES="${PENDING_GATES:+$PENDING_GATES, }closeout (blocking defects open)"
fi

QUICKFIX_EXISTS="false"
QUICKFIX_COMPLETE="false"
if [[ "$CURRENT_BRANCH" =~ ^QF- || "$CURRENT_BRANCH" =~ ^[0-9][0-9][0-9]-fix- ]]; then
    QUICKFIX_RECORD="$REPO_ROOT/.devspark.work/quickfixes/$CURRENT_BRANCH.md"
    if [[ -f "$QUICKFIX_RECORD" ]]; then
        QUICKFIX_EXISTS="true"
        if grep -qE '^[[:space:]]*-[[:space:]]*\*\*Completed\*\*:[[:space:]]*[^[:space:]]' "$QUICKFIX_RECORD" 2>/dev/null; then QUICKFIX_COMPLETE="true"; fi
    fi
fi

# Best-effort PR + review detection (platform-aware)
PR_EXISTS="false"
PR_NUMBER=""
PR_STATE=""
if [[ "$HAS_GIT" == "true" ]]; then
    if [[ "$PLATFORM" == "azdo" ]] && command -v az >/dev/null 2>&1; then
        IFS=$'\t' read -r NAZ_ORG NAZ_PROJ NAZ_REPO < <(get_azdo_repo_context)
        AZ_PR_JSON=$(az repos pr list --organization "https://dev.azure.com/$NAZ_ORG" --project "$NAZ_PROJ" --repository "$NAZ_REPO" --source-branch "$CURRENT_BRANCH" --status active --top 1 --output json 2>/dev/null | jq '.[0] // {}' 2>/dev/null || echo '{}')
        PR_NUMBER=$(echo "$AZ_PR_JSON" | jq -r '.pullRequestId // empty')
        PR_STATE=$(echo "$AZ_PR_JSON" | jq -r '.status // empty')
        [[ -n "$PR_NUMBER" ]] && PR_EXISTS="true"
    elif command -v gh >/dev/null 2>&1; then
        PR_JSON=$(gh pr view --json number,state 2>/dev/null || echo "")
        if [[ -n "$PR_JSON" ]]; then
            PR_NUMBER=$(echo "$PR_JSON" | jq -r '.number // empty' 2>/dev/null || echo "")
            PR_STATE=$(echo "$PR_JSON" | jq -r '.state // empty' 2>/dev/null || echo "")
            [[ -n "$PR_NUMBER" ]] && PR_EXISTS="true"
        fi
    fi
fi

REVIEW_FILE_EXISTS="false"
REVIEW_OPEN="false"
REVIEW_DIR="$REPO_ROOT/.devspark.work/pr-review"
if [[ -d "$REVIEW_DIR" ]]; then
    REVIEW_FILE=""
    if [[ -n "$PR_NUMBER" && -f "$REVIEW_DIR/pr-$PR_NUMBER.md" ]]; then
        REVIEW_FILE="$REVIEW_DIR/pr-$PR_NUMBER.md"
    else
        REVIEW_FILE=$(ls -t "$REVIEW_DIR"/pr-*.md 2>/dev/null | head -1 || echo "")
    fi
    if [[ -n "$REVIEW_FILE" && -f "$REVIEW_FILE" ]]; then
        REVIEW_FILE_EXISTS="true"
        grep -q "Open" "$REVIEW_FILE" 2>/dev/null && REVIEW_OPEN="true"
    fi
fi

# ---- Recommendation ladder ----
RECOMMENDED="/devspark-specify"
STATE="unknown"
REASON="Describe the work to /devspark-specify and it will route you."

if [[ "$CONSTITUTION_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-constitution"
    STATE="no-constitution"
    REASON="No project constitution found. Create your principles first so every command has a validation baseline."
elif [[ "$SPEC_AMBIGUOUS" == "true" ]]; then
    RECOMMENDED="resolve"
    STATE="ambiguous-spec"
    REASON="Branch '$CURRENT_BRANCH' matches more than one spec directory ($SPEC_AMBIGUOUS_MATCHES). Set DEVSPARK_FEATURE to the intended directory name, or remove the duplicate, then re-run /devspark.next."
elif [[ "$SPEC_EXISTS" == "true" && ( "$JUDGE_VERDICT" == "CONDITIONAL" || "$JUDGE_PENDING_DECISIONS" -gt 0 ) ]]; then
    RECOMMENDED="/devspark-planning-review"
    STATE="review-needs-decision"
    REASON="A human planning decision is pending. Answer it before remediation or implementation, then use /devspark-planning-review --judge-only to record the disposition."
elif [[ "$IS_DEFAULT_BRANCH" == "true" && "$SPEC_EXISTS" == "false" && "$QUICKFIX_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-specify"
    STATE="no-active-work"
    REASON="You're on '$CURRENT_BRANCH' with no active spec or quickfix. Start with /devspark-specify (unsure of size) or /devspark-quickfix (small, contained fix)."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$PLAN_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-plan"
    STATE="spec-needs-plan"
    REASON="Spec exists for '$CURRENT_BRANCH' but there's no plan yet. Run /devspark-plan to produce the technical design."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$PLAN_EXISTS" == "true" && "$PLAN_SPEC_STATE" == "stale" && "$PLANNING_EDGES_ROUTABLE" == "true" ]]; then
    RECOMMENDED="/devspark-plan"
    STATE="plan-stale-against-spec"
    REASON="The spec changed in meaning since the plan was produced or last checked. Run /devspark-plan: it checks the existing plan against the current spec and changes only what the spec requires."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$PLAN_EXISTS" == "true" && "$TASKS_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-tasks"
    STATE="plan-needs-tasks"
    REASON="Plan exists but there's no task breakdown. Run /devspark-tasks to generate the ordered task list."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$TASKS_PLAN_STATE" == "stale" && "$PLANNING_EDGES_ROUTABLE" == "true" ]]; then
    RECOMMENDED="/devspark-tasks"
    STATE="tasks-stale-against-plan"
    REASON="The plan changed in meaning since the task list was projected. Run /devspark-tasks: it checks the task list against the current plan and re-projects only unchecked work."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_GATE" == "false" && ( "$REQUIRE_ANALYZE" == "true" || "$REQUIRE_CRITIC" == "true" ) ]]; then
    RECOMMENDED="/devspark-planning-review"
    STATE="tasks-need-review"
    REASON="Tasks are ready and this full-spec route requires planning assurance. Run /devspark-planning-review: it runs Analyze, Critic when applicable, and Judge, and returns one planning decision before implementing."
# Unresolved Planning Review outranks task progress: a finished task list never overrides open assurance.
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_INVALIDATED" == "true" ]]; then
    RECOMMENDED="/devspark-planning-review"
    STATE="review-needs-recheck"
    REASON="The specification or adjudicated child evidence changed. Re-establish planning authority before applying corrections."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_VERDICT" == "STOP" && "$PLAN_REVISION_COUNT" -gt 0 && "$PLAN_CHANGED" == "true" && "$TASKS_STALE" == "true" ]]; then
    RECOMMENDED="/devspark-tasks"
    STATE="plan-needs-reprojection"
    REASON="Plan semantics changed. Re-project unchecked Tasks from the approved Plan and protect completed work before focused review."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_VERDICT" == "STOP" && "$PLAN_REVISION_COUNT" -gt 0 && "$PLAN_CHANGED" == "false" ]]; then
    RECOMMENDED="/devspark-plan"
    STATE="review-needs-plan"
    REASON="Judge assigned an approved Plan correction. Run /devspark-plan in focused revise mode; preserve unrelated decisions and never reset automatically."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_VERDICT" == "STOP" && ( "$PLAN_CHANGED" == "true" || "$TASKS_CHANGED" == "true" || "$REMEDIATION_APPENDED" == "true" || ( "$PLAN_REVISION_COUNT" -gt 0 && "$PLAN_CHANGED" == "null" ) ) ]]; then
    RECOMMENDED="/devspark-planning-review"
    STATE="review-needs-recheck"
    REASON="Correction inputs changed or their provenance is unknown. Re-run /devspark-planning-review for a focused check; changed content does not resolve STOP by itself."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_VERDICT" == "STOP" ]]; then
    RECOMMENDED="/devspark-tasks"
    STATE="review-blocked"
    REASON="Planning review returned STOP. Run /devspark-tasks for validated task-owned corrections only; missing ownership or scope authority must stop for a decision."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$JUDGE_NEEDS_RECHECK" == "true" ]]; then
    RECOMMENDED="/devspark-planning-review"
    STATE="review-needs-recheck"
    REASON="A reviewed artifact changed in a way that invalidates the Planning Review evidence. Re-run /devspark-planning-review: it reuses what is still fresh and rechecks only what changed."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_EXISTS" == "true" && "$TASKS_COMPLETE" == "false" ]]; then
    RECOMMENDED="/devspark-implement"
    STATE="tasks-incomplete"
    REASON="Tasks are defined ($TASKS_DONE/$TASKS_TOTAL complete) but not finished. Run /devspark-implement to execute the remaining tasks."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_COMPLETE" == "true" && "$VERIFY_REQUIRED" == "true" && "$VERIFY_PASSED" == "false" ]]; then
    RECOMMENDED="/devspark-verify"
    STATE="implementation-needs-verification"
    REASON="Implementation tasks are complete; declared Verify modes still need passing evidence. Run /devspark-verify, including its Closeout phase."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_COMPLETE" == "true" && "$CLOSEOUT_DECISION" == "not-complete" ]]; then
    RECOMMENDED="/devspark-implement"
    STATE="closeout-not-complete"
    REASON="Closeout records blocking defects. Resume approved defect handling; any semantic planning change must stop for Plan and explicit authority."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "true" && "$TASKS_COMPLETE" == "true" && "$VERIFY_REQUIRED" == "true" && "$CLOSEOUT_DECISION" != "complete" ]]; then
    RECOMMENDED="/devspark-verify"
    STATE="verification-needs-closeout"
    REASON="Required proof has passed but Closeout is missing or unresolved. Run /devspark-verify to complete the convergence phase; retain the planning bundle."
elif [[ "$SPEC_EXISTS" == "true" && "$IS_FULL_SPEC" == "false" && "$SPEC_STATUS" != "Complete" ]]; then
    RECOMMENDED="/devspark-implement"
    STATE="quickspec-needs-implement"
    REASON="A temporary quick-spec exists for '$CURRENT_BRANCH' (Status: ${SPEC_STATUS:-Draft}). Run /devspark-implement to finish its action plan, verify the durable result, and retain the bundle."
elif [[ "$QUICKFIX_EXISTS" == "true" && "$QUICKFIX_COMPLETE" == "false" && "$PR_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-implement"
    STATE="quickfix-open"
    REASON="A temporary quickfix record exists for '$CURRENT_BRANCH'. Run /devspark-implement to finish the change, verify code, tests, and current knowledge, and retain the record."
elif [[ "$DIRTY" == "true" ]]; then
    RECOMMENDED="commit"
    STATE="uncommitted-changes"
    REASON="You have uncommitted changes on '$CURRENT_BRANCH'. Commit your work (then re-run /devspark.next). If this is a spec route mid-implementation, continue with /devspark-implement."
elif [[ "$PR_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-create-pr"
    STATE="ready-for-pr"
    REASON="Work is committed on '$CURRENT_BRANCH' and no PR exists yet. Open one with /devspark-create-pr (it will publish the branch and let you optionally run /devspark-verify)."
elif [[ "$PR_EXISTS" == "true" && "$REVIEW_FILE_EXISTS" == "false" ]]; then
    RECOMMENDED="/devspark-pr-review"
    STATE="pr-needs-review"
    REASON="PR #$PR_NUMBER is open but has no review yet. Run /devspark-pr-review to review it against the constitution."
elif [[ "$REVIEW_FILE_EXISTS" == "true" && "$REVIEW_OPEN" == "true" ]]; then
    RECOMMENDED="/devspark-address-pr-review"
    STATE="review-has-open-findings"
    REASON="PR #$PR_NUMBER has open review findings. Run /devspark-address-pr-review to fix them, then re-review with /devspark-pr-review #$PR_NUMBER re-review."
elif [[ "$REVIEW_FILE_EXISTS" == "true" && "$BEHIND_TARGET" == "true" ]]; then
    RECOMMENDED="sync"
    STATE="behind-target"
    REASON="Review looks clear but '$CURRENT_BRANCH' is behind '$DEFAULT_BRANCH'. Sync it (merge origin/$DEFAULT_BRANCH in) before merging the PR."
elif [[ "$REVIEW_FILE_EXISTS" == "true" ]]; then
    RECOMMENDED="merge"
    STATE="ready-to-merge"
    REASON="PR #$PR_NUMBER is reviewed and the branch is in sync. Approve and merge it in your Git portal (GitHub / Azure DevOps)."
fi

# Surface still-open advisory gates without blocking the primary recommendation.
if [[ -n "$PENDING_GATES" && ! "$RECOMMENDED" =~ (analyze|critic|checklist|verify|constitution|planning-review) ]]; then
    REASON="$REASON Open advisory gates still recommended before merge: $PENDING_GATES."
fi

if [[ "$JSON_MODE" == true ]]; then
    cat <<EOF
{
  "REPO_ROOT": "$REPO_ROOT",
  "REPO_NAME": "$REPO_NAME",
  "PLATFORM": "$PLATFORM",
  "ASSISTANT_NAME": "$ASSISTANT_NAME",
  "GIT_USER": "$GIT_USER",
  "HAS_GIT": $HAS_GIT,
  "CONSTITUTION_EXISTS": $CONSTITUTION_EXISTS,
  "CURRENT_BRANCH": "$CURRENT_BRANCH",
  "DEFAULT_BRANCH": "$DEFAULT_BRANCH",
  "IS_DEFAULT_BRANCH": $IS_DEFAULT_BRANCH,
  "DIRTY": $DIRTY,
  "HAS_REMOTE": $HAS_REMOTE,
  "BRANCH_PUSHED": $BRANCH_PUSHED,
  "BEHIND_TARGET": $BEHIND_TARGET,
  "SPEC_EXISTS": $SPEC_EXISTS,
  "SPEC_FEATURE": "$SPEC_FEATURE",
  "SPEC_AMBIGUOUS": $SPEC_AMBIGUOUS,
  "IS_FULL_SPEC": $IS_FULL_SPEC,
  "SPEC_STATUS": "$SPEC_STATUS",
  "PLAN_EXISTS": $PLAN_EXISTS,
  "TASKS_EXISTS": $TASKS_EXISTS,
  "TASKS_TOTAL": $TASKS_TOTAL,
  "TASKS_DONE": $TASKS_DONE,
  "TASKS_COMPLETE": $TASKS_COMPLETE,
  "VERIFY_REQUIRED": $VERIFY_REQUIRED,
  "VERIFY_PASSED": $VERIFY_PASSED,
  "CLOSEOUT_DECISION": "$CLOSEOUT_DECISION",
  "JUDGE_VERDICT": "$JUDGE_VERDICT",
  "JUDGE_NEEDS_RECHECK": $JUDGE_NEEDS_RECHECK,
  "PLAN_SPEC_STATE": "$PLAN_SPEC_STATE",
  "TASKS_PLAN_STATE": "$TASKS_PLAN_STATE",
  "PENDING_GATES": $(echo "$PENDING_GATES" | jq -R -s '.' 2>/dev/null || echo '""'),
  "QUICKFIX_EXISTS": $QUICKFIX_EXISTS,
  "QUICKFIX_COMPLETE": $QUICKFIX_COMPLETE,
  "PR_EXISTS": $PR_EXISTS,
  "PR_NUMBER": "$PR_NUMBER",
  "PR_STATE": "$PR_STATE",
  "REVIEW_FILE_EXISTS": $REVIEW_FILE_EXISTS,
  "REVIEW_OPEN": $REVIEW_OPEN,
  "RECOMMENDED_COMMAND": "$RECOMMENDED",
  "STATE": "$STATE",
  "REASON": $(echo "$REASON" | jq -R -s '.')
}
EOF
else
    echo "Next-Step Router"
    echo "================"
    echo "Assistant: $ASSISTANT_NAME"
    echo "Repo: $REPO_NAME ($PLATFORM)"
    echo "Branch: $CURRENT_BRANCH (default: $DEFAULT_BRANCH)"
    echo "State: $STATE"
    echo "Recommended: $RECOMMENDED"
    echo "Reason: $REASON"
fi

#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
#
# Deterministic backstop for command-preamble-contract.md §0: durable code, tests, and
# .knowledge/ must never reference the ephemeral planning bundle that produced them. Prose
# instructions in implement.md/pr-review.md/site-audit.md ask the agent to self-check this on
# every run; this script makes it a real, repeatable gate instead of relying on the agent
# remembering, since a leaked reference (a spec/task/quickfix ID, or the branch name itself) has
# kept recurring even with the prose instruction in place.
#
# Parity twin of scripts/powershell/check-planning-references.ps1 -- keep both in sync.

set -euo pipefail

REPO_ROOT="."
BRANCH=""
JSON=false
INCLUDE_TESTS=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --repo-root) REPO_ROOT="$2"; shift 2 ;;
        --branch) BRANCH="$2"; shift 2 ;;
        --include-tests) INCLUDE_TESTS=true; shift ;;
        --json) JSON=true; shift ;;
        *) echo "Unknown argument: $1" >&2; exit 2 ;;
    esac
done

cd "$REPO_ROOT"

if [[ -z "$BRANCH" ]]; then
    BRANCH="$(git branch --show-current 2>/dev/null || true)"
fi

# Roots this rule never applies to: the ephemeral planning roots themselves, the installed
# framework payload, vendor/build output, and two meta-documents whose entire purpose is to
# summarize active/historical work by design (AGENTS.md's rolling "Recent Changes", CHANGELOG.md's
# release notes) -- excluded from both tiers below.
ALWAYS_EXCLUDED=(".devspark.work/" ".archive/" ".devspark/" ".git/" ".venv/" "node_modules/" "site/_site/" ".pytest_cache/")
ALWAYS_EXCLUDED_FILES=("AGENTS.md" "CHANGELOG.md")
# templates/, .github/ (shims generated from templates/), and tests/ legitimately document or
# exercise the ID conventions themselves (e.g. tasks-template.md's illustrative task/requirement IDs,
# a generated shim's example command invocation, or this repo's own tests asserting spec/task/
# quickfix numbering and parsing behavior) -- excluded only from the generic-ID-pattern tier
# below, never from the exact-branch-name tier, since a real feature's own branch name should
# never legitimately appear in either. tests/ is deliberately excluded by default to keep the
# routine gate low-noise; pass --include-tests for a stricter, human-reviewed audit pass, since a
# real leak can hide in a test file's own comment/docstring rather than its fixture data.
GENERIC_ID_EXCLUDED=("${ALWAYS_EXCLUDED[@]}" "templates/" ".github/" "site/src/")
[[ "$INCLUDE_TESTS" == false ]] && GENERIC_ID_EXCLUDED+=("tests/")
ID_PATTERN='\bFR-[0-9]{3}\b|\bT[0-9]{3}\b|\bQF-[0-9]{4}-[0-9]{3}\b'
# Complements the backward-link checks above: the ephemeral record itself MUST link forward to
# every completed item's code/knowledge, or the traceability this contract exists for is
# one-sided. Only tasks.md and quickfix records use this exact marker (quick-spec's Action Plan
# does not), so this pass targets those two file shapes specifically rather than every tracked file.
LINKAGE_PATTERN='^- \[[Xx]\].*\(code_ref:[[:space:]]*([^|]+[^| ])[[:space:]]*\|[[:space:]]*knowledge_ref:[[:space:]]*([^)]+[^) ])[[:space:]]*\)[[:space:]]*$'

is_excluded() {
    local path="$1"; shift
    for prefix in "$@"; do
        [[ "$path" == "$prefix"* ]] && return 0
    done
    return 1
}

is_excluded_file() {
    local path="$1"
    for name in "${ALWAYS_EXCLUDED_FILES[@]}"; do
        [[ "$path" == "$name" ]] && return 0
    done
    return 1
}

trim() {
    local value="$1"
    value="${value#"${value%%[![:space:]]*}"}"
    value="${value%"${value##*[![:space:]]}"}"
    printf '%s' "$value"
}

BRANCH_FINDINGS=()
ID_FINDINGS=()
LINKAGE_FINDINGS=()

while IFS= read -r file; do
    is_excluded "$file" "${ALWAYS_EXCLUDED[@]}" && continue
    is_excluded_file "$file" && continue
    in_generic_scope=true
    is_excluded "$file" "${GENERIC_ID_EXCLUDED[@]}" && in_generic_scope=false

    if [[ -n "$BRANCH" ]]; then
        while IFS=: read -r line_number _; do
            BRANCH_FINDINGS+=("$file:$line_number:$BRANCH")
        done < <(grep -Ins -F -- "$BRANCH" "$file" 2>/dev/null || true)
    fi

    if [[ "$in_generic_scope" == true ]]; then
        while IFS=: read -r line_number match; do
            ID_FINDINGS+=("$file:$line_number:$match")
        done < <(grep -InsoE "$ID_PATTERN" "$file" 2>/dev/null || true)
    fi
done < <(git ls-files)

# Second pass, deliberately separate from the loop above: tasks.md and quickfix records live
# under .devspark.work/, which the loop above always skips (it is the ephemeral record's own
# linkage target list, not a backward-link risk). A completed item left at `pending` is a real
# gap the same way a leaked reference is -- just the other direction of the same contract.
while IFS= read -r file; do
    [[ -z "$file" ]] && continue
    line_number=0
    while IFS= read -r line; do
        line_number=$((line_number + 1))
        if [[ "$line" =~ $LINKAGE_PATTERN ]]; then
            coderef="$(trim "${BASH_REMATCH[1]}")"
            kref="$(trim "${BASH_REMATCH[2]}")"
            [[ "${coderef,,}" == "pending" ]] && LINKAGE_FINDINGS+=("$file:$line_number:code_ref")
            [[ "${kref,,}" == "pending" ]] && LINKAGE_FINDINGS+=("$file:$line_number:knowledge_ref")
        fi
    done < "$file"
done < <(git ls-files -- '.devspark.work/specs/*/tasks.md' '.devspark.work/quickfixes/*.md')

OK=true
[[ ${#BRANCH_FINDINGS[@]} -gt 0 || ${#ID_FINDINGS[@]} -gt 0 || ${#LINKAGE_FINDINGS[@]} -gt 0 ]] && OK=false

if [[ "$JSON" == true ]]; then
    branch_json="[]"
    if [[ ${#BRANCH_FINDINGS[@]} -gt 0 ]]; then
        branch_json="["
        for i in "${!BRANCH_FINDINGS[@]}"; do
            IFS=: read -r p l m <<< "${BRANCH_FINDINGS[$i]}"
            [[ $i -gt 0 ]] && branch_json+=","
            branch_json+="{\"path\":\"$p\",\"line\":$l,\"match\":\"$m\"}"
        done
        branch_json+="]"
    fi
    id_json="[]"
    if [[ ${#ID_FINDINGS[@]} -gt 0 ]]; then
        id_json="["
        for i in "${!ID_FINDINGS[@]}"; do
            IFS=: read -r p l m <<< "${ID_FINDINGS[$i]}"
            [[ $i -gt 0 ]] && id_json+=","
            id_json+="{\"path\":\"$p\",\"line\":$l,\"match\":\"$m\"}"
        done
        id_json+="]"
    fi
    linkage_json="[]"
    if [[ ${#LINKAGE_FINDINGS[@]} -gt 0 ]]; then
        linkage_json="["
        for i in "${!LINKAGE_FINDINGS[@]}"; do
            IFS=: read -r p l f <<< "${LINKAGE_FINDINGS[$i]}"
            [[ $i -gt 0 ]] && linkage_json+=","
            linkage_json+="{\"path\":\"$p\",\"line\":$l,\"field\":\"$f\"}"
        done
        linkage_json+="]"
    fi
    printf '{"contract": 1, "branch": "%s", "branch_findings": %s, "id_findings": %s, "linkage_findings": %s, "ok": %s}\n' \
        "$BRANCH" "$branch_json" "$id_json" "$linkage_json" "$OK"
else
    for f in "${BRANCH_FINDINGS[@]+"${BRANCH_FINDINGS[@]}"}"; do
        IFS=: read -r p l m <<< "$f"
        echo "PLANNING-REF: $p:$l references the current branch name '$m'"
    done
    for f in "${ID_FINDINGS[@]+"${ID_FINDINGS[@]}"}"; do
        IFS=: read -r p l m <<< "$f"
        echo "PLANNING-REF: $p:$l references planning identifier '$m'"
    done
    for f in "${LINKAGE_FINDINGS[@]+"${LINKAGE_FINDINGS[@]}"}"; do
        IFS=: read -r p l fd <<< "$f"
        echo "LINKAGE-GAP: $p:$l is checked off but $fd is still 'pending'"
    done
    [[ "$OK" == true ]] && echo "No planning-artifact references found."
fi

[[ "$OK" == true ]] && exit 0 || exit 1

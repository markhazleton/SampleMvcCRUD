#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com

set -e

# Parse command line arguments
JSON_MODE=false
RESET=false
ARGS=()

for arg in "$@"; do
    case "$arg" in
        --json)
            JSON_MODE=true
            ;;
        --reset)
            RESET=true
            ;;
        --help|-h)
            echo "Usage: $0 [--json] [--reset]"
            echo "  --json    Output results in JSON format"
            echo "  --reset   Explicitly replace the existing plan with the resolved template"
            echo "  --help    Show this help message"
            exit 0
            ;;
        *)
            ARGS+=("$arg")
            ;;
    esac
done

# Get script directory and load common functions
SCRIPT_DIR="$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

# Parse multi-app context if present
parse_app_context "${ARGS[@]}" 2>/dev/null || true
if [[ -n "${DEVSPARK_APP_ID:-}" || "${DEVSPARK_REPO_SCOPE:-false}" == "true" ]]; then
    resolve_app_scope 2>/dev/null || true
fi

# Get all paths and variables — use app-aware version if scope is resolved
if [[ -n "${DEVSPARK_SCOPE:-}" ]]; then
    eval "$(get_feature_paths_app_aware)"
    print_scope_summary >&2
else
    eval "$(get_feature_paths)"
fi

# Check if we're on a proper feature branch (only for git repos)
check_feature_branch "$CURRENT_BRANCH" "$HAS_GIT" || exit 1

# Ensure the feature directory exists
mkdir -p "$FEATURE_DIR"

# Copy plan template from override/stock fallback chain if it exists
TEMPLATE="$(resolve_template_path "$REPO_ROOT" "plan-template.md" || true)"
PLAN_EXISTS=false
PLAN_MODE=create
if [[ -e "$IMPL_PLAN" ]]; then PLAN_EXISTS=true; PLAN_MODE=revise; fi
if $RESET; then PLAN_MODE=reset; fi
if $PLAN_EXISTS && ! $RESET; then
    if ! $JSON_MODE; then echo "Preserved existing plan at $IMPL_PLAN"; fi
elif [[ -n "$TEMPLATE" && -f "$TEMPLATE" ]]; then
    cp "$TEMPLATE" "$IMPL_PLAN"
    echo "Copied plan template to $IMPL_PLAN"
else
    echo "[devspark] Warning: No plan-template.md found in fallback chain; creating empty plan.md"
    # Create a basic plan file if template doesn't exist
    : > "$IMPL_PLAN"
fi

# Knowledge documents: parse
# functional requirements out of the now-populated spec.md and emit/refresh
# knowledge/fr-###.md + knowledge/feature.md's links.requirements. Gated by a
# git hash-object staleness check (research.md) stored as an internal comment
# in feature.md -- never part of the OKF frontmatter contract. Best-effort:
# any failure here must not affect this script's output or exit code.
{
    if [[ -f "$FEATURE_SPEC" ]]; then
        SPEC_HASH="$(git hash-object "$FEATURE_SPEC" 2>/dev/null || true)"
        FEATURE_DOC="$FEATURE_DIR/knowledge/feature.md"
        PRIOR_HASH=""
        if [[ -f "$FEATURE_DOC" ]]; then
            PRIOR_HASH="$(grep -m1 '^<!-- spec-hash: ' "$FEATURE_DOC" 2>/dev/null | sed -E 's/^<!-- spec-hash: (\S+) -->/\1/' || true)"
        fi
        if [[ -z "$SPEC_HASH" || "$PRIOR_HASH" != "$SPEC_HASH" ]]; then
            FR_IDS_JSON="[]"
            FR_IDS_ARR=()
            while IFS= read -r fr_line; do
                [[ -z "$fr_line" ]] && continue
                fr_id="$(echo "$fr_line" | sed -E 's/^- \*\*(FR-[0-9]+)\*\*:.*/\1/' | tr '[:upper:]' '[:lower:]')"
                # First-sentence heuristic: protect common abbreviations (e.g., i.e., etc.)
                # from being mistaken for a sentence boundary, split on the first real
                # ". " + uppercase, then restore (PR #61822 M-02, mirrors setup-plan.ps1).
                fr_title="$(echo "$fr_line" | sed -E 's/^- \*\*FR-[0-9]+\*\*:\s*//')"
                fr_title="$(printf '%s' "$fr_title" | sed -E 's/\b(e\.g|i\.e|etc)\./\1@DOT@/g')"
                fr_title="$(printf '%s' "$fr_title" | sed -E 's/\.[[:space:]]+[A-Z].*$//')"
                fr_title="$(printf '%s' "$fr_title" | sed -E 's/@DOT@/./g; s/\.$//')"
                FR_IDS_ARR+=("$fr_id")
                write_knowledge_document "$FEATURE_DIR" "$fr_id" requirement "$fr_title" draft \
                    '{"feature":"feature","implemented_by":[]}' setup-plan || true
            done < <(grep -E '^-\s+\*\*FR-[0-9]+\*\*:' "$FEATURE_SPEC" 2>/dev/null || true)

            if [[ ${#FR_IDS_ARR[@]} -gt 0 ]]; then
                FR_IDS_JSON="$(printf '%s\n' "${FR_IDS_ARR[@]}" | jq -R . | jq -s . 2>/dev/null || echo "[]")"
            fi

            FEATURE_TITLE="$CURRENT_BRANCH"
            EXTRACTED_TITLE="$(grep -m1 -E '^# Feature Specification:' "$FEATURE_SPEC" 2>/dev/null | sed -E 's/^# Feature Specification:\s*//' || true)"
            [[ -n "$EXTRACTED_TITLE" ]] && FEATURE_TITLE="$EXTRACTED_TITLE"

            # Reference (never copy) the spec sections that carry consumed spec knowledge; a
            # section the spec lacks is recorded as absent so no reference is left dangling.
            SECTIONS_PRESENT=()
            SECTIONS_ABSENT=()
            for section in 'Requirements' 'Key Entities' 'Clarifications' 'Success Criteria' 'Assumptions' 'Out of Scope' 'Minimum Sufficient Change'; do
                if grep -qiE "^#{2,3}[[:space:]]+${section}([[:space:]]|\*|$)" "$FEATURE_SPEC" 2>/dev/null; then
                    SECTIONS_PRESENT+=("$section")
                else
                    SECTIONS_ABSENT+=("$section")
                fi
            done
            PRESENT_JSON="$(printf '%s\n' "${SECTIONS_PRESENT[@]}" | jq -R . | jq -s . 2>/dev/null || echo '[]')"
            ABSENT_JSON="$(printf '%s\n' "${SECTIONS_ABSENT[@]}" | jq -R . | jq -s . 2>/dev/null || echo '[]')"
            [[ ${#SECTIONS_PRESENT[@]} -eq 0 ]] && PRESENT_JSON='[]'
            [[ ${#SECTIONS_ABSENT[@]} -eq 0 ]] && ABSENT_JSON='[]'
            LINKS_JSON="$(jq -cn --argjson reqs "$FR_IDS_JSON" --argjson present "$PRESENT_JSON" --argjson absent "$ABSENT_JSON" '{requirements:$reqs, spec_sections:$present, spec_sections_absent:$absent}' 2>/dev/null || echo '{"requirements":[]}')"
            if write_knowledge_document "$FEATURE_DIR" feature feature "$FEATURE_TITLE" draft "$LINKS_JSON" setup-plan && [[ -n "$SPEC_HASH" && -f "$FEATURE_DOC" ]]; then
                # Below the closing frontmatter delimiter: above it, the YAML block stops being
                # recognised and markdownlint reads the delimiters as setext headings.
                awk -v hash="$SPEC_HASH" 'BEGIN{n=0} {print} /^---[[:space:]]*$/{n++; if(n==2){print ""; print "<!-- spec-hash: " hash " -->"}}' "$FEATURE_DOC" > "$FEATURE_DOC.rehash.tmp" && mv -f "$FEATURE_DOC.rehash.tmp" "$FEATURE_DOC"
            fi
        fi
    fi
} 2>/dev/null || echo "[devspark] Warning: knowledge document refresh failed" >&2

# Output results
if $JSON_MODE; then
    printf '{"FEATURE_SPEC":"%s","IMPL_PLAN":"%s","SPECS_DIR":"%s","BRANCH":"%s","HAS_GIT":"%s","PLAN_EXISTS":%s,"PLAN_MODE":"%s"}\n' \
        "$FEATURE_SPEC" "$IMPL_PLAN" "$FEATURE_DIR" "$CURRENT_BRANCH" "$HAS_GIT" "$PLAN_EXISTS" "$PLAN_MODE"
else
    echo "FEATURE_SPEC: $FEATURE_SPEC"
    echo "IMPL_PLAN: $IMPL_PLAN"
    echo "SPECS_DIR: $FEATURE_DIR"
    echo "BRANCH: $CURRENT_BRANCH"
    echo "HAS_GIT: $HAS_GIT"
    echo "PLAN_EXISTS: $PLAN_EXISTS"
    echo "PLAN_MODE: $PLAN_MODE"
fi


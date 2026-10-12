#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Validates requirement<->task coverage across a feature's OKF knowledge
# documents . Mirrors
# validate-knowledge-coverage.ps1 exactly (Constitution SVI parity). See
# contracts/coverage-report-contract.md in that spec for the JSON shape.

set -e

JSON_MODE=false
FEATURE_DIR_ARG=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --json) JSON_MODE=true; shift ;;
        --feature)
            [[ $# -lt 2 ]] && { echo "Error: --feature requires a value" >&2; exit 1; }
            FEATURE_DIR_ARG="$2"; shift 2 ;;
        --help|-h)
            echo "Usage: $0 [--json] [--feature <feature-dir>]"
            echo "  --json              Output in JSON format"
            echo "  --feature <path>    Feature directory to check (defaults to the current feature)"
            exit 0 ;;
        *) shift ;;
    esac
done

SCRIPT_DIR="$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

if [[ -z "$FEATURE_DIR_ARG" ]]; then
    eval "$(get_feature_paths)"
    FEATURE_DIR_ARG="$FEATURE_DIR"
fi

# links_array_empty <frontmatter_file> <key>
# Returns "true"/"false" for whether the given links.<key> array is empty.
links_array_empty() {
    local file="$1"
    local key="$2"
    local inline
    inline="$(grep -m1 -E "^  ${key}:" "$file" 2>/dev/null || true)"
    if [[ "$inline" =~ \[\][[:space:]]*$ ]]; then
        echo "true"
        return
    fi
    # Block-style array: look for a "    - " line immediately following the key.
    local after
    after="$(awk -v key="  ${key}:" '
        found { print; next }
        $0 == key || $0 ~ ("^" key "[[:space:]]*$") { found=1 }
    ' "$file")"
    if [[ -n "$after" ]] && printf '%s\n' "$after" | grep -qE '^    - \S+'; then
        echo "false"
    else
        echo "true"
    fi
}

KNOWLEDGE_DIR="$FEATURE_DIR_ARG/knowledge"
ORPHAN_TASKS=()
UNIMPLEMENTED_REQUIREMENTS=()
TASK_COUNT=0

if [[ -d "$KNOWLEDGE_DIR" ]]; then
    for doc in "$KNOWLEDGE_DIR"/*.md; do
        [[ -f "$doc" ]] || continue
        frontmatter_file="$(mktemp)"
        get_markdown_frontmatter "$doc" > "$frontmatter_file" 2>/dev/null || true
        [[ -s "$frontmatter_file" ]] || { rm -f "$frontmatter_file"; continue; }

        doc_type="$(grep -m1 -E '^type:' "$frontmatter_file" | sed -E 's/^type:\s*//')"
        doc_id="$(grep -m1 -E '^id:' "$frontmatter_file" | sed -E 's/^id:\s*//')"
        [[ -z "$doc_id" ]] && doc_id="$(basename "$doc" .md)"

        if [[ "$doc_type" == "requirement" ]]; then
            if [[ "$(links_array_empty "$frontmatter_file" "implemented_by")" == "true" ]]; then
                UNIMPLEMENTED_REQUIREMENTS+=("$doc_id")
            fi
        elif [[ "$doc_type" == "task" ]]; then
            TASK_COUNT=$((TASK_COUNT + 1))
            if [[ "$(links_array_empty "$frontmatter_file" "implements")" == "true" ]]; then
                ORPHAN_TASKS+=("$doc_id")
            fi
        fi
        rm -f "$frontmatter_file"
    done
fi

# An unimplemented requirement is only a finding once at
# least one task document exists for the feature.
if [[ "$TASK_COUNT" -eq 0 ]]; then
    UNIMPLEMENTED_REQUIREMENTS=()
fi

FEATURE_ID="$(basename "$FEATURE_DIR_ARG")"
OK="true"
[[ ${#ORPHAN_TASKS[@]} -gt 0 || ${#UNIMPLEMENTED_REQUIREMENTS[@]} -gt 0 ]] && OK="false"

if $JSON_MODE; then
    if command -v jq >/dev/null 2>&1; then
        jq -cn \
            --arg fid "$FEATURE_ID" \
            --argjson orphans "$(printf '%s\n' "${ORPHAN_TASKS[@]:-}" | jq -R . | jq -s 'map(select(length > 0))')" \
            --argjson unimpl "$(printf '%s\n' "${UNIMPLEMENTED_REQUIREMENTS[@]:-}" | jq -R . | jq -s 'map(select(length > 0))')" \
            --argjson ok "$OK" \
            '{contract:1, feature_id:$fid, orphan_tasks:$orphans, unimplemented_requirements:$unimpl, ok:$ok}'
    else
        echo "Error: jq is required for --json output" >&2
        exit 1
    fi
else
    echo "feature_id: $FEATURE_ID"
    echo "orphan_tasks: ${ORPHAN_TASKS[*]:-}"
    echo "unimplemented_requirements: ${UNIMPLEMENTED_REQUIREMENTS[*]:-}"
    echo "ok: $OK"
fi

exit 0

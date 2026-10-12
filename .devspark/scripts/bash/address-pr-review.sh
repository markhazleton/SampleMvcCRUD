#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Helper script for /devspark.address-pr-review
# --pr-id: parse review file and emit open findings
# --gate:  enforce staged-path isolation for code-only or review-only commits

set -euo pipefail

SCRIPT_DIR="$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

PR_ID=""
GATE=""
JSON_OUTPUT=false

# ---------------------------------------------------------------------------
# Argument parsing
# ---------------------------------------------------------------------------
while [[ $# -gt 0 ]]; do
    case "$1" in
        --pr-id)
            PR_ID="$2"
            shift 2
            ;;
        --gate)
            GATE="$2"
            shift 2
            ;;
        --json)
            JSON_OUTPUT=true
            shift
            ;;
        *)
            printf 'Unknown option: %s\n' "$1" >&2
            exit 1
            ;;
    esac
done

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
get_staged_paths() {
    git diff --cached --name-only 2>/dev/null || true
}

is_review_path() {
    [[ "$1" =~ ^\.devspark.work/pr-review/pr-.*\.md$ ]]
}

write_gate_failure() {
    local message="$1"
    shift
    printf 'BSW.DevSpark: %s\n' "$message" >&2
    if [[ $# -gt 0 ]]; then
        printf 'Offending staged paths:\n' >&2
        for path in "$@"; do
            printf '  - %s\n' "$path" >&2
        done
    fi
    exit 1
}

# ---------------------------------------------------------------------------
# Gate mode
# ---------------------------------------------------------------------------
if [[ -n "$GATE" ]]; then
    # bash 3.2 compatible (macOS system bash lacks mapfile/readarray).
    STAGED=()
    while IFS= read -r staged_line; do
        [[ -n "$staged_line" ]] && STAGED+=("$staged_line")
    done < <(get_staged_paths)

    if [[ "$GATE" == "code-only" ]]; then
        review_staged=()
        for p in "${STAGED[@]:-}"; do
            is_review_path "$p" && review_staged+=("$p") || true
        done
        if [[ ${#review_staged[@]} -gt 0 ]]; then
            write_gate_failure \
                "Code commit gate failed. Review files must not be staged for code-only commits." \
                "${review_staged[@]}"
        fi

    elif [[ "$GATE" == "review-only" ]]; then
        non_review_staged=()
        for p in "${STAGED[@]:-}"; do
            is_review_path "$p" || non_review_staged+=("$p")
        done
        if [[ ${#non_review_staged[@]} -gt 0 ]]; then
            write_gate_failure \
                "Review commit gate failed. Only PR review markdown files may be staged." \
                "${non_review_staged[@]}"
        fi

    else
        printf 'BSW.DevSpark: Invalid --gate value "%s". Expected code-only or review-only.\n' "$GATE" >&2
        exit 1
    fi

    "$JSON_OUTPUT" && printf '{"gate":"%s","passed":true}\n' "$GATE" || printf "Gate '%s' passed.\n" "$GATE"
    exit 0
fi

# ---------------------------------------------------------------------------
# PR findings mode
# ---------------------------------------------------------------------------
if [[ -z "$PR_ID" ]]; then
    printf 'BSW.DevSpark: Provide --pr-id <N> or --gate <code-only|review-only>.\n' >&2
    exit 1
fi

# Strip leading #
NORMALIZED="${PR_ID#\#}"
if ! [[ "$NORMALIZED" =~ ^[0-9]+$ ]]; then
    printf 'BSW.DevSpark: Invalid PR id "%s". Expected a positive integer.\n' "$PR_ID" >&2
    exit 1
fi

REPO_ROOT="$(get_repo_root)"
REVIEW_FILE="$REPO_ROOT/.devspark.work/pr-review/pr-${NORMALIZED}.md"

if [[ ! -f "$REVIEW_FILE" ]]; then
    printf 'BSW.DevSpark: Review file not found: %s\n' "$REVIEW_FILE" >&2
    exit 1
fi

json_escape() {
    local s="$1"
    s="${s//\\/\\\\}"
    s="${s//\"/\\\"}"
    s="${s//$'\t'/\\t}"
    printf '%s' "$s"
}

lower() { printf '%s' "$1" | tr '[:upper:]' '[:lower:]'; }

# Open findings: unchecked lines matching - [ ] **{C|H|M|L|CON}-NN** or **PRD{N}-NN**.
# Auto eligibility comes from status, severity and blocking evidence; an ID prefix alone never grants it.
finding_re='^[[:space:]]*-[[:space:]]\[[[:space:]]\][[:space:]]+\*\*((C|H|M|L|CON|PRD[0-9]+)-[0-9]{2,})\*\*'
contrary_re="(accepted|deferred|waived|withdrawn|wontfix|won't fix)"
findings_json="["
first=true
line_num=0
open_count=0
auto_count=0
section=""
text_lines=""

while IFS= read -r line || [[ -n "$line" ]]; do
    line="${line%$'\r'}"
    line_num=$((line_num + 1))
    if [[ "$line" =~ ^#+[[:space:]]+(.*)$ ]]; then
        section="${BASH_REMATCH[1]}"
        continue
    fi
    if [[ "$line" =~ $finding_re ]]; then
        finding_id="${BASH_REMATCH[1]}"
        severity="${BASH_REMATCH[2]}"
        [[ "$severity" == PRD* ]] && severity="PRD"

        status="open"
        row="$(grep -E "^\|[[:space:]]*${finding_id}[[:space:]]*\|" "$REVIEW_FILE" 2>/dev/null | tail -1 || true)"
        if [[ -n "$row" ]]; then
            status_cell="$(lower "$(printf '%s' "$row" | awk -F'|' '{print $3}')")"
            if [[ "$status_cell" == *resolved* || "$status_cell" == *withdrawn* ]]; then
                status="closed"
            elif [[ "$status_cell" == *partial* ]]; then
                status="partial"
            fi
        fi

        section_lc="$(lower "$section")"
        blocking=false
        [[ "$severity" == "C" ]] && blocking=true
        [[ "$section_lc" == *blocking* && "$section_lc" != *non-blocking* ]] && blocking=true

        route="report"
        eligible=false
        reason=""
        if [[ "$severity" == "CON" ]]; then
            route="evolve-constitution"
            reason="Constitution findings route to /devspark-evolve-constitution, not to product code."
        elif [[ "$status" != "open" ]]; then
            reason="Status is $status; a human decides whether it still needs work."
        elif [[ "$(lower "$line")" =~ $contrary_re ]]; then
            reason="The finding records an accepted, deferred or withdrawn disposition."
        elif [[ "$severity" == "C" || "$severity" == "H" ]]; then
            route="auto"
            eligible=true
            reason="Open $severity finding."
        elif [[ "$severity" == "PRD" && "$blocking" == "true" ]]; then
            route="auto"
            eligible=true
            reason="Open blocking PR-delta finding."
        elif [[ "$severity" == "PRD" ]]; then
            reason="PR-delta finding without blocking evidence."
        else
            reason="Ordinary Medium or Low findings are report-only."
        fi

        open_count=$((open_count + 1))
        [[ "$eligible" == "true" ]] && auto_count=$((auto_count + 1))
        "$first" || findings_json+=","
        findings_json+="{\"id\":\"$finding_id\",\"severity\":\"$severity\",\"line_number\":$line_num,\"line\":\"$(json_escape "$line")\",\"section\":\"$(json_escape "$section")\",\"status\":\"$status\",\"blocking\":$blocking,\"auto_eligible\":$eligible,\"route\":\"$route\",\"reason\":\"$(json_escape "$reason")\"}"
        first=false
        text_lines+="- $finding_id [$route]"$'\n'
    fi
done < "$REVIEW_FILE"

findings_json+="]"

if "$JSON_OUTPUT"; then
    printf '{"pr_id":%s,"review_file":"%s","open_findings":%s,"open_count":%s,"auto_eligible_count":%s}\n' \
        "$NORMALIZED" "$REVIEW_FILE" "$findings_json" "$open_count" "$auto_count"
else
    printf 'Review file: %s\n' "$REVIEW_FILE"
    printf 'Open findings: %s\n' "$open_count"
    printf 'Auto-eligible findings: %s\n' "$auto_count"
    printf '%s' "$text_lines"
fi

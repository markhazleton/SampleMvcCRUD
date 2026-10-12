#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Helper script for /devspark.merge-from-main
# Default: fetch <remote>/<base> and merge it into the current branch.
# --continue: verify conflict resolution, stage resolved paths, and commit the merge.
# --abort: abort an in-progress merge.
# Exit codes: 0 done, 2 needs agent/developer action, 1 error.

set -uo pipefail

BASE="main"
REMOTE="origin"
MODE="run"
PUSH=false
JSON_OUTPUT=false

while [[ $# -gt 0 ]]; do
    case "$1" in
        --base) BASE="$2"; shift 2 ;;
        --remote) REMOTE="$2"; shift 2 ;;
        --continue) MODE="continue"; shift ;;
        --abort) MODE="abort"; shift ;;
        --push) PUSH=true; shift ;;
        --json) JSON_OUTPUT=true; shift ;;
        *) printf 'Unknown option: %s\n' "$1" >&2; exit 1 ;;
    esac
done

BRANCH=""
BEHIND=0
AHEAD=0
FILES=0
PUSHED=false
MESSAGE=""
CONFLICTS=()   # entries: "state<TAB>path<TAB>hunks"
MARKERS=()
DIRTY=()

json_str() {
    local s="$1"
    s="${s//\\/\\\\}"; s="${s//\"/\\\"}"; s="${s//$'\t'/\\t}"; s="${s//$'\r'/}"; s="${s//$'\n'/\\n}"
    printf '"%s"' "$s"
}

json_list() {
    local first=true item
    printf '['
    for item in "$@"; do
        $first || printf ','
        first=false
        json_str "$item"
    done
    printf ']'
}

complete_run() {
    local status="$1" code="$2"
    [[ -n "${3:-}" ]] && MESSAGE="$3"
    local entry state path hunks
    if $JSON_OUTPUT; then
        printf '{"status":%s,"branch":%s,"base":%s,"behind":%s,"ahead":%s,"files":%s,"pushed":%s,"conflicts":[' \
            "$(json_str "$status")" "$(json_str "$BRANCH")" "$(json_str "$REMOTE/$BASE")" "$BEHIND" "$AHEAD" "$FILES" "$PUSHED"
        local first=true
        for entry in "${CONFLICTS[@]+"${CONFLICTS[@]}"}"; do
            IFS=$'\t' read -r state path hunks <<< "$entry"
            $first || printf ','
            first=false
            # shellcheck disable=SC2086
            printf '{"path":%s,"state":%s,"hunks":%s}' "$(json_str "$path")" "$(json_str "$state")" "$(json_list ${hunks//,/ })"
        done
        printf '],"markers":%s,"dirty":%s,"message":%s}\n' \
            "$(json_list "${MARKERS[@]+"${MARKERS[@]}"}")" "$(json_list "${DIRTY[@]+"${DIRTY[@]}"}")" "$(json_str "$MESSAGE")"
    else
        printf 'STATUS: %s\n' "$status"
        printf 'BRANCH: %s  BASE: %s/%s  BEHIND: %s  AHEAD: %s\n' "$BRANCH" "$REMOTE" "$BASE" "$BEHIND" "$AHEAD"
        [[ "$FILES" != "0" ]] && printf 'FILES_CHANGED: %s\n' "$FILES"
        $PUSHED && printf 'PUSHED: true\n'
        if [[ ${#CONFLICTS[@]} -gt 0 ]]; then
            printf 'CONFLICTS:\n'
            for entry in "${CONFLICTS[@]}"; do
                IFS=$'\t' read -r state path hunks <<< "$entry"
                if [[ -n "$hunks" ]]; then printf '  %s %s %s\n' "$state" "$path" "$hunks"; else printf '  %s %s\n' "$state" "$path"; fi
            done
        fi
        if [[ ${#MARKERS[@]} -gt 0 ]]; then
            printf 'MARKERS_REMAIN:\n'
            printf '  %s\n' "${MARKERS[@]}"
        fi
        if [[ ${#DIRTY[@]} -gt 0 ]]; then
            printf 'DIRTY:\n'
            printf '  %s\n' "${DIRTY[@]}"
        fi
        [[ -n "$MESSAGE" ]] && printf 'MESSAGE: %s\n' "$MESSAGE"
    fi
    exit "$code"
}

merge_in_progress() { git rev-parse -q --verify MERGE_HEAD >/dev/null 2>&1; }

unmerged_paths() { git diff --name-only --diff-filter=U 2>/dev/null; }

collect_conflicts() {
    CONFLICTS=()
    local path state hunks
    while IFS= read -r path; do
        [[ -z "$path" ]] && continue
        state="$(git status --porcelain=v1 --untracked-files=no -- "$path" 2>/dev/null | head -n 1 | cut -c1-2)"
        [[ -z "$state" ]] && state="UU"
        hunks=""
        if [[ -f "$path" ]]; then
            hunks="$(awk '
                /^<<<<<<<( |$)/ { start = NR; next }
                start && /^>>>>>>>( |$)/ { printf "%sL%d-%d", sep, start, NR; sep = ","; start = 0 }
            ' "$path")"
        fi
        CONFLICTS+=("$state"$'\t'"$path"$'\t'"$hunks")
    done < <(unmerged_paths)
}

do_push() {
    local out
    if git rev-parse --abbrev-ref --symbolic-full-name '@{u}' >/dev/null 2>&1; then
        out="$(git push --quiet 2>&1)" || complete_run error 1 "push failed: $out"
    else
        out="$(git push --quiet -u "$REMOTE" "$BRANCH" 2>&1)" || complete_run error 1 "push failed: $out"
    fi
    PUSHED=true
}

count_lines() { grep -c . || true; }

git rev-parse --is-inside-work-tree >/dev/null 2>&1 || complete_run error 1 "not inside a git repository"
cd "$(git rev-parse --show-toplevel)" || complete_run error 1 "cannot enter repository root"

BRANCH="$(git symbolic-ref --short -q HEAD || true)"
[[ -z "$BRANCH" ]] && complete_run blocked 2 "detached HEAD; check out a branch first"

if [[ "$MODE" == "abort" ]]; then
    merge_in_progress || complete_run blocked 2 "no merge in progress"
    out="$(git merge --abort 2>&1)" || complete_run error 1 "$out"
    complete_run aborted 0
fi

if [[ "$MODE" == "continue" ]]; then
    merge_in_progress || complete_run blocked 2 "no merge in progress; nothing to continue"
    UNMERGED=()
    while IFS= read -r p; do [[ -n "$p" ]] && UNMERGED+=("$p"); done < <(unmerged_paths)
    CANDIDATES=()
    while IFS= read -r p; do [[ -n "$p" ]] && CANDIDATES+=("$p"); done < <({ unmerged_paths; git diff --name-only HEAD 2>/dev/null; } | sort -u)
    if [[ ${#CANDIDATES[@]} -gt 0 ]]; then
        while IFS= read -r m; do [[ -n "$m" ]] && MARKERS+=("$m"); done \
            < <(git grep -n -I -E '^(<<<<<<<|>>>>>>>)( |$)' -- "${CANDIDATES[@]}" 2>/dev/null)
    fi
    if [[ ${#MARKERS[@]} -gt 0 ]]; then
        collect_conflicts
        complete_run blocked 2 "conflict markers remain"
    fi
    for p in "${UNMERGED[@]+"${UNMERGED[@]}"}"; do
        if [[ -e "$p" ]]; then git add -- "$p"; else git rm --quiet --cached --ignore-unmatch -- "$p"; fi
    done
    if [[ -n "$(unmerged_paths)" ]]; then
        collect_conflicts
        complete_run blocked 2 "unmerged paths remain"
    fi
    out="$(git commit --no-edit --quiet 2>&1)" || complete_run error 1 "commit failed: $out"
    FILES="$(git diff --name-only HEAD^1 HEAD 2>/dev/null | count_lines)"
    $PUSH && do_push
    complete_run resolved 0
fi

if merge_in_progress; then
    collect_conflicts
    complete_run conflicts 2 "merge already in progress; resolve then run with --continue, or --abort"
fi

while IFS= read -r d; do [[ -n "$d" ]] && DIRTY+=("$d"); done < <(git status --porcelain=v1 --untracked-files=no 2>/dev/null)
[[ ${#DIRTY[@]} -gt 0 ]] && complete_run blocked 2 "working tree has uncommitted changes; commit or stash first"

out="$(git fetch --quiet "$REMOTE" "$BASE" 2>&1)" || complete_run error 1 "fetch failed: $out"

TARGET="$REMOTE/$BASE"
BEHIND="$(git rev-list --count "HEAD..$TARGET")"
AHEAD="$(git rev-list --count "$TARGET..HEAD")"
[[ "$BEHIND" == "0" ]] && complete_run up-to-date 0

BEFORE="$(git rev-parse HEAD)"
if [[ "$BRANCH" == "$BASE" ]]; then
    git merge --ff-only --quiet "$TARGET" >/dev/null 2>&1 \
        || complete_run error 1 "local $BASE has diverged from $TARGET; reconcile manually"
    FILES="$(git diff --name-only "$BEFORE" HEAD | count_lines)"
    $PUSH && do_push
    complete_run fast-forwarded 0
fi

if out="$(git merge --no-edit --quiet "$TARGET" 2>&1)"; then
    FILES="$(git diff --name-only "$BEFORE" HEAD | count_lines)"
    $PUSH && do_push
    complete_run merged 0
fi

merge_in_progress || complete_run error 1 "merge failed: $out"
collect_conflicts
complete_run conflicts 2 "resolve the listed hunks, then run with --continue"

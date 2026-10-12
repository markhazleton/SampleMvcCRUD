#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Build commit-history audit context for /devspark.commit-audit
# Usage:
#   commit-audit.sh [--json] [--scope=<full|velocity|hygiene|dora|contributors|ai|architecture>]
#                   [--since=YYYY-MM-DD] [--until=YYYY-MM-DD] [--branch=NAME]
#
# Emits a JSON document on stdout with anonymized contributor data,
# per-commit change stats, and monthly velocity aggregates. Contributor
# identities are replaced with role-based IDs (Contributor A = most
# commits) — real names and emails never appear in the output.

set -euo pipefail

# Multi-app support
source "$(dirname "${BASH_SOURCE[0]}")/common.sh" 2>/dev/null || true
parse_app_context "$@" 2>/dev/null || true
if [[ -n "${DEVSPARK_APP_ID:-}" || "${DEVSPARK_REPO_SCOPE:-false}" == "true" ]]; then
    resolve_app_scope 2>/dev/null || true
    print_scope_summary >&2
fi

SCOPE="full"
SINCE=""
UNTIL=""
BRANCH=""
MAX_COMMITS=1000

while [[ $# -gt 0 ]]; do
  case "$1" in
    --json)
      shift
      ;;
    --scope=*)
      SCOPE="${1#--scope=}"
      shift
      ;;
    --since=*)
      SINCE="${1#--since=}"
      shift
      ;;
    --until=*)
      UNTIL="${1#--until=}"
      shift
      ;;
    --branch=*)
      BRANCH="${1#--branch=}"
      shift
      ;;
    --max-commits=*)
      MAX_COMMITS="${1#--max-commits=}"
      shift
      ;;
    --help|-h)
      cat <<'EOF'
Usage: commit-audit.sh [--json] [--scope=<name>] [--since=YYYY-MM-DD] [--until=YYYY-MM-DD] [--branch=NAME]

Emits commit-audit context JSON on stdout:
- repo_root, branch, total_commits, date_range
- contributors (anonymized role IDs with commit counts and shares)
- commits (sha, date, author_role, message, files_changed, insertions, deletions)
- monthly velocity aggregates and report_path

Options:
  --json                 Accepted for command-contract compatibility (output is always JSON)
  --scope=<name>         full | velocity | hygiene | dora | contributors | ai | architecture
  --since=YYYY-MM-DD     Limit analysis to commits after this date
  --until=YYYY-MM-DD     Limit analysis to commits before this date
  --branch=NAME          Analyze a specific branch (default: current branch)
  --max-commits=<n>      Cap per-commit detail entries (default: 1000)
EOF
      exit 0
      ;;
    *)
      # Tolerate free-text/unknown arguments: the invoking prompt forwards raw
      # user input, so unrecognized tokens are ignored rather than fatal.
      echo "WARNING: ignoring unrecognized argument: $1" >&2
      shift
      ;;
  esac
done

if ! git rev-parse --show-toplevel >/dev/null 2>&1; then
  echo "ERROR: Not inside a git repository." >&2
  exit 1
fi

REPO_ROOT=$(git rev-parse --show-toplevel)
cd "$REPO_ROOT"

if [[ -z "$BRANCH" ]]; then
  BRANCH=$(git rev-parse --abbrev-ref HEAD)
fi
if ! git rev-parse --verify --quiet "$BRANCH" >/dev/null; then
  echo "ERROR: branch not found: $BRANCH" >&2
  exit 1
fi

PYTHON_CMD=""
if command -v python3 >/dev/null 2>&1; then
  PYTHON_CMD="python3"
elif command -v python >/dev/null 2>&1; then
  PYTHON_CMD="python"
else
  echo "ERROR: python3/python is required to generate JSON output." >&2
  exit 1
fi

tmp_dir=$(mktemp -d)
cleanup() {
  rm -rf "$tmp_dir"
}
trap cleanup EXIT

LOG_ARGS=("$BRANCH" --date=short --numstat "--format=@%H%x09%ad%x09%an%x09%s")
[[ -n "$SINCE" ]] && LOG_ARGS+=("--since=$SINCE")
[[ -n "$UNTIL" ]] && LOG_ARGS+=("--until=$UNTIL")
git log "${LOG_ARGS[@]}" > "$tmp_dir/log.txt"

REPORT_PATH=".devspark.work/audit/commit-audit-$(date -u +%Y-%m-%d).md"

"$PYTHON_CMD" - "$REPO_ROOT" "$BRANCH" "$SCOPE" "$SINCE" "$UNTIL" "$MAX_COMMITS" "$REPORT_PATH" "$tmp_dir/log.txt" <<'PY'
import datetime as dt
import json
import pathlib
import sys

repo_root = sys.argv[1]
branch = sys.argv[2]
scope = sys.argv[3]
since = sys.argv[4]
until = sys.argv[5]
max_commits = int(sys.argv[6])
report_path = sys.argv[7]
log_path = pathlib.Path(sys.argv[8])

commits = []
current = None
for raw in log_path.read_text(encoding="utf-8", errors="replace").splitlines():
    if raw.startswith("@"):
        parts = raw[1:].split("\t", 3)
        current = {
            "sha": parts[0],
            "date": parts[1] if len(parts) > 1 else "",
            "author": parts[2] if len(parts) > 2 else "",
            "message": parts[3] if len(parts) > 3 else "",
            "files_changed": 0,
            "insertions": 0,
            "deletions": 0,
        }
        commits.append(current)
    elif raw.strip() and current is not None:
        cols = raw.split("\t")
        if len(cols) >= 3:
            current["files_changed"] += 1
            if cols[0].isdigit():
                current["insertions"] += int(cols[0])
            if cols[1].isdigit():
                current["deletions"] += int(cols[1])

# Anonymize: order authors by commit count desc, then Contributor A, B, ...
counts = {}
for c in commits:
    counts[c["author"]] = counts.get(c["author"], 0) + 1
ordered = sorted(counts.items(), key=lambda kv: (-kv[1], kv[0]))


def role_label(index: int) -> str:
    label = ""
    index += 1
    while index > 0:
        index, rem = divmod(index - 1, 26)
        label = chr(ord("A") + rem) + label
    return f"Contributor {label}"


role_by_author = {author: role_label(i) for i, (author, _) in enumerate(ordered)}
total = len(commits)

contributors = []
for i, (author, n) in enumerate(ordered):
    first = min(c["date"] for c in commits if c["author"] == author)
    last = max(c["date"] for c in commits if c["author"] == author)
    contributors.append({
        "role": role_by_author[author],
        "commits": n,
        "share_pct": round(100.0 * n / total, 1) if total else 0.0,
        "first_commit": first,
        "last_commit": last,
    })

monthly = {}
for c in commits:
    month = c["date"][:7]
    bucket = monthly.setdefault(month, {
        "commits": 0, "files_changed": 0, "insertions": 0, "deletions": 0,
    })
    bucket["commits"] += 1
    bucket["files_changed"] += c["files_changed"]
    bucket["insertions"] += c["insertions"]
    bucket["deletions"] += c["deletions"]

detail = []
for c in commits[:max_commits]:
    detail.append({
        "sha": c["sha"],
        "date": c["date"],
        "author_role": role_by_author[c["author"]],
        "message": c["message"],
        "files_changed": c["files_changed"],
        "insertions": c["insertions"],
        "deletions": c["deletions"],
    })

result = {
    "generated_at": dt.datetime.now(dt.timezone.utc).isoformat(),
    "audit_parameters": {
        "scope": scope,
        "since": since or None,
        "until": until or None,
        "max_commits": max_commits,
    },
    "repo_root": repo_root,
    "branch": branch,
    "total_commits": total,
    "date_range": {
        "first": min((c["date"] for c in commits), default=None),
        "last": max((c["date"] for c in commits), default=None),
    },
    "contributors": contributors,
    "commits": detail,
    "commits_truncated": total > max_commits,
    "monthly_velocity": {k: monthly[k] for k in sorted(monthly)},
    "report_path": report_path,
}

print(json.dumps(result, indent=2))
PY

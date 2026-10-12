#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
set -euo pipefail

SCRIPT_DIR=$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=common.sh
. "$SCRIPT_DIR/common.sh"

repo_root=""
output=""
generated_at=""
stdout=false
fail_on_stale=false
detect_drift=false
full_inventory=false
base=""
head_ref=""
report_output=""
breadth_guidance=""
migrate_source_claims=false
prune_baselines=false
write=false
currency_report=false
currency_days=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        --repo-root) repo_root="$2"; shift 2 ;;
        --output) output="$2"; shift 2 ;;
        --generated-at) generated_at="$2"; shift 2 ;;
        --stdout) stdout=true; shift ;;
        --fail-on-stale) fail_on_stale=true; shift ;;
        --detect-drift) detect_drift=true; shift ;;
        --full-inventory) full_inventory=true; shift ;;
        --base) base="$2"; shift 2 ;;
        --head) head_ref="$2"; shift 2 ;;
        --report-output) report_output="$2"; shift 2 ;;
        --breadth-guidance) breadth_guidance="$2"; shift 2 ;;
        --migrate-source-claims) migrate_source_claims=true; shift ;;
        --prune-baselines) prune_baselines=true; shift ;;
        --write) write=true; shift ;;
        --currency-report) currency_report=true; shift ;;
        --currency-days) currency_days="$2"; shift 2 ;;
        *) echo "Unknown argument: $1" >&2; exit 2 ;;
    esac
done
[[ -n "$repo_root" ]] || repo_root=$(get_repo_root)

arguments=("$SCRIPT_DIR/../build_knowledge_index.py" --repo-root "$repo_root")
[[ -z "$output" ]] || arguments+=(--output "$output")
[[ -z "$generated_at" ]] || arguments+=(--generated-at "$generated_at")
[[ "$stdout" == false ]] || arguments+=(--stdout)
[[ "$fail_on_stale" == false ]] || arguments+=(--fail-on-stale)
[[ "$detect_drift" == false ]] || arguments+=(--detect-drift)
[[ "$full_inventory" == false ]] || arguments+=(--full-inventory)
[[ -z "$base" ]] || arguments+=(--base "$base")
[[ -z "$head_ref" ]] || arguments+=(--head "$head_ref")
[[ -z "$report_output" ]] || arguments+=(--report-output "$report_output")
[[ -z "$breadth_guidance" ]] || arguments+=(--breadth-guidance "$breadth_guidance")
[[ "$migrate_source_claims" == false ]] || arguments+=(--migrate-source-claims)
[[ "$prune_baselines" == false ]] || arguments+=(--prune-baselines)
[[ "$write" == false ]] || arguments+=(--write)
[[ "$currency_report" == false ]] || arguments+=(--currency-report)
[[ -z "$currency_days" ]] || arguments+=(--currency-days "$currency_days")
python3 "${arguments[@]}"

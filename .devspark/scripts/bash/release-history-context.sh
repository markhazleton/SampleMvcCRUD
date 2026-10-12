#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Compatibility adapter: v4 release history is the durable Git delta.
set -euo pipefail

SCRIPT_DIR=$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
args=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    --base-ref) args+=(--from "$2"); shift 2 ;;
    --from) shift 2 ;;
    *) args+=("$1"); shift ;;
  esac
done
exec python3 "$SCRIPT_DIR/../release-context.py" "${args[@]}"

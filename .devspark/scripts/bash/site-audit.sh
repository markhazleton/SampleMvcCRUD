#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Site Audit Pre-Scan Script
# Gathers codebase data as JSON for LLM context efficiency

set -e

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common.sh"

# Multi-app support
parse_app_context "$@" 2>/dev/null || true
if [[ -n "${DEVSPARK_APP_ID:-}" || "${DEVSPARK_REPO_SCOPE:-false}" == "true" ]]; then
    resolve_app_scope 2>/dev/null || true
    print_scope_summary >&2
fi

# Default values
SCOPE="full"
OUTPUT_FORMAT="json"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --scope=*)
            SCOPE="${1#*=}"
            shift
            ;;
        --json)
            OUTPUT_FORMAT="json"
            shift
            ;;
        --summary)
            OUTPUT_FORMAT="summary"
            shift
            ;;
        *)
            shift
            ;;
    esac
done

# Validate scope
case $SCOPE in
    full|constitution|packages|quality|unused|duplicate|comments|evidence|knowledge)
        ;;
    *)
        echo "Invalid scope: $SCOPE" >&2
        echo "Valid options: full, constitution, packages, quality, unused, duplicate, comments, evidence, knowledge" >&2
        exit 1
        ;;
esac

REPO_ROOT=$(get_repo_root)
CONSTITUTION_PATH="$REPO_ROOT/.knowledge/governance/constitution.md"
AUDIT_DIR=".devspark.work/audit"

# Check constitution
CONSTITUTION_EXISTS="false"
CONSTITUTION_VERSION=""
CONSTITUTION_PRINCIPLES="[]"

if [[ -f "$CONSTITUTION_PATH" ]]; then
    CONSTITUTION_EXISTS="true"
    # Extract version if present
    CONSTITUTION_VERSION=$(sed -nE 's/\*\*Version\*\*:[[:space:]]*([^[:space:]|]+).*/\1/p' "$CONSTITUTION_PATH" 2>/dev/null | head -1 || echo "")
    # Extract principles (### headers)
    CONSTITUTION_PRINCIPLES=$(grep -E '^###\s+' "$CONSTITUTION_PATH" 2>/dev/null | sed 's/^###\s*//' | jq -R -s 'split("\n") | map(select(. != ""))' 2>/dev/null || echo "[]")
fi

# Exclusion predicates for find (bash array for reliable word-splitting)
EXCLUDE_DIRS=(-path '*/node_modules/*' -o -path '*/.git/*' -o -path '*/venv/*' -o -path '*/.venv/*' -o -path '*/__pycache__/*' -o -path '*/dist/*' -o -path '*/build/*' -o -path '*/.next/*' -o -path '*/coverage/*' -o -path '*/.devspark.work/*' -o -path '*/.archive/*')

# Count files by category
count_files() {
    local pattern="$1"
    find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o -name "$pattern" -print 2>/dev/null | wc -l | tr -d ' '
}

# Source files
SOURCE_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.py" -o -name "*.ts" -o -name "*.tsx" -o -name "*.js" -o -name "*.jsx" -o -name "*.cs" -o -name "*.java" -o -name "*.go" -o -name "*.rs" \) -print 2>/dev/null | grep -v -E '(test_|_test\.|\.test\.|_spec\.)' | wc -l | tr -d ' ')

# Test files
TEST_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -path '*/test/*' -o -path '*/tests/*' -o -name 'test_*' -o -name '*_test.*' -o -name '*.test.*' -o -name '*_spec.*' \) -print 2>/dev/null | wc -l | tr -d ' ')

# Config files
CONFIG_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.json" -o -name "*.yaml" -o -name "*.yml" -o -name "*.toml" -o -name "*.ini" -o -name ".env*" \) -print 2>/dev/null | wc -l | tr -d ' ')

# Documentation
DOC_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.md" -o -name "*.rst" -o -name "*.txt" \) -print 2>/dev/null | wc -l | tr -d ' ')

# Scripts
SCRIPT_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.sh" -o -name "*.ps1" -o -name "*.bash" \) -print 2>/dev/null | wc -l | tr -d ' ')

# Build files
BUILD_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "Dockerfile*" -o -name "Makefile" -o -path '*/.github/workflows/*' \) -print 2>/dev/null | wc -l | tr -d ' ')

# Detect package manager
PACKAGE_MANAGER="null"
PACKAGE_MANIFEST="null"
PACKAGE_LOCKFILE="null"

if [[ -f "$REPO_ROOT/pyproject.toml" ]]; then
    PACKAGE_MANAGER="pip"
    PACKAGE_MANIFEST="pyproject.toml"
elif [[ -f "$REPO_ROOT/requirements.txt" ]]; then
    PACKAGE_MANAGER="pip"
    PACKAGE_MANIFEST="requirements.txt"
elif [[ -f "$REPO_ROOT/package.json" ]]; then
    PACKAGE_MANAGER="npm"
    PACKAGE_MANIFEST="package.json"
    if [[ -f "$REPO_ROOT/package-lock.json" ]]; then
        PACKAGE_LOCKFILE="package-lock.json"
    elif [[ -f "$REPO_ROOT/yarn.lock" ]]; then
        PACKAGE_LOCKFILE="yarn.lock"
    fi
elif [[ -f "$REPO_ROOT/go.mod" ]]; then
    PACKAGE_MANAGER="go"
    PACKAGE_MANIFEST="go.mod"
    PACKAGE_LOCKFILE="go.sum"
elif [[ -f "$REPO_ROOT/Cargo.toml" ]]; then
    PACKAGE_MANAGER="cargo"
    PACKAGE_MANIFEST="Cargo.toml"
    PACKAGE_LOCKFILE="Cargo.lock"
fi

# Calculate total lines of code (limit to first 100 files for performance)
TOTAL_LINES=0
if [[ "$SCOPE" == "full" || "$SCOPE" == "quality" ]]; then
    TOTAL_LINES=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.py" -o -name "*.ts" -o -name "*.js" \) -print 2>/dev/null | head -100 | xargs wc -l 2>/dev/null | tail -1 | awk '{print $1}' || echo 0)
fi

# Pattern detection (limited scan)
SECRET_COUNT=0
TODO_COUNT=0

if [[ "$SCOPE" == "full" || "$SCOPE" == "constitution" || "$SCOPE" == "quality" || "$SCOPE" == "comments" ]]; then
    # Check for potential secrets (limited to 50 files)
    SECRET_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.py" -o -name "*.ts" -o -name "*.js" -o -name "*.json" \) -print 2>/dev/null | head -50 | xargs grep -l -E '(api[_-]?key|password|secret|token)\s*[=:]\s*['"'"'"][^'"'"'"]{8,}['"'"'"]' 2>/dev/null | wc -l | tr -d ' ' || echo 0)

    # Count TODO/FIXME comments
    TODO_COUNT=$(find "$REPO_ROOT" -type f \( "${EXCLUDE_DIRS[@]}" \) -prune -o \( -name "*.py" -o -name "*.ts" -o -name "*.js" -o -name "*.md" \) -print 2>/dev/null | head -100 | xargs grep -c -E '(TODO|FIXME|HACK|XXX):' 2>/dev/null | awk -F: '{sum+=$1} END {print sum}' || echo 0)
fi

# Knowledge pre-scan (index currency, entity coverage)
KNOWLEDGE_JSON="null"
KNOWLEDGE_DOCS=0
KNOWLEDGE_ENTITIES=0
KNOWLEDGE_INDEX_CURRENT=null
KNOWLEDGE_DANGLING=null
KNOWLEDGE_COVERAGE_GAPS="[]"
KNOWLEDGE_GAP_COUNT=0
if [[ "$SCOPE" == "full" || "$SCOPE" == "knowledge" || "$SCOPE" == "evidence" ]]; then
    KNOWLEDGE_ROOT="$REPO_ROOT/.knowledge"
    if [[ -d "$KNOWLEDGE_ROOT" ]]; then
        KNOWLEDGE_DOCS=$(find "$KNOWLEDGE_ROOT" -type f -name '*.md' 2>/dev/null | wc -l | tr -d ' ')
        if [[ -d "$KNOWLEDGE_ROOT/entities" ]]; then
            KNOWLEDGE_ENTITIES=$(find "$KNOWLEDGE_ROOT/entities" -mindepth 1 -maxdepth 1 -type d 2>/dev/null | wc -l | tr -d ' ')
        fi
        KNOWLEDGE_INDEX_PRESENT=false
        [[ -f "$KNOWLEDGE_ROOT/index.json" ]] && KNOWLEDGE_INDEX_PRESENT=true
        KNOWLEDGE_COVERAGE_PRESENT=false
        [[ -f "$KNOWLEDGE_ROOT/ontology/coverage.json" ]] && KNOWLEDGE_COVERAGE_PRESENT=true

        KNOWLEDGE_ENGINE=$(resolve_knowledge_engine "$REPO_ROOT")
        PYTHON_BIN=$(command -v python3 || command -v python || true)
        if [[ -f "$KNOWLEDGE_ENGINE" && -n "$PYTHON_BIN" ]]; then
            if "$PYTHON_BIN" "$KNOWLEDGE_ENGINE" --repo-root "$REPO_ROOT" --check >/dev/null 2>&1; then
                KNOWLEDGE_INDEX_CURRENT=true
            else
                KNOWLEDGE_INDEX_CURRENT=false
            fi
        fi

        if [[ -n "$PYTHON_BIN" && "$KNOWLEDGE_INDEX_PRESENT" == "true" ]]; then
            KNOWLEDGE_DANGLING=$("$PYTHON_BIN" -c 'import json,sys; print(len(json.load(open(sys.argv[1], encoding="utf-8")).get("dangling_references", [])))' "$KNOWLEDGE_ROOT/index.json" 2>/dev/null || echo null)
        fi

        if [[ -n "$PYTHON_BIN" && "$KNOWLEDGE_COVERAGE_PRESENT" == "true" ]]; then
            KNOWLEDGE_COVERAGE_GAPS=$("$PYTHON_BIN" -c 'import json,sys; print(json.dumps(json.load(open(sys.argv[1], encoding="utf-8")).get("gaps", [])))' "$KNOWLEDGE_ROOT/ontology/coverage.json" 2>/dev/null || echo "[]")
            KNOWLEDGE_GAP_COUNT=$("$PYTHON_BIN" -c 'import json,sys; print(len(json.load(open(sys.argv[1], encoding="utf-8")).get("gaps", [])))' "$KNOWLEDGE_ROOT/ontology/coverage.json" 2>/dev/null || echo 0)
        fi

        KNOWLEDGE_JSON="{ \"present\": true, \"index_present\": $KNOWLEDGE_INDEX_PRESENT, \"index_current\": $KNOWLEDGE_INDEX_CURRENT, \"coverage_present\": $KNOWLEDGE_COVERAGE_PRESENT, \"coverage_gaps\": $KNOWLEDGE_COVERAGE_GAPS, \"dangling_references\": $KNOWLEDGE_DANGLING, \"counts\": { \"documents\": $KNOWLEDGE_DOCS, \"entities\": $KNOWLEDGE_ENTITIES } }"
    else
        KNOWLEDGE_JSON="{ \"present\": false, \"index_present\": false, \"index_current\": null, \"coverage_present\": false, \"coverage_gaps\": [], \"dangling_references\": null, \"counts\": { \"documents\": 0, \"entities\": 0 } }"
    fi
fi

# Output
if [[ "$OUTPUT_FORMAT" == "json" ]]; then
    cat <<EOF
{
  "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "scope": "$SCOPE",
  "repo_root": "$REPO_ROOT",
  "audit_dir": "$AUDIT_DIR",
  "constitution": {
    "exists": $CONSTITUTION_EXISTS,
    "path": ".knowledge/governance/constitution.md",
    "version": $(if [[ -n "$CONSTITUTION_VERSION" ]]; then echo "\"$CONSTITUTION_VERSION\""; else echo "null"; fi),
    "principles": $CONSTITUTION_PRINCIPLES
  },
  "files": {
    "counts": {
      "source": $SOURCE_COUNT,
      "config": $CONFIG_COUNT,
      "documentation": $DOC_COUNT,
      "tests": $TEST_COUNT,
      "scripts": $SCRIPT_COUNT,
      "build": $BUILD_COUNT
    }
  },
  "packages": {
    "manager": $(if [[ "$PACKAGE_MANAGER" != "null" ]]; then echo "\"$PACKAGE_MANAGER\""; else echo "null"; fi),
    "manifest": $(if [[ "$PACKAGE_MANIFEST" != "null" ]]; then echo "\"$PACKAGE_MANIFEST\""; else echo "null"; fi),
    "lockfile": $(if [[ "$PACKAGE_LOCKFILE" != "null" ]]; then echo "\"$PACKAGE_LOCKFILE\""; else echo "null"; fi)
  },
  "metrics": {
    "total_lines": $TOTAL_LINES,
    "total_files": $SOURCE_COUNT
  },
  "patterns": {
    "security": {
      "potential_secrets_files": $SECRET_COUNT
    },
    "quality": {
      "todo_count": $TODO_COUNT
    }
  },
  "knowledge": $KNOWLEDGE_JSON
}
EOF
else
    echo "Site Audit Pre-Scan Summary"
    echo "=========================="
    echo "Repository: $REPO_ROOT"
    echo "Scope: $SCOPE"
    echo "Constitution: $(if [[ "$CONSTITUTION_EXISTS" == "true" ]]; then echo "Found"; else echo "MISSING"; fi)"
    echo ""
    echo "File Counts:"
    echo "  Source files: $SOURCE_COUNT"
    echo "  Config files: $CONFIG_COUNT"
    echo "  Documentation: $DOC_COUNT"
    echo "  Test files: $TEST_COUNT"
    echo "  Scripts: $SCRIPT_COUNT"
    echo "  Build files: $BUILD_COUNT"
    echo ""
    echo "Package Manager: $PACKAGE_MANAGER"
    echo ""
    if [[ "$SCOPE" == "full" || "$SCOPE" == "quality" ]]; then
        echo "Code Metrics:"
        echo "  Total lines (sample): $TOTAL_LINES"
        echo ""
    fi
    if [[ "$SCOPE" == "full" || "$SCOPE" == "constitution" || "$SCOPE" == "quality" || "$SCOPE" == "comments" ]]; then
        echo "Pattern Detection:"
        echo "  Files with potential secrets: $SECRET_COUNT"
        echo "  TODO/FIXME comments: $TODO_COUNT"
    fi
    if [[ "$SCOPE" == "full" || "$SCOPE" == "knowledge" || "$SCOPE" == "evidence" ]]; then
        echo ""
        echo "Knowledge:"
        echo "  Documents: $KNOWLEDGE_DOCS"
        echo "  Entities: $KNOWLEDGE_ENTITIES"
        echo "  Index current: $KNOWLEDGE_INDEX_CURRENT"
        echo "  Coverage gaps: $KNOWLEDGE_GAP_COUNT"
        echo "  Dangling references: $KNOWLEDGE_DANGLING"
    fi
fi

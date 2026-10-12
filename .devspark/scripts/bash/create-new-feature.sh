#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com

set -e
script_start_epoch=$(date +%s)

JSON_MODE=false
SHORT_NAME=""
BRANCH_NUMBER=""
ARGS=()
i=1
while [ $i -le $# ]; do
    arg="${!i}"
    case "$arg" in
        --json)
            JSON_MODE=true
            ;;
        --short-name)
            if [ $((i + 1)) -gt $# ]; then
                echo 'Error: --short-name requires a value' >&2
                exit 1
            fi
            i=$((i + 1))
            next_arg="${!i}"
            # Check if the next argument is another option (starts with --)
            if [[ "$next_arg" == --* ]]; then
                echo 'Error: --short-name requires a value' >&2
                exit 1
            fi
            SHORT_NAME="$next_arg"
            ;;
        --number)
            if [ $((i + 1)) -gt $# ]; then
                echo 'Error: --number requires a value' >&2
                exit 1
            fi
            i=$((i + 1))
            next_arg="${!i}"
            if [[ "$next_arg" == --* ]]; then
                echo 'Error: --number requires a value' >&2
                exit 1
            fi
            BRANCH_NUMBER="$next_arg"
            ;;
        --help|-h)
            echo "Usage: $0 [--json] [--short-name <name>] [--number N] <feature_description>"
            echo ""
            echo "Options:"
            echo "  --json              Output in JSON format"
            echo "  --short-name <name> Provide a custom short name (2-4 words) for the branch"
            echo "  --number N          Specify branch number manually (overrides auto-detection)"
            echo "  --help, -h          Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0 'Add user authentication system' --short-name 'user-auth'"
            echo "  $0 'Implement OAuth2 integration for API' --number 5"
            exit 0
            ;;
        *)
            ARGS+=("$arg")
            ;;
    esac
    i=$((i + 1))
done

FEATURE_DESCRIPTION="${ARGS[*]}"
if [ -z "$FEATURE_DESCRIPTION" ]; then
    echo "Usage: $0 [--json] [--short-name <name>] [--number N] <feature_description>" >&2
    exit 1
fi

# Function to find the repository root by searching for existing project markers
find_repo_root() {
    local dir="$1"
    while [ "$dir" != "/" ]; do
        if [ -d "$dir/.git" ] || [ -d "$dir/.devspark" ]; then
            echo "$dir"
            return 0
        fi
        dir="$(dirname "$dir")"
    done
    return 1
}

# Function to get highest number from specs directory
get_highest_from_specs() {
    local specs_dir="$1"
    local highest=0

    if [ -d "$specs_dir" ]; then
        for dir in "$specs_dir"/*; do
            [ -d "$dir" ] || continue
            dirname=$(basename "$dir")
            number=$(echo "$dirname" | grep -o '^[0-9]\+' || echo "0")
            number=$((10#$number))
            if [ "$number" -gt "$highest" ]; then
                highest=$number
            fi
        done
    fi

    echo "$highest"
}

# Function to get highest number from git branches
get_highest_from_branches() {
    local highest=0

    # Get all branches (local and remote)
    branches=$(git branch -a 2>/dev/null || echo "")

    if [ -n "$branches" ]; then
        while IFS= read -r branch; do
            # Clean branch name: remove leading markers and remote prefixes
            clean_branch=$(echo "$branch" | sed 's/^[* ]*//; s|^remotes/[^/]*/||')

            # Extract feature number if branch matches pattern ###-*
            if echo "$clean_branch" | grep -q '^[0-9]\{3\}-'; then
                number=$(echo "$clean_branch" | grep -o '^[0-9]\{3\}' || echo "0")
                number=$((10#$number))
                if [ "$number" -gt "$highest" ]; then
                    highest=$number
                fi
            fi
        done <<< "$branches"
    fi

    echo "$highest"
}

# Function to check existing branches (local and remote) and return next available number
check_existing_branches() {
    local specs_dir="$1"

    # Fetch all remotes to get latest branch info (suppress errors if no remotes)
    git fetch --all --prune 2>/dev/null || true

    # Get highest number from ALL branches (not just matching short name)
    local highest_branch
    highest_branch=$(get_highest_from_branches)

    # Get highest number from ALL specs (not just matching short name)
    local highest_spec
    highest_spec=$(get_highest_from_specs "$specs_dir")

    # Take the maximum of both
    local max_num=$highest_branch
    if [ "$highest_spec" -gt "$max_num" ]; then
        max_num=$highest_spec
    fi

    # Return next number
    echo $((max_num + 1))
}

# Function to clean and format a branch name
clean_branch_name() {
    local name="$1"
    echo "$name" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]/-/g' | sed 's/-\+/-/g' | sed 's/^-//' | sed 's/-$//'
}

# Resolve repository root. Prefer git information when available, but fall back
# to searching for repository markers so the workflow still functions in repositories that
# were initialised with --no-git.
SCRIPT_DIR="$(CDPATH="" cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if git rev-parse --show-toplevel >/dev/null 2>&1; then
    REPO_ROOT=$(git rev-parse --show-toplevel)
    HAS_GIT=true
else
    REPO_ROOT="$(find_repo_root "$SCRIPT_DIR")"
    if [ -z "$REPO_ROOT" ]; then
        echo "Error: Could not determine repository root. Please run this script from within the repository." >&2
        exit 1
    fi
    HAS_GIT=false
fi

cd "$REPO_ROOT"

# Load common for multi-app helpers
source "$SCRIPT_DIR/common.sh" 2>/dev/null || source "$(dirname "${BASH_SOURCE[0]}")/common.sh" 2>/dev/null || true

# Multi-app support: parse --app and --repo-scope from ARGS
parse_app_context "${ARGS[@]}" 2>/dev/null || true
if [[ ${#DEVSPARK_REMAINING_ARGS[@]} -gt 0 ]]; then
    ARGS=("${DEVSPARK_REMAINING_ARGS[@]}")
fi
FEATURE_DESCRIPTION="${ARGS[*]}"

# Determine specs directory based on app context
if [[ -n "${DEVSPARK_APP_ID:-}" ]]; then
    APP_DOC_ROOT=$(resolve_app_doc_root "$REPO_ROOT" "$DEVSPARK_APP_ID" 2>/dev/null || true)
    if [[ -n "$APP_DOC_ROOT" && "$APP_DOC_ROOT" != ERROR* ]]; then
        SPECS_DIR="$APP_DOC_ROOT/specs"
    else
        SPECS_DIR="$REPO_ROOT/.devspark.work/specs"
    fi
else
    SPECS_DIR="$REPO_ROOT/.devspark.work/specs"
fi
mkdir -p "$SPECS_DIR"

# Delegate to the single branch-creation
# choke point (new-branch.sh) for the common (non-multi-app-scoped) case.
# new-branch.sh does not yet support an app-scoped specs directory, so a repo
# using multi-app mode with an --app context keeps the legacy inline path below.
if [[ -z "${DEVSPARK_APP_ID:-}" ]]; then
    NEW_BRANCH_ARGS=(--type spec --json --yes)
    [[ -n "$SHORT_NAME" ]] && NEW_BRANCH_ARGS+=(--short-name "$SHORT_NAME")
    [[ -n "$BRANCH_NUMBER" ]] && NEW_BRANCH_ARGS+=(--number "$BRANCH_NUMBER")
    NEW_BRANCH_ARGS+=("$FEATURE_DESCRIPTION")

    NEW_BRANCH_OUTPUT="$("$SCRIPT_DIR/new-branch.sh" "${NEW_BRANCH_ARGS[@]}")"
    NEW_BRANCH_EXIT=$?
    if [[ $NEW_BRANCH_EXIT -ne 0 ]]; then
        echo "$NEW_BRANCH_OUTPUT" >&2
        exit $NEW_BRANCH_EXIT
    fi

    BRANCH_NAME=$(echo "$NEW_BRANCH_OUTPUT" | grep -o '"BRANCH_NAME":"[^"]*"' | sed 's/.*:"//; s/"$//')
    FEATURE_NUM=$(echo "$NEW_BRANCH_OUTPUT" | grep -o '"NUMBER":"[^"]*"' | sed 's/.*:"//; s/"$//')
    SPEC_FILE=$(echo "$NEW_BRANCH_OUTPUT" | grep -o '"ARTIFACT_PATH":"[^"]*"' | sed 's/.*:"//; s/"$//')

    # Knowledge document:
    # only the feature-overview doc is emitted here -- spec.md is still the empty
    # template stub at this point (the agent drafts real content afterward), so
    # Requirement-document emission is deferred to setup-plan (see plan.md Implementation Notes).
    # Failure here must never affect this script's JSON contract or exit code.
    if [[ -n "$SPEC_FILE" ]]; then
        write_knowledge_document "$(dirname "$SPEC_FILE")" feature feature "$BRANCH_NAME" draft \
            '{"requirements":[]}' create-new-feature || true
    fi

    # Preserve the original JSON contract (BRANCH_NAME, SPEC_FILE, FEATURE_NUM).
    if $JSON_MODE; then
        printf '{"contract":1,"BRANCH_NAME":"%s","SPEC_FILE":"%s","FEATURE_NUM":"%s"}\n' "$BRANCH_NAME" "$SPEC_FILE" "$FEATURE_NUM"
    else
        echo "BRANCH_NAME: $BRANCH_NAME"
        echo "SPEC_FILE: $SPEC_FILE"
        echo "FEATURE_NUM: $FEATURE_NUM"
        echo "DEVSPARK_FEATURE environment variable set to: $BRANCH_NAME"
    fi

    script_end_epoch=$(date +%s)
    duration_ms=$(( (script_end_epoch - script_start_epoch) * 1000 ))
    add_devspark_metric "create-new-feature" "success" "$duration_ms" '{"showstopper":0,"critical":0,"high":0,"medium":0,"low":0}'
    exit 0
fi

# --- Legacy inline path (multi-app --app context only; new-branch.sh does not
# support an app-scoped specs directory yet) ---

# Function to generate branch name with stop word filtering and length filtering
generate_branch_name() {
    local description="$1"

    # Common stop words to filter out
    local stop_words="^(i|a|an|the|to|for|of|in|on|at|by|with|from|is|are|was|were|be|been|being|have|has|had|do|does|did|will|would|should|could|can|may|might|must|shall|this|that|these|those|my|your|our|their|want|need|add|get|set)$"

    # Convert to lowercase and split into words
    local clean_name
    clean_name=$(echo "$description" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]/ /g')

    # Filter words: remove stop words and words shorter than 3 chars (unless they're uppercase acronyms in original)
    local meaningful_words=()
    for word in $clean_name; do
        # Skip empty words
        [ -z "$word" ] && continue

        # Keep words that are NOT stop words AND (length >= 3 OR are potential acronyms)
        if ! echo "$word" | grep -qiE "$stop_words"; then
            if [ ${#word} -ge 3 ]; then
                meaningful_words+=("$word")
            elif echo "$description" | grep -q "\b$(echo "$word" | tr '[:lower:]' '[:upper:]')\b"; then
                # Keep short words if they appear as uppercase in original (likely acronyms)
                meaningful_words+=("$word")
            fi
        fi
    done

    # If we have meaningful words, use first 3-4 of them
    if [ ${#meaningful_words[@]} -gt 0 ]; then
        local max_words=3
        if [ ${#meaningful_words[@]} -eq 4 ]; then max_words=4; fi

        local result=""
        local count=0
        for word in "${meaningful_words[@]}"; do
            if [ $count -ge $max_words ]; then break; fi
            if [ -n "$result" ]; then result="$result-"; fi
            result="$result$word"
            count=$((count + 1))
        done
        echo "$result"
    else
        # Fallback to original logic if no meaningful words found
        local cleaned
        cleaned=$(clean_branch_name "$description")
        echo "$cleaned" | tr '-' '\n' | grep -v '^$' | head -3 | tr '\n' '-' | sed 's/-$//'
    fi
}

# Generate branch name
if [ -n "$SHORT_NAME" ]; then
    # Use provided short name, just clean it up
    BRANCH_SUFFIX=$(clean_branch_name "$SHORT_NAME")
else
    # Generate from description with smart filtering
    BRANCH_SUFFIX=$(generate_branch_name "$FEATURE_DESCRIPTION")
fi

# Determine branch number
if [ -z "$BRANCH_NUMBER" ]; then
    if [ "$HAS_GIT" = true ]; then
        # Check existing branches on remotes
        BRANCH_NUMBER=$(check_existing_branches "$SPECS_DIR")
    else
        # Fall back to local directory check
        HIGHEST=$(get_highest_from_specs "$SPECS_DIR")
        BRANCH_NUMBER=$((HIGHEST + 1))
    fi
fi

# Force base-10 interpretation to prevent octal conversion (e.g., 010 → 8 in octal, but should be 10 in decimal)
FEATURE_NUM=$(printf "%03d" "$((10#$BRANCH_NUMBER))")
BRANCH_NAME="${FEATURE_NUM}-${BRANCH_SUFFIX}"

# Collision guard: numbering is a single repo-wide sequence, never scoped to a
# short-name. Refuse to proceed if this exact number is already in use by any
# branch (local/remote) or spec directory, regardless of short-name -- this
# prevents the class of bug where two different features silently share a number.
NUMERIC_PREFIX="${FEATURE_NUM}-"
SPEC_COLLISION=false
if [ -d "$SPECS_DIR" ]; then
    for dir in "$SPECS_DIR"/${NUMERIC_PREFIX}*; do
        [ -d "$dir" ] || continue
        SPEC_COLLISION=true
        break
    done
fi
BRANCH_COLLISION=false
if [ "$HAS_GIT" = true ]; then
    EXISTING_BRANCHES=$(git branch -a 2>/dev/null || echo "")
    if [ -n "$EXISTING_BRANCHES" ]; then
        while IFS= read -r branch; do
            clean_branch=$(echo "$branch" | sed 's/^[* ]*//; s|^remotes/[^/]*/||')
            case "$clean_branch" in
                "${NUMERIC_PREFIX}"*)
                    BRANCH_COLLISION=true
                    break
                    ;;
            esac
        done <<< "$EXISTING_BRANCHES"
    fi
fi
if [ "$SPEC_COLLISION" = true ] || [ "$BRANCH_COLLISION" = true ]; then
    echo "Error: feature number $FEATURE_NUM is already in use by an existing branch or spec directory (collision guard). Re-run without --number so it can be recomputed, or choose a different number explicitly." >&2
    exit 1
fi

# GitHub enforces a 244-byte limit on branch names
# Validate and truncate if necessary
MAX_BRANCH_LENGTH=244
if [ ${#BRANCH_NAME} -gt $MAX_BRANCH_LENGTH ]; then
    # Calculate how much we need to trim from suffix
    # Account for: feature number (3) + hyphen (1) = 4 chars
    MAX_SUFFIX_LENGTH=$((MAX_BRANCH_LENGTH - 4))

    # Truncate suffix at word boundary if possible
    TRUNCATED_SUFFIX=$(echo "$BRANCH_SUFFIX" | cut -c1-$MAX_SUFFIX_LENGTH)
    # Remove trailing hyphen if truncation created one
    TRUNCATED_SUFFIX=$(echo "$TRUNCATED_SUFFIX" | sed 's/-$//')

    ORIGINAL_BRANCH_NAME="$BRANCH_NAME"
    BRANCH_NAME="${FEATURE_NUM}-${TRUNCATED_SUFFIX}"

    >&2 echo "[devspark] Warning: Branch name exceeded GitHub's 244-byte limit"
    >&2 echo "[devspark] Original: $ORIGINAL_BRANCH_NAME (${#ORIGINAL_BRANCH_NAME} bytes)"
    >&2 echo "[devspark] Truncated to: $BRANCH_NAME (${#BRANCH_NAME} bytes)"
fi

if [ "$HAS_GIT" = true ]; then
    git checkout -b "$BRANCH_NAME" 2>/dev/null || {
        echo "[devspark] Error: Failed to create git branch '$BRANCH_NAME' (it may already exist). Aborting before writing spec." >&2
        exit 1
    }
else
    >&2 echo "[devspark] Warning: Git repository not detected; skipped branch creation for $BRANCH_NAME"
fi

FEATURE_DIR="$SPECS_DIR/$BRANCH_NAME"
mkdir -p "$FEATURE_DIR"

TEMPLATE="$(resolve_template_path "$REPO_ROOT" "spec-template.md" || true)"
SPEC_FILE="$FEATURE_DIR/spec.md"
if [ -n "$TEMPLATE" ] && [ -f "$TEMPLATE" ]; then
    cp "$TEMPLATE" "$SPEC_FILE"
else
    echo "[devspark] Warning: No spec-template.md found in fallback chain; creating empty spec.md" >&2
    touch "$SPEC_FILE"
fi

# Set the DEVSPARK_FEATURE environment variable for the current session
export DEVSPARK_FEATURE="$BRANCH_NAME"

if $JSON_MODE; then
    printf '{"contract":1,"BRANCH_NAME":"%s","SPEC_FILE":"%s","FEATURE_NUM":"%s"}\n' "$BRANCH_NAME" "$SPEC_FILE" "$FEATURE_NUM"
else
    echo "BRANCH_NAME: $BRANCH_NAME"
    echo "SPEC_FILE: $SPEC_FILE"
    echo "FEATURE_NUM: $FEATURE_NUM"
    echo "DEVSPARK_FEATURE environment variable set to: $BRANCH_NAME"
fi

script_end_epoch=$(date +%s)
duration_ms=$(( (script_end_epoch - script_start_epoch) * 1000 ))
add_devspark_metric "create-new-feature" "success" "$duration_ms" '{"showstopper":0,"critical":0,"high":0,"medium":0,"low":0}'

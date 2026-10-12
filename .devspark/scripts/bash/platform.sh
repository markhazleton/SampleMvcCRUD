#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
# Platform detection and adapter for BSW.DevSpark scripts
# Detects GitHub, Azure DevOps, or GitLab and exports platform-specific values.
#
# Usage: source "$(dirname "${BASH_SOURCE[0]}")/platform.sh"
#        Then use $DEVSPARK_PLATFORM_NAME, $DEVSPARK_PR_CLI, etc.
#
# Override: Set DEVSPARK_PLATFORM env var to force a platform (github|azdo|gitlab)
# Config:   Or set "platform" in .devspark.work/devspark.json

# Load common if not already loaded
if ! type get_repo_root &>/dev/null; then
    source "$(dirname "${BASH_SOURCE[0]}")/common.sh"
fi

detect_platform() {
    # 1. Explicit env var override
    if [[ -n "${DEVSPARK_PLATFORM:-}" ]]; then
        echo "$DEVSPARK_PLATFORM" | tr '[:upper:]' '[:lower:]'
        return
    fi

    # 2. Config file override
    local repo_root
    repo_root=$(get_repo_root)
    local config_file="$repo_root/.devspark.work/devspark.json"
    if [[ -f "$config_file" ]]; then
        local platform_val
        platform_val=$(jq -r '.platform // empty' "$config_file" 2>/dev/null || true)
        if [[ -n "$platform_val" ]]; then
            echo "$platform_val" | tr '[:upper:]' '[:lower:]'
            return
        fi
    fi

    # 3. CI environment variable detection
    if [[ -n "${GITHUB_ACTIONS:-}" || -n "${GITHUB_REPOSITORY:-}" ]]; then
        echo "github"; return
    fi
    if [[ -n "${SYSTEM_TEAMFOUNDATIONCOLLECTIONURI:-}" || -n "${BUILD_REPOSITORY_PROVIDER:-}" ]]; then
        echo "azdo"; return
    fi
    if [[ -n "${GITLAB_CI:-}" || -n "${CI_PROJECT_ID:-}" ]]; then
        echo "gitlab"; return
    fi

    # 4. Remote URL detection (authoritative host signal; beats local folder layout).
    #    A .github/ folder is common even on Azure DevOps repos (Copilot shims), so
    #    the remote must be checked before repository-structure heuristics.
    local remote_url
    remote_url=$(git remote get-url origin 2>/dev/null || true)
    if [[ -n "$remote_url" ]]; then
        if [[ "$remote_url" == *github.com* ]]; then echo "github"; return; fi
        if [[ "$remote_url" == *dev.azure.com* || "$remote_url" == *visualstudio.com* ]]; then echo "azdo"; return; fi
        if [[ "$remote_url" == *gitlab.com* || "$remote_url" == *gitlab.* ]]; then echo "gitlab"; return; fi
    fi

    # 5. Repository structure detection (fallback when no remote is configured).
    if [[ -d "$repo_root/.github" ]]; then
        echo "github"; return
    fi
    if [[ -f "$repo_root/azure-pipelines.yml" ]]; then
        echo "azdo"; return
    fi
    if [[ -f "$repo_root/.gitlab-ci.yml" ]]; then
        echo "gitlab"; return
    fi

    echo "github"  # Default fallback
}

# Set platform-specific variables based on detected platform
_set_platform_config() {
    local platform="$1"

    case "$platform" in
        github)
            DEVSPARK_PLATFORM_NAME="github"
            DEVSPARK_PLATFORM_DISPLAY="GitHub"
            DEVSPARK_PR_CLI="gh"
            DEVSPARK_PR_CLI_INSTALL_URL="https://cli.github.com/"
            DEVSPARK_CI_DIR=".github/workflows"
            DEVSPARK_CI_FILE_PATTERN="*.yml"
            DEVSPARK_AGENT_CONFIG_PATH="AGENTS.md"
            DEVSPARK_BRANCH_NAME_LIMIT=244
            DEVSPARK_PR_ENV_VAR="GITHUB_PR_NUMBER"
            ;;
        azdo)
            DEVSPARK_PLATFORM_NAME="azdo"
            DEVSPARK_PLATFORM_DISPLAY="Azure DevOps"
            DEVSPARK_PR_CLI="az"
            DEVSPARK_PR_CLI_INSTALL_URL="https://learn.microsoft.com/en-us/cli/azure/install-azure-cli"
            DEVSPARK_CI_DIR="."
            DEVSPARK_CI_FILE_PATTERN="azure-pipelines*.yml"
            DEVSPARK_AGENT_CONFIG_PATH="AGENTS.md"
            DEVSPARK_BRANCH_NAME_LIMIT=250
            DEVSPARK_PR_ENV_VAR="SYSTEM_PULLREQUEST_PULLREQUESTID"
            ;;
        gitlab)
            DEVSPARK_PLATFORM_NAME="gitlab"
            DEVSPARK_PLATFORM_DISPLAY="GitLab"
            DEVSPARK_PR_CLI="glab"
            DEVSPARK_PR_CLI_INSTALL_URL="https://gitlab.com/gitlab-org/cli#installation"
            DEVSPARK_CI_DIR="."
            DEVSPARK_CI_FILE_PATTERN=".gitlab-ci.yml"
            DEVSPARK_AGENT_CONFIG_PATH="AGENTS.md"
            DEVSPARK_BRANCH_NAME_LIMIT=255
            DEVSPARK_PR_ENV_VAR="CI_MERGE_REQUEST_IID"
            ;;
        *)
            echo "ERROR: Unknown platform: $platform. Supported: github, azdo, gitlab" >&2
            return 1
            ;;
    esac
}

# Check platform CLI authentication
check_platform_auth() {
    case "$DEVSPARK_PLATFORM_NAME" in
        github) gh auth status &>/dev/null ;;
        azdo)   az account show &>/dev/null ;;
        gitlab) glab auth status &>/dev/null ;;
    esac
}

# The lowest azure-devops CLI extension version this repository has verified against a real
# project for every field the work-item helper writes. Its PowerShell twin declares the same
# value and the script parity test fails when the two drift.
DEVSPARK_WORKITEM_MIN_EXTENSION_VERSION="1.0.5"

# Compare two dotted numeric versions. Echoes -1, 0 or 1. Missing components count as zero,
# so '1.0' and '1.0.0' compare equal. Non-numeric components sort as zero rather than failing,
# because a preview suffix must not turn a capability probe into an error.
compare_devspark_version() {
    local left="$1" right="$2"
    local -a l r
    IFS='.-+' read -r -a l <<< "$left"
    IFS='.-+' read -r -a r <<< "$right"
    local count=${#l[@]}
    [[ ${#r[@]} -gt $count ]] && count=${#r[@]}
    local i lv rv
    for ((i = 0; i < count; i++)); do
        lv="${l[i]:-0}"; rv="${r[i]:-0}"
        [[ "$lv" =~ ^[0-9]+$ ]] || lv=0
        [[ "$rv" =~ ^[0-9]+$ ]] || rv=0
        if ((10#$lv < 10#$rv)); then echo "-1"; return; fi
        if ((10#$lv > 10#$rv)); then echo "1"; return; fi
    done
    echo "0"
}

# Probe the Azure DevOps work-item capability. Reports three facts independently so a caller
# can tell "no CLI" from "no extension" from "extension too old" and name the right remedy.
# Emits one value per line: cli_present extension_present extension_version minimum meets.
# Newline-delimited rather than tab-delimited because tab is IFS whitespace, and `read`
# collapses runs of it -- which silently shifts every value after the first empty one.
get_azdo_workitem_capability() {
    local cli_present="false" extension_present="false" version="" meets="false"

    if command -v az &>/dev/null; then
        cli_present="true"
        version=$(az extension show --name azure-devops --query version -o tsv 2>/dev/null || echo "")
        version="${version//[$'\t\r\n ']/}"
        [[ -n "$version" ]] && extension_present="true"
    fi

    if [[ "$extension_present" == "true" ]]; then
        if [[ "$(compare_devspark_version "$version" "$DEVSPARK_WORKITEM_MIN_EXTENSION_VERSION")" != "-1" ]]; then
            meets="true"
        fi
    fi

    printf '%s\n%s\n%s\n%s\n%s\n' \
        "$cli_present" "$extension_present" "$version" \
        "$DEVSPARK_WORKITEM_MIN_EXTENSION_VERSION" "$meets"
}

# Resolve script path: app override → team override → stock default (FR-C5)
resolve_devspark_script() {
    local script_name="$1"
    local shell="${2:-bash}"  # bash or powershell

    local repo_root
    repo_root=$(get_repo_root)

    # App-specific override (if app context is set)
    if [[ -n "${DEVSPARK_APP_ID:-}" ]]; then
        local app_doc_root
        app_doc_root=$(resolve_app_doc_root "$repo_root" "$DEVSPARK_APP_ID" 2>/dev/null || true)
        if [[ -n "$app_doc_root" ]]; then
            local app_path="$app_doc_root/scripts/$shell/$script_name"
            if [[ -f "$app_path" ]]; then echo "$app_path"; return; fi
        fi
    fi

    local team_path="$repo_root/.devspark.work/scripts/$shell/$script_name"
    local stock_path="$repo_root/.devspark/scripts/$shell/$script_name"
    local dev_path="$repo_root/scripts/$shell/$script_name"

    if [[ -f "$team_path" ]];  then echo "$team_path";  return; fi
    if [[ -f "$stock_path" ]]; then echo "$stock_path"; return; fi
    if [[ -f "$dev_path" ]];   then echo "$dev_path";   return; fi

    return 1
}

# Auto-detect and export on source
DEVSPARK_PLATFORM_NAME=""
DEVSPARK_PLATFORM_DISPLAY=""
DEVSPARK_PR_CLI=""
DEVSPARK_PR_CLI_INSTALL_URL=""
DEVSPARK_CI_DIR=""
DEVSPARK_CI_FILE_PATTERN=""
DEVSPARK_AGENT_CONFIG_PATH=""
DEVSPARK_BRANCH_NAME_LIMIT=""
DEVSPARK_PR_ENV_VAR=""

_set_platform_config "$(detect_platform)"

export DEVSPARK_PLATFORM_NAME DEVSPARK_PLATFORM_DISPLAY DEVSPARK_PR_CLI
export DEVSPARK_PR_CLI_INSTALL_URL DEVSPARK_CI_DIR DEVSPARK_CI_FILE_PATTERN
export DEVSPARK_AGENT_CONFIG_PATH DEVSPARK_BRANCH_NAME_LIMIT DEVSPARK_PR_ENV_VAR
export DEVSPARK_WORKITEM_MIN_EXTENSION_VERSION

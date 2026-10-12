#!/usr/bin/env bash
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
#
# The only place in DevSpark that talks to Azure DevOps Boards. Every operation emits exactly
# one canonical JSON line and exits 0 even when the operation failed, so callers parse a result
# instead of interpreting an exit code. A non-zero exit means the script was invoked wrongly.
#
# This is the twin of scripts/powershell/workitem.ps1. Output must be byte-identical.

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=./platform.sh
source "$SCRIPT_DIR/platform.sh"
# shellcheck source=./common.sh
source "$SCRIPT_DIR/common.sh"

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

WORKITEM_CONTRACT_VERSION=1

# Retrieved free text is capped in characters rather than bytes. The consumer limit is a text
# budget, and counting characters cannot split a multi-byte character the way a byte cut can.
WORKITEM_MAX_FIELD_CHARS=8000

UNTRUSTED_OPEN_MARKER='<<<UNTRUSTED-WORKITEM-DATA'
UNTRUSTED_CLOSE_MARKER='UNTRUSTED-WORKITEM-DATA'
UNTRUSTED_NEUTRALISED='UNTRUSTED_WORKITEM_DATA_NEUTRALIZED'

# Applied to every created item so a reader of the board can tell where it came from.
WORKITEM_AGENT_TAG='devspark-generated'

# Declared order matters: it is the order every emitted object uses.
WORKITEM_FIELD_NAMES=(title state area_path iteration_path tags description acceptance_criteria story_points assigned_to)

workitem_field_reference() {
    case "$1" in
        title)               echo 'System.Title' ;;
        state)               echo 'System.State' ;;
        area_path)           echo 'System.AreaPath' ;;
        iteration_path)      echo 'System.IterationPath' ;;
        tags)                echo 'System.Tags' ;;
        description)         echo 'System.Description' ;;
        acceptance_criteria) echo 'Microsoft.VSTS.Common.AcceptanceCriteria' ;;
        story_points)        echo 'Microsoft.VSTS.Scheduling.StoryPoints' ;;
        assigned_to)         echo 'System.AssignedTo' ;;
        *) return 1 ;;
    esac
}

WARNINGS=()
TRUNCATED_FIELDS=()

add_warning() {
    local code="$1" existing
    for existing in ${WARNINGS[@]+"${WARNINGS[@]}"}; do
        [[ "$existing" == "$code" ]] && return 0
    done
    WARNINGS+=("$code")
}

# ---------------------------------------------------------------------------
# Canonical JSON emitter
#
# Output must be byte-identical to the PowerShell twin, so nothing here relies on a
# serializer's default key ordering. jq encodes individual scalars only; every object is
# assembled from an explicit ordered key list.
# ---------------------------------------------------------------------------

# Every jq call returns JSON-encoded output and is stripped of CR. On Windows jq writes its
# line terminator in text mode, so raw multi-line text picks up a CR on every line. Encoded
# output contains no literal CR at all, which is what makes the strip safe rather than lossy:
# a carriage return inside the data survives as the two characters \r.
jq_encoded() {
    jq -c "$@" | tr -d '\r'
}

json_str() {
    # An absent value is null. The contract admits no empty string, so the distinction between
    # "not set" and "set to nothing" is collapsed here rather than at every call site.
    if [[ -z "${1:-}" ]]; then echo 'null'; return; fi
    jq_encoded -n --arg v "$1" '$v'
}

json_num() {
    if [[ -z "${1:-}" ]]; then echo 'null'; return; fi
    printf '%s\n' "$1"
}

json_bool() {
    if [[ "${1:-false}" == "true" ]]; then echo 'true'; else echo 'false'; fi
}

# Join already-encoded JSON fragments into an array.
json_array_of() {
    local out="" item
    for item in "$@"; do
        [[ -n "$out" ]] && out+=", "
        out+="$item"
    done
    printf '[%s]' "$out"
}

# Join already-encoded "key<TAB>fragment" pairs into an object, preserving argument order.
json_object_of() {
    local out="" pair key frag
    for pair in "$@"; do
        key="${pair%%$'\t'*}"
        frag="${pair#*$'\t'}"
        [[ -n "$out" ]] && out+=", "
        out+="$(json_str "$key"): $frag"
    done
    printf '{%s}' "$out"
}

# ---------------------------------------------------------------------------
# Result emission
# ---------------------------------------------------------------------------

# Spelled as a variable because a literal {} cannot appear as a parameter-expansion default:
# the closing brace would end the expansion, and escaping it leaves the backslashes in the
# result, which silently produced invalid JSON.
JSON_EMPTY_OBJECT='{}'

emit_result() {
    local data_fragment="${1:-$JSON_EMPTY_OBJECT}" error_fragment="${2:-null}" ok="true"
    [[ "$error_fragment" != "null" ]] && ok="false"

    local warnings_fragment=()
    local w
    for w in ${WARNINGS[@]+"${WARNINGS[@]}"}; do
        warnings_fragment+=("$(json_str "$w")")
    done

    printf '%s\n' "$(json_object_of \
        "contract$(printf '\t')$WORKITEM_CONTRACT_VERSION" \
        "operation$(printf '\t')$(json_str "$OPERATION")" \
        "ok$(printf '\t')$ok" \
        "error$(printf '\t')$error_fragment" \
        "warnings$(printf '\t')$(json_array_of ${warnings_fragment[@]+"${warnings_fragment[@]}"})" \
        "data$(printf '\t')$data_fragment")"
    exit 0
}

# Every error names what went wrong and what the caller can do about it. The two are always
# distinct: a cause that repeats itself as an action tells the reader nothing.
emit_failure() {
    local code="$1" cause="$2" action="$3" data_fragment="${4:-$JSON_EMPTY_OBJECT}"
    local err
    err="$(json_object_of \
        "code$(printf '\t')$(json_str "$code")" \
        "cause$(printf '\t')$(json_str "$cause")" \
        "action$(printf '\t')$(json_str "$action")")"
    emit_result "$data_fragment" "$err"
}

# A usage error is the caller's mistake, not a handled outcome, so it is the one path that
# leaves the canonical envelope behind and exits non-zero.
exit_usage() {
    echo "workitem: $1" >&2
    exit 2
}

# ---------------------------------------------------------------------------
# Untrusted-data handling
# ---------------------------------------------------------------------------

# Retrieved free text is attacker-influenced. It is neutralised, capped, then wrapped in a
# fixed envelope so a caller cannot mistake it for instructions. Neutralisation runs before
# the cap so the cap can never split a marker into something that looks like a real one.
#
# The whole transformation happens inside jq and returns an encoded JSON string, because
# pulling multi-line text out of jq first is what corrupts line endings on Windows.
#
# This function raises no warning of its own: it is called from inside a command
# substitution, and a warning raised there would be lost with the subshell. Callers pair it
# with note_truncation, which runs in the caller's own shell.
item_field_protected() {
    local json="$1" name="$2" out

    out="$(jq_encoded --arg f "$name" \
        --arg open "$UNTRUSTED_OPEN_MARKER" \
        --arg close "$UNTRUSTED_CLOSE_MARKER" \
        --arg neut "$UNTRUSTED_NEUTRALISED" \
        --argjson max "$WORKITEM_MAX_FIELD_CHARS" '
        (.fields[$f] // null)
        | if . == null or . == "" then null
          else (tostring | gsub($close; $neut))
               | (if (length > $max) then .[0:$max] else . end)
               | ($open + "\n" + . + "\n" + $close)
          end' <<< "$json" 2>/dev/null)"
    [[ -z "$out" ]] && out='null'
    printf '%s' "$out"
}

# Raises content_truncated when the named field exceeds the cap, and records the
# caller-facing name of what was cut. Must be called from the caller's own shell, never from
# inside a command substitution.
note_truncation() {
    local json="$1" reference="$2" name="$3" length existing
    length="$(jq -r --arg f "$reference" '((.fields[$f] // "") | tostring | length)' <<< "$json" 2>/dev/null | tr -d '\r')"
    if [[ -n "$length" ]] && (( length > WORKITEM_MAX_FIELD_CHARS )); then
        add_warning 'content_truncated'
        for existing in ${TRUNCATED_FIELDS[@]+"${TRUNCATED_FIELDS[@]}"}; do
            [[ "$existing" == "$name" ]] && return 0
        done
        TRUNCATED_FIELDS+=("$name")
    fi
}

# The caller-facing names of every field the cap shortened, encoded as a JSON array.
truncated_fragment() {
    local names=() n
    for n in ${TRUNCATED_FIELDS[@]+"${TRUNCATED_FIELDS[@]}"}; do names+=("$(json_str "$n")"); done
    json_array_of ${names[@]+"${names[@]}"}
}

# ---------------------------------------------------------------------------
# Azure CLI invocation
# ---------------------------------------------------------------------------

AZ_EXIT=0
AZ_STDOUT=""
AZ_STDERR=""

# Strip anything credential-shaped before any CLI diagnostic reaches the caller, then cap the
# remainder. Verbose and debug are never enabled: they are the switches that make the CLI print
# request headers.
scrub_stderr() {
    local value="${1:-}"
    [[ -z "$value" ]] && { printf ''; return; }
    value="$(printf '%s' "$value" \
        | sed -E 's/[Bb]earer[[:space:]]+[A-Za-z0-9._~+/-]+=*/bearer [redacted]/g' \
        | sed -E 's/([Pp]at|[Tt]oken|[Pp]assword|[Ss]ecret|[Aa]uthorization)[[:space:]]*[=:][[:space:]]*[^[:space:]]+/\1=[redacted]/g' \
        | sed -E 's/\b[A-Za-z0-9_-]{40,}\b/[redacted]/g' \
        | tr '\n' ' ' \
        | sed -E 's/[[:space:]]+/ /g; s/^ //; s/ $//')"
    printf '%s' "${value:0:400}"
}

# Run az with an argument array. Nothing is interpolated into a command string, so free text
# never has to survive a round trip through shell quoting.
invoke_az() {
    local out_file err_file
    out_file="$(mktemp)"
    err_file="$(mktemp)"
    az "$@" >"$out_file" 2>"$err_file"
    AZ_EXIT=$?
    AZ_STDOUT="$(cat "$out_file")"
    AZ_STDERR="$(scrub_stderr "$(cat "$err_file")")"
    rm -f "$out_file" "$err_file"
    return 0
}

# Write a request body to a process-unique UTF-8 file with no BOM and hand the CLI the path.
# The caller removes it on every exit path, including failure.
new_payload_file() {
    local path
    path="$(mktemp "${TMPDIR:-/tmp}/devspark-workitem-$$-XXXXXXXX.json")"
    printf '%s' "$1" > "$path"
    printf '%s' "$path"
}

# ---------------------------------------------------------------------------
# Fixture-backed mode
# ---------------------------------------------------------------------------

FIXTURE_PATH=""

init_fixture() {
    [[ -z "$FIXTURE_PATH" ]] && return 0
    if [[ "$CONFIRMED" == "true" ]]; then
        exit_usage 'a fixture path and --confirmed are mutually exclusive; fixture mode never performs a real write'
    fi
    [[ -f "$FIXTURE_PATH" ]] || exit_usage "fixture not found: $FIXTURE_PATH"
    add_warning 'fixture_mode'
}

get_fixture_response() {
    [[ -z "$FIXTURE_PATH" ]] && return 1
    local out
    out="$(jq -c --arg k "$1" '.responses[$k] // empty' "$FIXTURE_PATH" 2>/dev/null)"
    [[ -z "$out" ]] && return 1
    printf '%s' "$out"
}

# ---------------------------------------------------------------------------
# Context
# ---------------------------------------------------------------------------

CTX_CONFIGURED=""; CTX_PLATFORM=""; CTX_ORG=""; CTX_PROJECT=""; CTX_REPO=""
CTX_AREA=""; CTX_ITERATION=""; CTX_TYPE=""; CTX_TAGS=""; CTX_REVIEW=""

resolve_context() {
    local fields=()
    mapfile -t fields < <(get_work_tracking_context "$ARG_ORG" "$ARG_PROJECT" "$ARG_AREA")
    CTX_CONFIGURED="${fields[0]:-false}"
    CTX_PLATFORM="${fields[1]:-}"
    CTX_ORG="${fields[2]:-}"
    CTX_PROJECT="${fields[3]:-}"
    CTX_REPO="${fields[4]:-}"
    CTX_AREA="${fields[5]:-}"
    CTX_ITERATION="${fields[6]:-}"
    CTX_TYPE="${fields[7]:-}"
    CTX_TAGS="${fields[8]:-}"
    CTX_REVIEW="${fields[9]:-true}"
}

# The marker is appended rather than configurable: a team that could switch it off would lose
# the only way to tell an agent-created item from a hand-written one.
create_tags() {
    local configured="${CTX_TAGS//,/; }"
    if [[ -n "$configured" ]]; then
        printf '%s; %s' "$configured" "$WORKITEM_AGENT_TAG"
    else
        printf '%s' "$WORKITEM_AGENT_TAG"
    fi
}

organization_url() {
    local org="$1"
    if [[ "$org" =~ ^https?:// ]]; then printf '%s' "${org%/}"; else printf 'https://dev.azure.com/%s' "$org"; fi
}

assert_context() {
    if [[ -z "$CTX_ORG" ]]; then
        emit_failure 'context_unresolved' \
            'no organization could be resolved from an argument, the work_tracking block, the top-level config or the origin remote' \
            'set "organization" in .devspark.work/devspark.json or pass --organization'
    fi
    if [[ -z "$CTX_PROJECT" ]]; then
        emit_failure 'context_unresolved' \
            'no project could be resolved from an argument, the work_tracking block, the top-level config or the origin remote' \
            'set "project" in .devspark.work/devspark.json or pass --project'
    fi
}

CAP_CLI=""; CAP_EXT=""; CAP_VERSION=""; CAP_MIN=""; CAP_MEETS=""

probe_capability() {
    local fields=()
    mapfile -t fields < <(get_azdo_workitem_capability)
    CAP_CLI="${fields[0]:-false}"
    CAP_EXT="${fields[1]:-false}"
    CAP_VERSION="${fields[2]:-}"
    CAP_MIN="${fields[3]:-}"
    CAP_MEETS="${fields[4]:-false}"
}

assert_capability() {
    [[ -n "$FIXTURE_PATH" ]] && return 0
    probe_capability
    if [[ "$CAP_CLI" != "true" ]]; then
        emit_failure 'cli_missing' \
            'the az command was not found on PATH' \
            'install the Azure CLI from https://learn.microsoft.com/cli/azure/install-azure-cli and reopen the shell'
    fi
    if [[ "$CAP_EXT" != "true" ]]; then
        emit_failure 'extension_missing' \
            'the azure-devops extension is not installed for the Azure CLI' \
            "run: az extension add --name azure-devops (version $CAP_MIN or later)"
    fi
    if [[ "$CAP_MEETS" != "true" ]]; then
        emit_failure 'extension_missing' \
            "the installed azure-devops extension is version $CAP_VERSION, below the verified minimum" \
            "run: az extension update --name azure-devops (version $CAP_MIN or later)"
    fi
}

assert_authenticated() {
    [[ -n "$FIXTURE_PATH" ]] && return 0
    invoke_az account show --only-show-errors --output none
    if [[ $AZ_EXIT -ne 0 ]]; then
        emit_failure 'auth_failed' \
            'the Azure CLI has no usable credential for this organization' \
            'run: az login, then re-run the command'
    fi
}

# A write happens because a person said so in this run. No configuration value, environment
# variable or autonomy setting can stand in for that.
#
# This exits as a usage error rather than returning an error envelope. The flag is a required
# argument for a write, and a caller that omitted it has not made a request the helper can
# answer -- as distinct from a request it tried and could not complete.
#
# Fixture mode is exempt because a recorded response has no service behind it: the write path
# records what it would have sent and stops. Confirming a write against invented data would
# mean nothing, which is why supplying both is rejected as a usage error instead.
assert_confirmed() {
    [[ -n "$FIXTURE_PATH" ]] && return
    if [[ "$CONFIRMED" != "true" ]]; then
        exit_usage "operation '$OPERATION' writes to Azure DevOps and requires --confirmed; re-run with --confirmed once the proposed change has been reviewed"
    fi
}

assert_rev() {
    [[ -z "$REV" ]] && exit_usage 'a write requires --rev carrying the revision the proposal was built from'
}

# ---------------------------------------------------------------------------
# Work item retrieval
# ---------------------------------------------------------------------------

ITEM_JSON=""

get_work_item_snapshot() {
    local id="$1" fixture
    if fixture="$(get_fixture_response "read:$id")"; then
        ITEM_JSON="$fixture"
        return 0
    fi
    if [[ -n "$FIXTURE_PATH" ]]; then
        emit_failure 'item_unresolvable' \
            "the fixture has no recorded response for work item $id" \
            'add a "read:<id>" entry to the fixture, or run without --fixture'
    fi

    invoke_az boards work-item show --id "$id" --org "$(organization_url "$CTX_ORG")" \
        --only-show-errors --output json
    if [[ $AZ_EXIT -ne 0 ]]; then
        emit_failure 'item_unresolvable' \
            "work item $id could not be retrieved: $AZ_STDERR" \
            'confirm the id exists in this project and that the signed-in account can read it'
    fi
    if ! jq -e . >/dev/null 2>&1 <<< "$AZ_STDOUT"; then
        emit_failure 'item_unresolvable' \
            "work item $id returned a response that could not be parsed" \
            'retry, and if it persists run the same az boards work-item show command directly to inspect it'
    fi
    ITEM_JSON="$AZ_STDOUT"
}

item_field() {
    local out
    out="$(jq -r --arg f "$1" '.fields[$f] // empty' <<< "$ITEM_JSON" 2>/dev/null | tr -d '\r')"
    printf '%s' "$out"
}

item_field_from() {
    local json="$1" name="$2" out
    out="$(jq -r --arg f "$name" '.fields[$f] // empty' <<< "$json" 2>/dev/null | tr -d '\r')"
    printf '%s' "$out"
}

# Encoded field value: a JSON scalar or null, never raw text.
item_field_encoded() {
    local json="$1" name="$2" out
    out="$(jq_encoded --arg f "$name" '.fields[$f] // null' <<< "$json" 2>/dev/null)"
    [[ -z "$out" ]] && out='null'
    printf '%s' "$out"
}

# Builds the pinned snapshot object into SNAPSHOT_JSON.
#
# The result is assigned to a global rather than written to stdout on purpose: this function
# raises warnings, and capturing it with $(...) would run it in a subshell where every
# add_warning call would be discarded along with that subshell.
SNAPSHOT_JSON='{}'

snapshot_fragment() {
    local wrap="${1:-wrap}"
    local id rev type title state area iteration tags description acceptance story assigned url
    local area_raw id_raw

    id="$(jq_encoded '.id // null' <<< "$ITEM_JSON")"
    id_raw="$(jq -r '.id // empty' <<< "$ITEM_JSON" | tr -d '\r')"
    rev="$(item_field_encoded "$ITEM_JSON" 'System.Rev')"
    [[ "$rev" == "null" ]] && rev="$(jq_encoded '.rev // null' <<< "$ITEM_JSON")"
    type="$(item_field_encoded "$ITEM_JSON" 'System.WorkItemType')"
    title="$(item_field_encoded "$ITEM_JSON" 'System.Title')"
    state="$(item_field_encoded "$ITEM_JSON" 'System.State')"
    area="$(item_field_encoded "$ITEM_JSON" 'System.AreaPath')"
    iteration="$(item_field_encoded "$ITEM_JSON" 'System.IterationPath')"
    tags="$(item_field_encoded "$ITEM_JSON" 'System.Tags')"
    story="$(item_field_encoded "$ITEM_JSON" 'Microsoft.VSTS.Scheduling.StoryPoints')"
    assigned="$(jq_encoded '.fields["System.AssignedTo"].displayName // .fields["System.AssignedTo"] // null' <<< "$ITEM_JSON" 2>/dev/null)"
    [[ -z "$assigned" ]] && assigned='null'

    area_raw="$(item_field 'System.AreaPath')"
    if [[ -n "$CTX_AREA" && -n "$area_raw" && "$area_raw" != "$CTX_AREA"* ]]; then add_warning 'area_mismatch'; fi
    [[ "$iteration" == "null" ]] && add_warning 'iteration_missing'
    [[ "$story" == "null" ]] && add_warning 'story_points_unsupported'

    if [[ "$wrap" == "wrap" ]]; then
        note_truncation "$ITEM_JSON" 'System.Description' 'description'
        note_truncation "$ITEM_JSON" 'Microsoft.VSTS.Common.AcceptanceCriteria' 'acceptance_criteria'
        description="$(item_field_protected "$ITEM_JSON" 'System.Description')"
        acceptance="$(item_field_protected "$ITEM_JSON" 'Microsoft.VSTS.Common.AcceptanceCriteria')"
    else
        description="$(item_field_encoded "$ITEM_JSON" 'System.Description')"
        acceptance="$(item_field_encoded "$ITEM_JSON" 'Microsoft.VSTS.Common.AcceptanceCriteria')"
    fi

    url='null'
    if [[ -n "$CTX_ORG" && -n "$id_raw" ]]; then
        url="$(json_str "$(organization_url "$CTX_ORG")/$CTX_PROJECT/_workitems/edit/$id_raw")"
    fi

    SNAPSHOT_JSON="$(json_object_of \
        "id$(printf '\t')$id" \
        "rev$(printf '\t')$rev" \
        "type$(printf '\t')$type" \
        "title$(printf '\t')$title" \
        "state$(printf '\t')$state" \
        "area_path$(printf '\t')$area" \
        "iteration_path$(printf '\t')$iteration" \
        "tags$(printf '\t')$tags" \
        "description$(printf '\t')$description" \
        "acceptance_criteria$(printf '\t')$acceptance" \
        "story_points$(printf '\t')$story" \
        "assigned_to$(printf '\t')$assigned" \
        "url$(printf '\t')$url" \
        "truncated$(printf '\t')$(truncated_fragment)")"
}

# ---------------------------------------------------------------------------
# Field parsing
# ---------------------------------------------------------------------------

SET_NAMES=()
SET_VALUES=()

# A work-item URL names the organization, the project, and the item in one string, so
# accepting it removes three chances to pair an id with the wrong project by hand.
apply_url_argument() {
    [[ -z "$URL" ]] && return 0
    local rest org project number
    if [[ "$URL" =~ ^https?://dev\.azure\.com/([^/]+)/(.+)$ ]]; then
        org="${BASH_REMATCH[1]}"; rest="${BASH_REMATCH[2]}"
    elif [[ "$URL" =~ ^https?://([^./]+)\.visualstudio\.com/(.+)$ ]]; then
        org="${BASH_REMATCH[1]}"; rest="${BASH_REMATCH[2]}"
    else
        exit_usage "not an Azure DevOps work item URL: $URL"
    fi
    [[ "$rest" =~ ^([^/]+)/_workitems/edit/([0-9]+) ]] || exit_usage "not an Azure DevOps work item URL: $URL"
    project="${BASH_REMATCH[1]}"; number="${BASH_REMATCH[2]}"

    [[ -n "$ID" && "$ID" != "$number" ]] && exit_usage "--id $ID and --url disagree about which work item is meant"
    ID="$number"
    [[ -z "$ARG_ORG" ]] && ARG_ORG="$(printf '%s' "$org" | sed 's/%20/ /g')"
    [[ -z "$ARG_PROJECT" ]] && ARG_PROJECT="$(printf '%s' "$project" | sed 's/%20/ /g')"
    return 0
}

# Accepts the forms a person actually writes in a pull request: AB#123, #123, a bare number,
# or a full work-item URL. Anything else returns nothing, so the caller can reject it rather
# than guess at what was meant.
reference_id_of() {
    local raw="$1"
    if [[ "$raw" =~ _workitems/edit/([0-9]+) ]]; then printf '%s' "${BASH_REMATCH[1]}"; return 0; fi
    if [[ "$raw" =~ ^(AB#|ab#|#)?([0-9]+)$ ]]; then printf '%s' "${BASH_REMATCH[2]}"; return 0; fi
    return 0
}

parse_set_arguments() {    local entry name value
    for entry in ${SET_ENTRIES[@]+"${SET_ENTRIES[@]}"}; do
        name="${entry%%=*}"
        value="${entry#*=}"
        if ! workitem_field_reference "$name" >/dev/null 2>&1; then
            exit_usage "unknown field '$name'; supported: ${WORKITEM_FIELD_NAMES[*]}"
        fi
        SET_NAMES+=("$name")
        SET_VALUES+=("$value")
    done
}

set_value_of() {
    local want="$1" i
    for i in "${!SET_NAMES[@]}"; do
        if [[ "${SET_NAMES[i]}" == "$want" ]]; then printf '%s' "${SET_VALUES[i]}"; return 0; fi
    done
    return 1
}

# ---------------------------------------------------------------------------
# Writes
# ---------------------------------------------------------------------------

# Every write goes through the REST endpoint with a JSON Patch body carried in a file. The
# az boards commands expose no file-based payload and no acceptance-criteria parameter, so
# they cannot satisfy the transport rule for the fields this feature actually writes.
PATCH_EXIT=0
PATCH_STDOUT=""
PATCH_STDERR=""

invoke_patch_write() {
    local id="$1" type="${2:-}"
    local ops=() i name value reference payload payload_file

    for i in "${!PATCH_NAMES[@]}"; do
        name="${PATCH_NAMES[i]}"
        value="${PATCH_VALUES[i]}"
        reference="$(workitem_field_reference "$name")"
        ops+=("$(json_object_of \
            "op$(printf '\t')$(json_str 'add')" \
            "path$(printf '\t')$(json_str "/fields/$reference")" \
            "value$(printf '\t')$(json_str "$value")")")
    done
    payload="$(json_array_of ${ops[@]+"${ops[@]}"})"

    payload_file="$(new_payload_file "$payload")"
    trap 'rm -f "$payload_file"' RETURN

    if [[ -n "$FIXTURE_PATH" ]]; then
        PATCH_EXIT=0; PATCH_STDOUT='{}'; PATCH_STDERR=""
        rm -f "$payload_file"
        return 0
    fi

    local args=(devops invoke --area wit --resource workitems
        --org "$(organization_url "$CTX_ORG")"
        --in-file "$payload_file" --encoding utf-8
        --media-type application/json-patch+json
        --only-show-errors --output json)
    if [[ -n "$id" ]]; then
        args+=(--route-parameters "project=$CTX_PROJECT" "id=$id" --http-method PATCH)
    else
        args+=(--route-parameters "project=$CTX_PROJECT" "type=$type" --http-method POST)
    fi
    args+=(--api-version 7.0)

    invoke_az "${args[@]}"
    PATCH_EXIT=$AZ_EXIT; PATCH_STDOUT="$AZ_STDOUT"; PATCH_STDERR="$AZ_STDERR"
    rm -f "$payload_file"
}

# A proposal is built against a revision. If the item moved since, the write is refused rather
# than resolved: silently overwriting someone else's edit is the failure this guards against.
revision_is_current() {
    local id="$1" current
    get_work_item_snapshot "$id"
    current="$(item_field 'System.Rev')"
    [[ -z "$current" ]] && current="$(jq -r '.rev // empty' <<< "$ITEM_JSON")"
    [[ "$current" == "$REV" ]]
}

# The record of what a run changed names fields only. Values are deliberately absent: an audit
# trail that quotes the body it wrote becomes a second copy of the data it is auditing.
audit_fragment() {
    local op="$1" id="$2" rev_before="$3" rev_after="$4"
    shift 4
    local names=() n
    for n in "$@"; do names+=("$(json_str "$n")"); done
    json_object_of \
        "operation$(printf '\t')$(json_str "$op")" \
        "id$(printf '\t')$(json_str "$id")" \
        "fields$(printf '\t')$(json_array_of ${names[@]+"${names[@]}"})" \
        "rev_before$(printf '\t')$(json_num "$rev_before")" \
        "rev_after$(printf '\t')$(json_num "$rev_after")"
}

# ---------------------------------------------------------------------------
# Operations
# ---------------------------------------------------------------------------

op_check() {
    resolve_context
    probe_capability

    local authenticated="false" ext="false"
    if [[ "$CAP_CLI" == "true" ]]; then
        invoke_az account show --only-show-errors --output none
        [[ $AZ_EXIT -eq 0 ]] && authenticated="true"
    fi
    [[ "$CAP_EXT" == "true" && "$CAP_MEETS" == "true" ]] && ext="true"

    emit_result "$(json_object_of \
        "cli_present$(printf '\t')$(json_bool "$CAP_CLI")" \
        "extension_present$(printf '\t')$(json_bool "$ext")" \
        "authenticated$(printf '\t')$(json_bool "$authenticated")" \
        "organization$(printf '\t')$(json_str "$CTX_ORG")" \
        "project$(printf '\t')$(json_str "$CTX_PROJECT")" \
        "work_tracking_configured$(printf '\t')$(json_bool "$CTX_CONFIGURED")" \
        "default_area_path$(printf '\t')$(json_str "$CTX_AREA")")"
}

op_read() {
    [[ -z "$ID" ]] && exit_usage 'read requires --id'
    resolve_context
    assert_context
    assert_capability
    assert_authenticated
    get_work_item_snapshot "$ID"
    snapshot_fragment wrap
    emit_result "$SNAPSHOT_JSON"
}

op_propose_update() {
    [[ -z "$ID" ]] && exit_usage 'propose-update requires --id'
    parse_set_arguments
    [[ ${#SET_NAMES[@]} -eq 0 ]] && exit_usage 'propose-update requires at least one --set name=value'

    resolve_context
    assert_context
    assert_capability
    assert_authenticated
    get_work_item_snapshot "$ID"

    local rev entries=() i name current
    rev="$(item_field_encoded "$ITEM_JSON" 'System.Rev')"
    for i in "${!SET_NAMES[@]}"; do
        name="${SET_NAMES[i]}"
        note_truncation "$ITEM_JSON" "$(workitem_field_reference "$name")" "$name"
        current="$(item_field_protected "$ITEM_JSON" "$(workitem_field_reference "$name")")"
        entries+=("$(json_object_of \
            "field$(printf '\t')$(json_str "$name")" \
            "current$(printf '\t')$current" \
            "proposed$(printf '\t')$(json_str "${SET_VALUES[i]}")")")
    done

    emit_result "$(json_object_of \
        "proposed$(printf '\t')$(json_array_of ${entries[@]+"${entries[@]}"})" \
        "rev$(printf '\t')$rev" \
        "truncated$(printf '\t')$(truncated_fragment)")"
}

PATCH_NAMES=()
PATCH_VALUES=()

op_apply_update() {
    [[ -z "$ID" ]] && exit_usage 'apply-update requires --id'
    assert_rev
    parse_set_arguments
    [[ ${#SET_NAMES[@]} -eq 0 ]] && exit_usage 'apply-update requires at least one --set name=value'

    resolve_context
    assert_context
    assert_confirmed
    assert_capability
    assert_authenticated

    if ! revision_is_current "$ID"; then
        emit_result "$(json_object_of \
            "written$(printf '\t')false" \
            "verified$(printf '\t')null" \
            "divergence$(printf '\t')[]" \
            "refused$(printf '\t')$(json_str 'stale_revision')" \
            "audit$(printf '\t')null")"
    fi

    PATCH_NAMES=("${SET_NAMES[@]}")
    PATCH_VALUES=("${SET_VALUES[@]}")
    invoke_patch_write "$ID"
    if [[ $PATCH_EXIT -ne 0 ]]; then
        emit_failure 'item_unresolvable' \
            "the update to work item $ID was rejected: $PATCH_STDERR" \
            'confirm the field names are valid for this work item type and that the account can edit it'
    fi

    # A write is not finished until it has been read back. The caller is told what the item
    # actually holds now, not what was sent.
    get_work_item_snapshot "$ID"
    local divergent=() i name actual after_rev verified
    for i in "${!SET_NAMES[@]}"; do
        name="${SET_NAMES[i]}"
        actual="$(item_field "$(workitem_field_reference "$name")")"
        [[ "$actual" != "${SET_VALUES[i]}" ]] && divergent+=("$(json_str "$name")")
    done
    after_rev="$(item_field 'System.Rev')"
    snapshot_fragment wrap
    verified="$SNAPSHOT_JSON"

    emit_result "$(json_object_of \
        "written$(printf '\t')true" \
        "verified$(printf '\t')$verified" \
        "divergence$(printf '\t')$(json_array_of ${divergent[@]+"${divergent[@]}"})" \
        "refused$(printf '\t')null" \
        "audit$(printf '\t')$(audit_fragment 'apply-update' "$ID" "$REV" "$after_rev" "${SET_NAMES[@]}")")"
}

op_comment() {
    [[ -z "$ID" ]] && exit_usage 'comment requires --id'
    [[ -z "$TEXT" ]] && exit_usage 'comment requires --text'
    assert_rev

    resolve_context
    assert_context
    assert_confirmed
    assert_capability
    assert_authenticated

    if ! revision_is_current "$ID"; then
        emit_result "$(json_object_of \
            "comment_id$(printf '\t')null" \
            "verified$(printf '\t')null" \
            "divergence$(printf '\t')[]" \
            "refused$(printf '\t')$(json_str 'stale_revision')" \
            "audit$(printf '\t')null")"
    fi

    get_work_item_snapshot "$ID"
    local before_json="$ITEM_JSON" payload_file comment_id=""
    payload_file="$(new_payload_file "$(json_object_of "text$(printf '\t')$(json_str "$TEXT")")")"

    if [[ -n "$FIXTURE_PATH" ]]; then
        AZ_EXIT=0; AZ_STDOUT='{"id": 0}'; AZ_STDERR=""
    else
        # A three-part preview revision (e.g. 7.0-preview.3) crashes the installed azure-devops
        # extension's own client-side apiVersionToFloat() check; the bare '-preview' form
        # negotiates the same server-side preview revision without it.
        invoke_az devops invoke --area wit --resource comments \
            --org "$(organization_url "$CTX_ORG")" \
            --route-parameters "project=$CTX_PROJECT" "workItemId=$ID" \
            --http-method POST --in-file "$payload_file" --encoding utf-8 \
            --api-version 7.0-preview --only-show-errors --output json
    fi
    rm -f "$payload_file"

    if [[ $AZ_EXIT -ne 0 ]]; then
        emit_failure 'item_unresolvable' \
            "the comment on work item $ID was rejected: $AZ_STDERR" \
            'confirm the id exists and that the signed-in account can comment on it'
    fi
    comment_id="$(jq -r '.id // empty' <<< "$AZ_STDOUT" 2>/dev/null)"

    # A comment must not disturb the item's own fields. The verification read exists to prove it.
    get_work_item_snapshot "$ID"
    local divergent=() name before after after_rev verified
    for name in "${WORKITEM_FIELD_NAMES[@]}"; do
        before="$(item_field_from "$before_json" "$(workitem_field_reference "$name")")"
        after="$(item_field "$(workitem_field_reference "$name")")"
        [[ "$before" != "$after" ]] && divergent+=("$(json_str "$name")")
    done
    after_rev="$(item_field 'System.Rev')"
    snapshot_fragment wrap
    verified="$SNAPSHOT_JSON"

    emit_result "$(json_object_of \
        "comment_id$(printf '\t')$(json_num "$comment_id")" \
        "verified$(printf '\t')$verified" \
        "divergence$(printf '\t')$(json_array_of ${divergent[@]+"${divergent[@]}"})" \
        "refused$(printf '\t')null" \
        "audit$(printf '\t')$(audit_fragment 'comment' "$ID" "$REV" "$after_rev")")"
}

resolve_create_type() {
    local type
    type="$TYPE"
    [[ -z "$type" ]] && type="${CTX_TYPE:-Task}"
    local lowered
    lowered="$(printf '%s' "$type" | tr '[:upper:]' '[:lower:]' | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"
    if [[ "$lowered" == "user story" ]]; then
        emit_failure 'type_unsupported' \
            'creating a User Story is prohibited; DevSpark never creates one' \
            'set work_tracking.default_work_item_type to a type other than User Story, or name another work item type'
    fi
    CREATE_TYPE="$type"
}

op_propose_create() {
    parse_set_arguments
    [[ -z "$TITLE" ]] && exit_usage 'propose-create requires --title'
    [[ -z "$DESCRIPTION_HTML" ]] && exit_usage 'propose-create requires --description-html'
    [[ -z "$ACCEPTANCE_HTML" ]] && exit_usage 'propose-create requires --acceptance-criteria-html'

    resolve_context
    assert_context
    resolve_create_type

    if [[ -z "$CTX_AREA" ]]; then
        emit_failure 'context_unresolved' \
            'no default_area_path is configured, so a created item would land wherever the project defaults put it' \
            'add work_tracking.default_area_path to .devspark.work/devspark.json'
    fi

    local type="$CREATE_TYPE" story_supported="false"

    # Story points only exist on some process templates. Saying so before the write is what
    # keeps a missing field from becoming a failed create.
    if set_value_of story_points >/dev/null 2>&1; then story_supported="true"; else add_warning 'story_points_unsupported'; fi
    [[ -z "$CTX_ITERATION" ]] && add_warning 'iteration_missing'

    local draft
    draft="$(json_object_of \
        "work_item_type$(printf '\t')$(json_str "$type")" \
        "title$(printf '\t')$(json_str "$TITLE")" \
        "description$(printf '\t')$(json_str "$DESCRIPTION_HTML")" \
        "acceptance_criteria$(printf '\t')$(json_str "$ACCEPTANCE_HTML")" \
        "area_path$(printf '\t')$(json_str "$CTX_AREA")" \
        "iteration_path$(printf '\t')$(json_str "$CTX_ITERATION")" \
        "tags$(printf '\t')$(json_str "$(create_tags)")")"

    emit_result "$(json_object_of \
        "draft$(printf '\t')$draft" \
        "story_points_supported$(printf '\t')$(json_bool "$story_supported")")"
}

op_apply_create() {
    parse_set_arguments
    [[ -z "$TITLE" ]] && exit_usage 'apply-create requires --title'
    [[ -z "$DESCRIPTION_HTML" ]] && exit_usage 'apply-create requires --description-html'
    [[ -z "$ACCEPTANCE_HTML" ]] && exit_usage 'apply-create requires --acceptance-criteria-html'

    resolve_context
    assert_context
    resolve_create_type
    assert_confirmed
    assert_capability
    assert_authenticated

    if [[ -z "$CTX_AREA" ]]; then
        emit_failure 'context_unresolved' \
            'no default_area_path is configured, so a created item would land wherever the project defaults put it' \
            'add work_tracking.default_area_path to .devspark.work/devspark.json'
    fi

    local type="$CREATE_TYPE" i

    PATCH_NAMES=("title" "description" "acceptance_criteria")
    PATCH_VALUES=("$TITLE" "$DESCRIPTION_HTML" "$ACCEPTANCE_HTML")
    for i in "${!SET_NAMES[@]}"; do
        PATCH_NAMES+=("${SET_NAMES[i]}")
        PATCH_VALUES+=("${SET_VALUES[i]}")
    done
    PATCH_NAMES+=("area_path"); PATCH_VALUES+=("$CTX_AREA")
    if [[ -n "$CTX_ITERATION" ]]; then PATCH_NAMES+=("iteration_path"); PATCH_VALUES+=("$CTX_ITERATION"); fi
    PATCH_NAMES+=("tags"); PATCH_VALUES+=("$(create_tags)")

    invoke_patch_write "" "$type"
    if [[ $PATCH_EXIT -ne 0 ]]; then
        emit_failure 'type_unsupported' \
            "a '$type' work item could not be created: $PATCH_STDERR" \
            "confirm the type exists in this project's process template and set work_tracking.default_work_item_type accordingly"
    fi

    local new_id
    new_id="$(jq -r '.id // empty' <<< "$PATCH_STDOUT" 2>/dev/null)"
    if [[ -z "$new_id" ]]; then
        emit_failure 'item_unresolvable' \
            'the create call returned no work item id, so the result could not be verified' \
            'check the project in the browser before retrying, so a duplicate is not created'
    fi

    get_work_item_snapshot "$new_id"
    local divergent=() name actual after_rev verified
    for i in "${!PATCH_NAMES[@]}"; do
        name="${PATCH_NAMES[i]}"
        actual="$(item_field "$(workitem_field_reference "$name")")"
        [[ "$actual" != "${PATCH_VALUES[i]}" ]] && divergent+=("$(json_str "$name")")
    done
    after_rev="$(item_field 'System.Rev')"
    snapshot_fragment wrap
    verified="$SNAPSHOT_JSON"

    emit_result "$(json_object_of \
        "id$(printf '\t')$(json_num "$new_id")" \
        "url$(printf '\t')$(json_str "$(organization_url "$CTX_ORG")/$CTX_PROJECT/_workitems/edit/$new_id")" \
        "verified$(printf '\t')$verified" \
        "divergence$(printf '\t')$(json_array_of ${divergent[@]+"${divergent[@]}"})" \
        "audit$(printf '\t')$(audit_fragment 'apply-create' "$new_id" "" "$after_rev" "${PATCH_NAMES[@]}")")"
}

op_verify_link() {
    [[ ${#REFERENCES[@]} -eq 0 ]] && exit_usage 'verify-link requires at least one --reference'
    resolve_context
    assert_context

    # Only an explicit reference counts. Nothing here searches for a plausible match, because a
    # wrong guess writes to somebody else's work item.
    local entries=() seen=" " raw number resolves in_area url probe_json
    for raw in "${REFERENCES[@]}"; do
        [[ -z "$raw" ]] && continue
        number="$(reference_id_of "$raw")"
        [[ -z "$number" ]] && exit_usage "not a work item reference: $raw"
        [[ "$seen" == *" $number "* ]] && continue
        seen+="$number "

        resolves="false"; in_area="false"; url=""
        probe_json=""
        if probe_json="$(get_fixture_response "read:$number")"; then
            :
        elif [[ -z "$FIXTURE_PATH" ]] && command -v az >/dev/null 2>&1; then
            invoke_az boards work-item show --id "$number" --org "$(organization_url "$CTX_ORG")" \
                --only-show-errors --output json
            [[ $AZ_EXIT -eq 0 ]] && probe_json="$AZ_STDOUT" || probe_json=""
        fi

        if [[ -n "$probe_json" ]]; then
            resolves="true"
            local area
            area="$(item_field_from "$probe_json" 'System.AreaPath')"
            if [[ -n "$CTX_AREA" && "$area" == "$CTX_AREA"* ]]; then in_area="true"; fi
            url="$(organization_url "$CTX_ORG")/$CTX_PROJECT/_workitems/edit/$number"
        fi

        entries+=("$(json_object_of \
            "raw$(printf '\t')$(json_str "$raw")" \
            "id$(printf '\t')$(json_num "$number")" \
            "resolves$(printf '\t')$(json_bool "$resolves")" \
            "in_configured_area$(printf '\t')$(json_bool "$in_area")" \
            "url$(printf '\t')$(json_str "$url")")")
    done

    emit_result "$(json_object_of "references$(printf '\t')$(json_array_of ${entries[@]+"${entries[@]}"})")"
}

# ---------------------------------------------------------------------------
# Argument parsing
# ---------------------------------------------------------------------------

OPERATION=""
ID=""
URL=""
TEXT=""
TYPE=""
TITLE=""
DESCRIPTION_HTML=""
ACCEPTANCE_HTML=""
ARG_ORG=""
ARG_PROJECT=""
ARG_AREA=""
REV=""
CONFIRMED="false"
JSON_REQUESTED="false"
SET_ENTRIES=()
REFERENCES=()
PENDING_FIELD=""
PENDING_FIELD_SEEN="false"

while [[ $# -gt 0 ]]; do
    case "$1" in
        --operation)    OPERATION="${2:-}"; shift 2 ;;
        --id)           ID="${2:-}"; shift 2 ;;
        --url)          URL="${2:-}"; shift 2 ;;
        --field)
            [[ "$PENDING_FIELD_SEEN" == "true" ]] && exit_usage "--field $PENDING_FIELD was given no --value"
            PENDING_FIELD="${2:-}"; PENDING_FIELD_SEEN="true"; shift 2 ;;
        --value)
            [[ "$PENDING_FIELD_SEEN" == "true" ]] || exit_usage '--value must follow a --field'
            SET_ENTRIES+=("$PENDING_FIELD=${2:-}"); PENDING_FIELD=""; PENDING_FIELD_SEEN="false"; shift 2 ;;
        --type)         TYPE="${2:-}"; shift 2 ;;
        --title)        TITLE="${2:-}"; shift 2 ;;
        --description-html)         DESCRIPTION_HTML="${2:-}"; shift 2 ;;
        --acceptance-criteria-html) ACCEPTANCE_HTML="${2:-}"; shift 2 ;;
        --reference)    REFERENCES+=("${2:-}"); shift 2 ;;
        --text)         TEXT="${2:-}"; shift 2 ;;
        --organization) ARG_ORG="${2:-}"; shift 2 ;;
        --project)      ARG_PROJECT="${2:-}"; shift 2 ;;
        --area-path)    ARG_AREA="${2:-}"; shift 2 ;;
        --rev)          REV="${2:-}"; shift 2 ;;
        --fixture)      FIXTURE_PATH="${2:-}"; shift 2 ;;
        --confirmed)    CONFIRMED="true"; shift ;;
        --json)         JSON_REQUESTED="true"; shift ;;
        *)              exit_usage "unknown argument: $1" ;;
    esac
done

[[ "$PENDING_FIELD_SEEN" == "true" ]] && exit_usage "--field $PENDING_FIELD was given no --value"
[[ -z "$OPERATION" ]] && exit_usage 'the --operation argument is required'
[[ "$JSON_REQUESTED" == "true" ]] || exit_usage 'the --json flag is required; the canonical JSON line is the only supported output'

apply_url_argument

init_fixture

case "$OPERATION" in
    check)          op_check ;;
    read)           op_read ;;
    propose-update) op_propose_update ;;
    apply-update)   op_apply_update ;;
    comment)        op_comment ;;
    propose-create) op_propose_create ;;
    apply-create)   op_apply_create ;;
    verify-link)    op_verify_link ;;
    *)              exit_usage "unknown operation: $OPERATION" ;;
esac

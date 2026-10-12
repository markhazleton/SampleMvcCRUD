#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Generate the current BSW.DevSpark knowledge index.

Superset of the legacy flat type-folder model (architecture/, governance/, reference/, ...,
classified by `type:` frontmatter) and the entity-ontology model
(.knowledge/entities/<id>/_entity.yaml + layer docs classified by `layer:` frontmatter).
Both models are additive and may coexist in the same repository per constitution Principle I —
adopting entities/ is an explicit, opt-in migration (see migrate-knowledge-to-entities.py), never
required for an existing repo to keep working.
"""

from __future__ import annotations

import argparse
import ast
import difflib
import fnmatch
import hashlib
import io
import json
import os
import re
import subprocess
import sys
import tempfile
import tokenize
from datetime import UTC, date, datetime
from pathlib import Path, PurePath
from typing import Any

import yaml

# --- Canonical ontology (form vs role) -------------------------------------------------------
# A Knowledge artifact carries two independent classifications. `form` is the structural kind of
# artifact and is derived from where/what the file is -- never configurable. `role` is the kind of
# truth it contains, and is the only axis the taxonomy registry may assign. Collapsing the two
# into a single `type` is what made a valid entity layer permanently unclassifiable: its form is
# `entity-layer` while its role is the layer it documents, and one field cannot hold both.
FORM_FLAT = "flat-document"
FORM_ENTITY_LAYER = "entity-layer"
FORM_GOVERNANCE_DECISION = "governance-decision"
FORMS = {FORM_FLAT, FORM_ENTITY_LAYER, FORM_GOVERNANCE_DECISION}

# A file that indexes a directory is navigation, not a member of it. Recognizing that by filename
# keeps form structural: without it the only way to exempt a decisions/ README is to let
# frontmatter declare its own form, which would let any file misreport what it structurally is.
NAVIGATION_FILENAMES = {"README.md", "index.md"}

# Roles assignable to a flat document. This set is the published taxonomy vocabulary and MUST stay
# identical to taxonomy-registry.schema.json's `nodeType` enum (proven by contract test).
FLAT_ROLES = {
    "authoritative-reference",
    "engineering-pattern",
    "reference-data",
    "operations-runbook",
    "research-or-context",
    "architecture",
    "governance",
}
ALLOWED_LAYERS = {"architecture", "business", "integration", "operations", "pattern", "plan"}
DECISION_ROLE = "governance"
# Each form draws its role from its own domain: a flat doc from the taxonomy vocabulary, an entity
# layer from the layer vocabulary, a decision from governance. No new role names were invented.
FORM_ROLES = {
    FORM_FLAT: FLAT_ROLES,
    FORM_ENTITY_LAYER: ALLOWED_LAYERS,
    FORM_GOVERNANCE_DECISION: {DECISION_ROLE},
}
ALLOWED_ROLES = set().union(*FORM_ROLES.values())

# Controlled entity vocabulary. Semantic classification only: it never mandates that a layer
# exist, because layer coverage is evidence-driven (see AUXILIARY_LAYERS/coverage_overrides).
ENTITY_TYPES = {
    "subsystem": "An internal capability of the system this repository builds.",
    "domain": "A business/problem-space concept whose rules originate outside the code.",
    "integration": "A boundary with an external system, protocol, or third-party service.",
    "tooling": "Developer-facing automation that supports delivery rather than being shipped.",
}

# Compatibility only. 7.7.x consumers read `node["type"]`; it stays in the index and keeps its
# exact historical values (role for flat documents, form otherwise). `form`/`role` are canonical.
ALLOWED_TYPES = FLAT_ROLES | {FORM_ENTITY_LAYER, FORM_GOVERNANCE_DECISION}
# Coverage counts only layers every entity is expected to carry. `pattern` and `plan` are authored
# when they apply, so counting them as gaps makes `fully_covered` unreachable rather than honest.
AUXILIARY_LAYERS = {"pattern", "plan"}
# Discovery projection: a document's own headings and aliases are indexed so a query can find it
# by the concepts it explains, not only by its id/title/path/source mapping. Normalized body text
# is deliberately NOT stored here -- it is rebuilt on demand by the retrieval path, because
# committing it grows the index roughly tenfold and re-churns it on every documentation edit.
MAX_INDEXED_HEADINGS = 100
HEADING_RE = re.compile(r"^\s{0,3}(#{1,6})\s+(.+?)\s*#*\s*$")
FENCE_RE = re.compile(r"^\s{0,3}(`{3,}|~{3,})")
FENCE_LINE_RE = re.compile(r"^\s{0,3}(?:`{3,}|~{3,}).*$", re.MULTILINE)
HTML_COMMENT_RE = re.compile(r"<!--.*?-->", re.DOTALL)
MD_LINK_RE = re.compile(r"!?\[([^\]]*)\]\([^)]*\)")
MD_BULLET_RE = re.compile(r"^\s*(?:[-+*]|\d+\.)\s+", re.MULTILINE)
MD_NOISE_RE = re.compile(r"[*_`>|#~\[\]()]")
WHITESPACE_RE = re.compile(r"\s+")
PROHIBITED_KEYS = {
    "status",
    "lifecycle",
    "supersedes",
    "superseded-by",
    "replaced",
    "obsolete",
}
PROHIBITED_TYPES = {
    "historical-record",
    "stale-reference",
    "decision",
    "rationale",
    "history",
    "feature",
    "requirement",
    "task",
}
# Decisions are keyed by domain/topic and edited in place, never by creation order — a filename
# carrying a sequence number or ADR-style counter is the historical-tracking pattern this model
# explicitly rejects (see .knowledge/tooling/content-lifecycle.md).
DECISION_SEQUENCE_PREFIX_RE = re.compile(r"^(?:\d+[-_]|adr[-_]?\d+|decision[-_]?\d+)", re.IGNORECASE)
SLUG_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
# Current knowledge is durable truth and must never point at ephemeral planning/work-product state
# or the write-only archive — both may contain a path that happens to exist on disk, so an
# existence check alone cannot catch this; these prefixes must be rejected explicitly. This is the
# ephemeral subset of `.devspark.work/` only (per .knowledge/taxonomy-registry.json's
# `workProductType` entries plus the CAP/legacy-release drafts folders) — durable config and
# override subtrees such as `.devspark.work/scripts/`, `.devspark.work/commands/`,
# `.devspark.work/templates/`, and `.devspark.work/autonomy/` are legitimate appliesTo/
# source_of_truth targets and must stay allowed.
FORBIDDEN_REFERENCE_PREFIXES = (
    ".archive/",
    ".devspark.work/specs/",
    ".devspark.work/quickfixes/",
    ".devspark.work/pr-review/",
    ".devspark.work/audit/",
    ".devspark.work/metrics/",
    ".devspark.work/telemetry/",
    ".devspark.work/governance/",
    ".devspark.work/releases/",
)


def _is_forbidden_reference(item: str) -> bool:
    return any(item.startswith(prefix) for prefix in FORBIDDEN_REFERENCE_PREFIXES)


# --- Knowledge drift detection: canonicalization, content addressing, object claims ---------
#
# Additive to the legacy string source_of_truth model above: a source_of_truth item may now be
# either a legacy path string (unchanged existing staleness semantics) or an object claim
# {path, digest, baseline, profile, region} compared against a retained, content-addressed
# baseline under .knowledge/evidence/source-baselines/.

SUPPORTED_COMPARISON_PROFILES = {"python-token-v1", "exact-text-v1", "binary-v1"}
_DIGEST_RE = re.compile(r"^sha256:[0-9a-f]{64}$")

# Token kinds that never indicate a behavioral change under python-token-v1 -- comments and pure
# layout/whitespace tokens. Anything else (including string/number literal changes) is significant.
_PYTHON_TRIVIA_TOKEN_TYPES = frozenset(
    {
        tokenize.COMMENT,
        tokenize.NL,
        tokenize.NEWLINE,
        tokenize.ENCODING,
        tokenize.INDENT,
        tokenize.DEDENT,
        tokenize.ENDMARKER,
    }
)

# Per-run cache: canonicalization is pure given (path content, profile, region) for the duration
# of a single invocation, so repeated claims against the same tuple never re-read or re-tokenize.
# Reset explicitly between test runs / CLI invocations via reset_canonicalization_cache().
_CANONICALIZATION_CACHE: dict[tuple[str, str, str | None], tuple[bytes, bool]] = {}


class CanonicalizationError(Exception):
    """Raised when a comparison profile cannot produce trustworthy canonical bytes for a path.

    Callers must never publish a finding when this is raised -- fail the whole run instead.
    """


class ObjectClaimError(ValueError):
    """Raised when a document's object-form source claim or verification record is malformed."""


def reset_canonicalization_cache() -> None:
    """Clear the per-run canonicalization cache. Call once per CLI invocation and between tests."""
    _CANONICALIZATION_CACHE.clear()


def compute_digest(canonical_bytes: bytes) -> str:
    """Return the `sha256:<64-hex>` content address for already-canonicalized bytes."""
    return "sha256:" + hashlib.sha256(canonical_bytes).hexdigest()


def baseline_address(digest: str) -> str:
    """Return the content-addressed baseline path derived from a `sha256:<64-hex>` digest."""
    if not _DIGEST_RE.match(digest):
        raise CanonicalizationError(f"unsupported or malformed digest '{digest}'")
    hex_digest = digest.split(":", 1)[1]
    return f".knowledge/evidence/source-baselines/sha256/{hex_digest[:2]}/{hex_digest}.baseline"


def _exact_text_bytes(path: Path) -> bytes:
    raw = path.read_bytes()
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as exc:
        raise CanonicalizationError(f"{path}: not valid UTF-8 text for exact-text-v1") from exc
    # Line-ending identity: CRLF/LF-only differences never indicate drift under exact-text-v1.
    return text.replace("\r\n", "\n").replace("\r", "\n").encode("utf-8")


def _binary_bytes(path: Path) -> bytes:
    return path.read_bytes()


def _python_region_source(text: str, region: str | None) -> tuple[str, bool]:
    """Return (source_segment, widened). widened=True means `region` was requested but could not
    be resolved, so the whole file was substituted and callers must emit a widening advisory."""
    if region is None:
        return text, False
    try:
        tree = ast.parse(text)
    except SyntaxError:
        return text, True
    for node in ast.walk(tree):
        if getattr(node, "name", None) == region and isinstance(
            node, ast.FunctionDef | ast.AsyncFunctionDef | ast.ClassDef
        ):
            segment = ast.get_source_segment(text, node)
            if segment is not None:
                return segment, False
    return text, True


def _python_token_canonical_bytes(path: Path, region: str | None) -> tuple[bytes, bool]:
    raw = path.read_bytes()
    try:
        text = raw.decode("utf-8")
    except UnicodeDecodeError as exc:
        raise CanonicalizationError(f"{path}: not valid UTF-8 text for python-token-v1") from exc
    source, widened = _python_region_source(text, region)
    try:
        tokens = list(tokenize.generate_tokens(io.StringIO(source).readline))
    except (tokenize.TokenError, IndentationError, SyntaxError) as exc:
        raise CanonicalizationError(
            f"{path}: python-token-v1 could not tokenize {region or 'the whole file'}"
        ) from exc
    significant = [tok for tok in tokens if tok.type not in _PYTHON_TRIVIA_TOKEN_TYPES]
    canonical_text = "\x1f".join(f"{tok.type}:{tok.string}" for tok in significant)
    return canonical_text.encode("utf-8"), widened


def canonicalize_source(path: Path, profile: str, region: str | None = None) -> tuple[bytes, bool]:
    """Return (canonical_bytes, region_widened) for `path` under the named comparison `profile`.

    Falls back conservatively to exact-text-v1 whenever `python-token-v1` cannot deterministically
    prove a difference non-behavioral (non-Python file, decode failure, or a tokenize/parse
    failure) -- this never silently suppresses an otherwise-detectable difference. An unrecognized
    profile name is a hard `CanonicalizationError`, not a silent fallback.
    """
    cache_key = (str(path), profile, region)
    cached = _CANONICALIZATION_CACHE.get(cache_key)
    if cached is not None:
        return cached

    if profile == "binary-v1":
        result = (_binary_bytes(path), False)
    elif profile == "exact-text-v1":
        result = (_exact_text_bytes(path), False)
    elif profile == "python-token-v1":
        if path.suffix == ".py":
            try:
                result = _python_token_canonical_bytes(path, region)
            except CanonicalizationError:
                result = (_exact_text_bytes(path), False)
        else:
            result = (_exact_text_bytes(path), False)
    else:
        raise CanonicalizationError(f"{path}: unknown comparison profile '{profile}'")

    _CANONICALIZATION_CACHE[cache_key] = result
    return result


def run_self_test(root: Path) -> list[str]:
    """Validate the installed positive, negative, and invalid-input controls."""
    controls_root = root / "templates" / "knowledge-drift" / "self-test"
    manifest_path = controls_root / "claims.yaml"
    if not manifest_path.is_file():
        return [
            "self-test controls are missing: install templates/knowledge-drift/self-test/"
        ]
    try:
        manifest = yaml.safe_load(manifest_path.read_text(encoding="utf-8")) or {}
        controls = manifest["controls"]
    except OSError:
        return [
            "self-test manifest cannot be read; restore templates/knowledge-drift/self-test/ "
            "and retry"
        ]
    except (KeyError, TypeError, yaml.YAMLError):
        return [
            "self-test manifest is malformed; restore templates/knowledge-drift/self-test/ "
            "from the installed framework and retry"
        ]

    errors: list[str] = []
    for control in controls:
        if not isinstance(control, dict):
            errors.append(
                "self-test manifest contains a malformed control; restore the installed "
                "self-test controls and retry"
            )
            continue
        name = control.get("name", "unnamed")
        try:
            source = controls_root / control["source"]
            baseline = controls_root / control["baseline"]
            current, _ = canonicalize_source(source, control["profile"])
            expected, _ = canonicalize_source(baseline, control["profile"])
            actual = "drifted" if current != expected else "unchanged"
            if control.get("expected") == "error":
                errors.append(f"self-test control '{name}' did not reject invalid canonicalization")
                continue
            if actual != control["expected"]:
                errors.append(
                    f"self-test control '{name}' returned {actual}; expected {control['expected']}"
                )
        except subprocess.TimeoutExpired:
            errors.append(
                f"self-test control '{name}' timed out; restore the installed tooling or "
                "retry in a healthy environment"
            )
        except subprocess.SubprocessError:
            errors.append(
                f"self-test control '{name}' tooling failed; restore the installed tooling "
                "or retry in a healthy environment"
            )
        except OSError:
            errors.append(
                f"self-test control '{name}' cannot read its source or baseline; restore the "
                "installed self-test controls and retry"
            )
        except CanonicalizationError:
            if control.get("expected") == "error":
                continue
            errors.append(
                f"self-test control '{name}' cannot canonicalize its input; correct the "
                "comparison profile or restore the installed control and retry"
            )
        except KeyError:
            if control.get("expected") == "error":
                continue
            errors.append(
                f"self-test control '{name}' is malformed; restore the installed self-test "
                "controls and retry"
            )
    return errors


def detection_report(mode: str, status: str, errors: list[str] | None = None) -> dict[str, Any]:
    """Build the versioned fail-closed report envelope without source content."""
    return {
        "schema_version": "1",
        "mode": mode,
        "status": status,
        "summary": {},
        "findings": [],
        "advisories": [],
        "errors": errors or [],
    }


def write_report_atomically(path: Path, payload: dict[str, Any]) -> None:
    """Replace a machine report only after the complete payload is ready."""
    path.parent.mkdir(parents=True, exist_ok=True)
    handle, temporary_name = tempfile.mkstemp(prefix=f".{path.name}.", dir=path.parent)
    try:
        with os.fdopen(handle, "w", encoding="utf-8") as stream:
            json.dump(payload, stream, indent=2, ensure_ascii=False)
            stream.write("\n")
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary_name, path)
    except OSError:
        try:
            os.unlink(temporary_name)
        except OSError:
            pass
        raise


def validate_source_claims(path: Path, metadata: dict[str, Any]) -> None:
    """Validate additive object-form source_of_truth claims and the verification record.

    Purely additive: legacy string items are untouched and remain valid on their own. Raises
    `ObjectClaimError` (a `ValueError`) with an actionable message; never silently drops, coerces,
    or reinterprets a malformed claim.
    """
    items = metadata.get("source_of_truth") or []
    seen_tuples: set[tuple[str, str, str | None]] = set()
    for item in items:
        if isinstance(item, str):
            continue
        if not isinstance(item, dict):
            raise ObjectClaimError(
                f"{path}: source_of_truth item must be a string or object, got {type(item).__name__}"
            )
        claim_path = item.get("path")
        digest = item.get("digest")
        baseline = item.get("baseline")
        profile = item.get("profile")
        region = item.get("region")
        for field_name, value in (
            ("path", claim_path),
            ("digest", digest),
            ("baseline", baseline),
            ("profile", profile),
        ):
            if not isinstance(value, str) or not value:
                raise ObjectClaimError(f"{path}: object source claim missing required field '{field_name}'")
        if _is_forbidden_reference(claim_path):
            raise ObjectClaimError(f"{path}: source claim path '{claim_path}' targets a planning/archive location")
        if not _DIGEST_RE.match(digest):
            raise ObjectClaimError(f"{path}: source claim digest '{digest}' is not a valid sha256:<64-hex> address")
        if baseline_address(digest) != baseline:
            raise ObjectClaimError(f"{path}: source claim baseline '{baseline}' does not match digest '{digest}'")
        tuple_key = (claim_path, profile, region)
        if tuple_key in seen_tuples:
            raise ObjectClaimError(f"{path}: duplicate source claim (path, profile, region) = {tuple_key}")
        seen_tuples.add(tuple_key)

    verification = metadata.get("verification")
    if verification is not None:
        state = verification.get("state")
        confirmed_on = verification.get("confirmed_on")
        if state == "verified" and not confirmed_on:
            raise ObjectClaimError(f"{path}: verification.state 'verified' requires 'confirmed_on'")
        if state == "pinned-unverified" and confirmed_on:
            raise ObjectClaimError(f"{path}: verification.state 'pinned-unverified' must not set 'confirmed_on'")


DEFAULT_MAPPINGS = (
    (".knowledge/reference/**", "reference-data"),
    (".knowledge/reference-data/**", "reference-data"),
    (".knowledge/integrations/**", "operations-runbook"),
    (".knowledge/operations/**", "operations-runbook"),
    (".knowledge/scripts/**", "operations-runbook"),
    (".knowledge/legal/**", "authoritative-reference"),
    (".knowledge/plans/**", "research-or-context"),
    (".knowledge/architecture/**", "architecture"),
    (".knowledge/governance/**", "governance"),
)
ROOTS = (
    {"path": ".devspark/", "owner": "framework", "purpose": "framework payload"},
    {"path": ".devspark.work/", "owner": "repo", "purpose": "work products"},
    {"path": ".knowledge/", "owner": "repo", "purpose": "current authoritative knowledge and published user-facing guidance"},
    {"path": ".archive/", "owner": "repo", "purpose": "landing place for old work products, grouped by release"},
)


def parse_frontmatter(path: Path) -> tuple[dict[str, Any], str]:
    text = path.read_text(encoding="utf-8-sig")
    if not text.startswith("---\n") and not text.startswith("---\r\n"):
        return {}, text
    normalized = text.replace("\r\n", "\n")
    parts = normalized.split("---\n", 2)
    if len(parts) != 3:
        raise ValueError(f"{path}: malformed YAML frontmatter")
    metadata = yaml.safe_load(parts[1]) or {}
    if not isinstance(metadata, dict):
        raise ValueError(f"{path}: frontmatter must be an object")
    return metadata, parts[2]


def load_registry(root: Path) -> list[dict[str, Any]]:
    path = root / ".knowledge/taxonomy-registry.json"
    if not path.exists():
        return []
    registry = json.loads(path.read_text(encoding="utf-8"))
    mappings = registry.get("mappings")
    if not isinstance(mappings, list):
        raise ValueError(f"{path}: mappings must be an array")
    return [mapping for mapping in mappings if isinstance(mapping, dict)]


def infer_type(relative_path: str, mappings: list[dict[str, Any]]) -> str:
    """Compatibility wrapper: the taxonomy assigns a *role*, which for a flat document is also its
    historical `type`."""
    return infer_role(relative_path, mappings)


GLOB_CHARS_RE = re.compile(r"[*?\[]")
DEFAULT_ROLE = "authoritative-reference"


def _pattern_specificity(pattern: str) -> tuple[int, int, int]:
    """Rank a path pattern so a more specific mapping always wins regardless of declaration order.

    Ordered by: exact path beats any glob, then more literal segments, then longer pattern. This
    is what makes `.knowledge/governance/decisions/**` beat `.knowledge/governance/**` without
    depending on which list happened to be scanned first.
    """
    segments = pattern.split("/")
    literal_segments = sum(1 for segment in segments if not GLOB_CHARS_RE.search(segment))
    return (0 if GLOB_CHARS_RE.search(pattern) else 1, literal_segments, len(pattern))


def resolve_role(relative_path: str, mappings: list[dict[str, Any]]) -> tuple[str, str | None]:
    """Resolve a flat document's semantic role from the taxonomy, most-specific mapping first.

    Returns the role and the pattern that produced it (`None` when nothing matched and the default
    applies). The pattern matters to callers that need to know *how firmly* the taxonomy spoke: a
    broad glob is a default for a directory, while an exact path is a statement about one file.

    Registry mappings outrank built-in defaults only as a tiebreak at equal specificity, so an
    explicit repository override still works while a broad override can no longer shadow a more
    specific rule. Two mappings of the same origin, same specificity and different roles are
    genuinely ambiguous and fail loudly rather than resolving by list position.
    """
    candidates: list[tuple[tuple[int, int, int], int, str, str]] = []
    for mapping in mappings:
        pattern = mapping.get("pathPattern")
        role = mapping.get("nodeType")
        if pattern and role and fnmatch.fnmatchcase(relative_path, str(pattern)):
            candidates.append((_pattern_specificity(str(pattern)), 1, str(role), str(pattern)))
    for pattern, role in DEFAULT_MAPPINGS:
        if fnmatch.fnmatchcase(relative_path, pattern):
            candidates.append((_pattern_specificity(pattern), 0, role, pattern))
    if not candidates:
        return DEFAULT_ROLE, None

    best = max(candidates, key=lambda item: (item[0], item[1]))
    ambiguous = {
        (item[2], item[3]) for item in candidates if item[0] == best[0] and item[1] == best[1]
    }
    if len({role for role, _pattern in ambiguous}) > 1:
        detail = ", ".join(f"{pattern} -> {role}" for role, pattern in sorted(ambiguous))
        raise ValueError(
            f"{relative_path}: taxonomy mappings are ambiguous at equal specificity ({detail}); "
            f"make one pattern more specific or remove the conflict"
        )
    return best[2], best[3]


def infer_role(relative_path: str, mappings: list[dict[str, Any]]) -> str:
    return resolve_role(relative_path, mappings)[0]


def infer_form(relative_path: str) -> str:
    """Structural kind of a Knowledge artifact, derived from its location and filename.

    Never registry-assigned and never author-assigned: form is a fact about where the file sits,
    not an editorial choice.
    """
    if relative_path.startswith(".knowledge/entities/"):
        return FORM_ENTITY_LAYER
    filename = relative_path.rsplit("/", 1)[-1]
    if (
        relative_path.startswith(".knowledge/governance/decisions/")
        and filename not in NAVIGATION_FILENAMES
    ):
        return FORM_GOVERNANCE_DECISION
    return FORM_FLAT


def compat_type(form: str, role: str) -> str:
    """Historical single-field `type` 7.7.x consumers still read: the role for a flat document,
    the form otherwise. Preserves every pre-7.8 value exactly."""
    return role if form == FORM_FLAT else form


def validate_current(path: Path, metadata: dict[str, Any], body: str) -> None:
    invalid_keys = sorted(PROHIBITED_KEYS.intersection(metadata))
    if invalid_keys:
        raise ValueError(f"{path}: current knowledge prohibits {', '.join(invalid_keys)}")
    node_type = str(metadata.get("type", ""))
    if node_type in PROHIBITED_TYPES or (node_type and node_type not in ALLOWED_TYPES):
        raise ValueError(f"{path}: prohibited knowledge type '{node_type}'")
    links = metadata.get("links", {})
    if links and not isinstance(links, dict):
        raise ValueError(f"{path}: links must be an object")
    if isinstance(links, dict) and set(links).difference({"references"}):
        raise ValueError(f"{path}: only current 'references' links are allowed")
    # No body-text scan: lifecycle language is prohibited in frontmatter (above), not in prose —
    # docs legitimately describe current system behavior using these words (e.g. a live Cosmos
    # "status": "superseded" field value, or a still-present-but-unread legacy field).


def extract_headings(body: str) -> list[str]:
    """ATX headings outside fenced code blocks, in document order, case-insensitively deduped."""
    headings: list[str] = []
    seen: set[str] = set()
    fence: str | None = None
    for line in body.splitlines():
        fence_match = FENCE_RE.match(line)
        if fence_match:
            marker = fence_match.group(1)[0]
            if fence is None:
                fence = marker
            elif fence == marker:
                fence = None
            continue
        if fence is not None:
            continue
        match = HEADING_RE.match(line)
        if not match:
            continue
        text = match.group(2).strip()
        key = text.lower()
        if not text or key in seen:
            continue
        seen.add(key)
        headings.append(text)
        if len(headings) >= MAX_INDEXED_HEADINGS:
            break
    return headings


def normalize_search_text(body: str) -> str:
    """Lowercased, markdown-stripped body text for deterministic content retrieval.

    Derived data only -- the Markdown file stays authoritative. Frontmatter is never included:
    callers pass the body `parse_frontmatter` returned.
    """
    text = HTML_COMMENT_RE.sub(" ", body)
    text = MD_LINK_RE.sub(r"\1 ", text)
    text = FENCE_LINE_RE.sub(" ", text)
    text = MD_BULLET_RE.sub(" ", text)
    text = MD_NOISE_RE.sub(" ", text)
    return WHITESPACE_RE.sub(" ", text).strip().lower()


def validate_aliases(path: Path, metadata: dict[str, Any]) -> list[str]:
    """Optional discovery-only vocabulary. Never taxonomy, identity, or behavioral truth."""
    raw = metadata.get("aliases")
    if raw is None:
        return []
    if not isinstance(raw, list) or not all(isinstance(item, str) for item in raw):
        raise ValueError(f"{path}: aliases must be an array of strings")
    aliases: list[str] = []
    seen: set[str] = set()
    for item in raw:
        alias = item.strip()
        if not alias:
            raise ValueError(f"{path}: aliases entries must be non-empty")
        if alias.lower() in seen:
            continue
        seen.add(alias.lower())
        aliases.append(alias)
    return aliases


def search_fields(path: Path, metadata: dict[str, Any], body: str) -> dict[str, Any]:
    """Additive discovery projection of a knowledge document.

    Nothing here changes what a document *is* -- only how a query can reach it.
    """
    fields: dict[str, Any] = {}
    aliases = validate_aliases(path, metadata)
    if aliases:
        fields["aliases"] = aliases
    headings = extract_headings(body)
    if headings:
        fields["headings"] = headings
    return fields


def _git_last_commit_date(root: Path, relative_path: str) -> str | None:
    try:
        result = subprocess.run(
            ["git", "log", "-1", "--format=%cI", "--", relative_path],
            cwd=root,
            capture_output=True,
            text=True,
            timeout=5,
            check=False,
        )
    except (OSError, subprocess.SubprocessError):
        return None
    line = result.stdout.strip()
    return line or None


def _memoized_commit_date(
    root: Path, relative_path: str, history: dict[tuple[str, str], str | None] | None
) -> str | None:
    if history is None:
        return _git_last_commit_date(root, relative_path)
    key = (root.as_posix(), relative_path)
    if key not in history:
        history[key] = _git_last_commit_date(root, relative_path)
    return history[key]


def validate_entity_layer_doc(
    root: Path,
    path: Path,
    metadata: dict[str, Any],
    stale_findings: list[dict[str, str]],
    history: dict[tuple[str, str], str | None] | None = None,
) -> None:
    """Validate an entity-layer doc's structure (raises immediately) and check staleness
    (appended to `stale_findings` instead of raising, so every stale doc in a run can be
    reported together rather than stopping at the first one). Staleness is reported, not
    hard-failed by default -- see the `stale_entities` field on the built index and the
    `--fail-on-stale` flag in `main()`.

    `history` memoizes source commit dates for one build only, so a source cited by several layer
    documents is queried once. The caller owns and discards it; nothing persists across builds.
    """
    layer = metadata.get("layer")
    if layer not in ALLOWED_LAYERS:
        raise ValueError(f"{path}: layer must be one of {sorted(ALLOWED_LAYERS)}")
    # Graph topology belongs to the entity, not its facets. Accepting `links` here produced an
    # edge-less field that looked authored but never reached the graph; refuse it instead.
    if metadata.get("links"):
        raise ValueError(
            f"{path}: entity-layer docs cannot declare 'links' — express relationships as "
            f"`relations` in the entity's _entity.yaml"
        )
    sources = metadata.get("source_of_truth")
    if not isinstance(sources, list) or not sources or not all(
        isinstance(s, str) or isinstance(s, dict) for s in sources
    ):
        raise ValueError(f"{path}: source_of_truth must be a non-empty array of paths or object claims")
    validate_source_claims(path, metadata)
    last_verified = metadata.get("last_verified")
    if not last_verified:
        raise ValueError(f"{path}: last_verified is required for entity-layer docs")
    try:
        verified_date = date.fromisoformat(str(last_verified))
    except ValueError as exc:
        raise ValueError(f"{path}: last_verified must be an ISO date (YYYY-MM-DD)") from exc
    for source in sources:
        if isinstance(source, dict):
            # Object claims are verified by content digest, not commit-date staleness -- see
            # validate_source_claims above for shape/tuple-uniqueness validation.
            source_path = source["path"]
            if _is_forbidden_reference(source_path):
                raise ValueError(f"{path}: source_of_truth cannot target .devspark.work/ or .archive/: {source_path}")
            if not (root / source_path).exists():
                raise ValueError(f"{path}: source_of_truth path does not exist: {source_path}")
            continue
        if _is_forbidden_reference(source):
            raise ValueError(f"{path}: source_of_truth cannot target .devspark.work/ or .archive/: {source}")
        if not (root / source).exists():
            raise ValueError(f"{path}: source_of_truth path does not exist: {source}")
        commit_date = _memoized_commit_date(root, source, history)
        if commit_date is None:
            continue
        try:
            source_date = datetime.fromisoformat(commit_date).date()
        except ValueError:
            continue
        if source_date > verified_date:
            stale_findings.append(
                {
                    "path": path.relative_to(root).as_posix(),
                    "source": source,
                    "source_date": str(source_date),
                    "last_verified": str(verified_date),
                }
            )


def load_entity_node(path: Path) -> dict[str, Any]:
    data = yaml.safe_load(path.read_text(encoding="utf-8")) or {}
    if not isinstance(data, dict):
        raise ValueError(f"{path}: _entity.yaml must be an object")
    for key in ("id", "type", "owner"):
        if not data.get(key):
            raise ValueError(f"{path}: _entity.yaml missing required '{key}'")
    entity_id = str(data["id"])
    if entity_id != path.parent.name:
        raise ValueError(f"{path}: id '{entity_id}' must match folder name '{path.parent.name}'")
    entity_type = str(data["type"])
    if entity_type not in ENTITY_TYPES:
        raise ValueError(
            f"{path}: type '{entity_type}' is not a known entity type; expected one of "
            f"{sorted(ENTITY_TYPES)}"
        )
    return data


def validate_decision_doc(path: Path, metadata: dict[str, Any], entity_ids: set[str]) -> list[str]:
    """Validate a governance-decision doc; returns its `constrains` entity id list.

    Decisions are current governance, not history: keyed by topic (filename), not creation order,
    and every entity they constrain must exist and carry a reciprocal `constrained_by` pointer
    (checked by the caller once all entities and decisions are known).
    """
    if DECISION_SEQUENCE_PREFIX_RE.match(path.stem):
        raise ValueError(
            f"{path}: decisions must be keyed by domain/topic, not creation order — rename away "
            "from a numbered/ADR-style filename"
        )
    constrains = metadata.get("constrains")
    if not isinstance(constrains, list) or not constrains or not all(isinstance(c, str) for c in constrains):
        raise ValueError(f"{path}: governance-decision requires a non-empty 'constrains' array of entity ids")
    for entity_id in constrains:
        if entity_id not in entity_ids:
            raise ValueError(f"{path}: constrains references unknown entity '{entity_id}'")
    return [str(c) for c in constrains]


def stable_path_sort_key(path: PurePath) -> str:
    """Return an OS-independent, case-sensitive sort key for a repository path.

    `Path`/`PurePath` comparison is platform-flavor-dependent -- `PureWindowsPath` normalizes
    case for ordering, `PurePosixPath` does not -- so sorting `Path` objects directly (rather
    than this key) produces a different, non-deterministic node/edge order in generated
    `.knowledge` artifacts depending on which OS ran the generator, causing false drift failures
    when one OS's artifact is checked against another's. This key compares plain, slash-
    normalized strings instead, which sort identically on every supported OS. Deliberately not
    lowercased: DevSpark preserves repository path identity rather than imposing Windows
    case-folding semantics everywhere.
    """
    return path.as_posix()


def build_entities(
    root: Path,
    history: dict[tuple[str, str], str | None] | None = None,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, str]]]:
    """Return (entity_nodes, layer_doc_nodes, stale_entities) for every .knowledge/entities/<id>/.

    `stale_entities` is reported, never raised here -- a source file's commit date moving past
    a doc's `last_verified` is common noise (an unrelated edit to a cited file), not proof the
    described behavior actually changed. The caller decides whether to treat it as blocking via
    `--fail-on-stale`; the routine build always succeeds so it never blocks unrelated work.
    """
    entities_root = root / ".knowledge/entities"
    entity_nodes: list[dict[str, Any]] = []
    layer_nodes: list[dict[str, Any]] = []
    stale_findings: list[dict[str, str]] = []
    if not entities_root.exists():
        return entity_nodes, layer_nodes, stale_findings
    for entity_dir in sorted((p for p in entities_root.iterdir() if p.is_dir()), key=stable_path_sort_key):
        node_file = entity_dir / "_entity.yaml"
        if not node_file.exists():
            raise ValueError(f"{entity_dir}: missing required _entity.yaml")
        entity = load_entity_node(node_file)
        entity_id = str(entity["id"])
        overrides = {
            str(item["layer"])
            for item in entity.get("coverage_overrides", []) or []
            if isinstance(item, dict) and item.get("layer")
        }
        present_layers: set[str] = set()

        for doc_path in sorted(entity_dir.glob("*.md"), key=stable_path_sort_key):
            relative = doc_path.relative_to(root).as_posix()
            metadata, body = parse_frontmatter(doc_path)
            validate_current(doc_path, metadata, body)
            validate_entity_layer_doc(root, doc_path, metadata, stale_findings, history)
            layer = str(metadata["layer"])
            present_layers.add(layer)
            node_id = str(metadata.get("id") or f"{entity_id}-{layer}")
            layer_nodes.append(
                {
                    "id": node_id,
                    "entity": entity_id,
                    "layer": layer,
                    "type": FORM_ENTITY_LAYER,
                    "form": FORM_ENTITY_LAYER,
                    "role": layer,
                    "path": relative,
                    "title": str(metadata.get("title") or node_id),
                    "source_of_truth": metadata["source_of_truth"],
                    "last_verified": str(metadata["last_verified"]),
                    **search_fields(doc_path, metadata, body),
                }
            )

        missing_layers = sorted(ALLOWED_LAYERS - AUXILIARY_LAYERS - present_layers - overrides)
        entity_nodes.append(
            {
                "id": entity_id,
                "type": str(entity["type"]),
                "owner": str(entity["owner"]),
                "path": entity_dir.relative_to(root).as_posix(),
                "relations": entity.get("relations", []) or [],
                "layers": sorted(present_layers),
                "missing_layers": missing_layers,
                "coverage_overrides": sorted(overrides),
                "constrained_by": [str(d) for d in entity.get("constrained_by", []) or []],
            }
        )

    return entity_nodes, layer_nodes, stale_findings


def relation_target(relation: Any) -> str:
    """Entity id a relation points at, accepting either authored spelling.

    `object:`/`predicate:` and `to:`/`rel:` are both in the wild; rejecting either would strand
    an existing corpus on an older generator.
    """
    if not isinstance(relation, dict):
        return ""
    return str(relation.get("object") or relation.get("to") or "")


def relation_predicate(relation: dict[str, Any]) -> str:
    return str(relation.get("predicate") or relation.get("rel") or "relates-to")


def generated_at(explicit: str | None = None) -> str:
    override = explicit or os.environ.get("DEVSPARK_GENERATED_AT")
    if override:
        return override
    return datetime.now(UTC).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def knowledge_markdown_paths(root: Path) -> list[Path]:
    """Every Markdown file under `.knowledge/`, in the order the builder reads them."""
    knowledge_root = root / ".knowledge"
    if not knowledge_root.exists():
        return []
    return sorted(knowledge_root.rglob("*.md"), key=stable_path_sort_key)


def build_index(root: Path, timestamp: str | None = None) -> dict[str, Any]:
    knowledge_root = root / ".knowledge"
    mappings = load_registry(root)
    nodes: list[dict[str, Any]] = []
    edges: list[dict[str, str]] = []
    seen: set[str] = set()
    # Source commit dates, memoized for this build only and discarded with it.
    history: dict[tuple[str, str], str | None] = {}

    # Entities are built first so governance-decision docs can validate `constrains` against real
    # entity ids, and so the reciprocal `constrained_by` check below has entities to compare against.
    entity_nodes, layer_nodes, stale_entities = build_entities(root, history)
    entity_by_id = {entity["id"]: entity for entity in entity_nodes}
    decision_constrains: dict[str, list[str]] = {}

    if knowledge_root.exists():
        for path in knowledge_markdown_paths(root):
            relative = path.relative_to(root).as_posix()
            if relative in {".knowledge/index.md", ".knowledge/README.md"}:
                continue
            if ".knowledge/entities/" in relative:
                continue  # handled by build_entities
            metadata, body = parse_frontmatter(path)
            validate_current(path, metadata, body)
            node_id = str(metadata.get("id") or path.relative_to(knowledge_root).with_suffix("").as_posix().replace("/", "-"))
            if node_id in seen:
                raise ValueError(f"{path}: duplicate knowledge id '{node_id}'")
            seen.add(node_id)
            declared = str(metadata.get("type") or "")
            form = infer_form(relative)
            # `type` may restate the form but never choose it. An author may say what kind of truth
            # a document carries; whether it *is* an entity layer is a fact about where it lives.
            if declared in FORMS and declared != form:
                raise ValueError(
                    f"{path}: declares form '{declared}' but its location makes it '{form}'; "
                    f"form is structural and cannot be set in frontmatter"
                )
            if form == FORM_GOVERNANCE_DECISION:
                role = DECISION_ROLE
            else:
                role = (declared if declared not in FORMS else "") or infer_role(relative, mappings)
                if role not in FLAT_ROLES:
                    raise ValueError(
                        f"{path}: '{role}' is not a document role; expected one of {sorted(FLAT_ROLES)}"
                    )
            node_type = compat_type(form, role)
            title = str(metadata.get("title") or next((line[2:].strip() for line in body.splitlines() if line.startswith("# ")), node_id))
            node: dict[str, Any] = {
                "id": node_id,
                "type": node_type,
                "form": form,
                "role": role,
                "path": relative,
                "title": title,
            }
            applies_to = metadata.get("appliesTo")
            if applies_to:
                if not isinstance(applies_to, list) or not all(isinstance(item, str) for item in applies_to):
                    raise ValueError(f"{path}: appliesTo must be an array of paths")
                if any(_is_forbidden_reference(item) for item in applies_to):
                    raise ValueError(f"{path}: appliesTo cannot target .devspark.work/ or .archive/ paths")
                node["appliesTo"] = applies_to
            if form == FORM_GOVERNANCE_DECISION:
                decision_constrains[node_id] = validate_decision_doc(path, metadata, set(entity_by_id))
            node.update(search_fields(path, metadata, body))
            nodes.append(node)
            for target in (metadata.get("links") or {}).get("references", []):
                edges.append({"from": node_id, "to": str(target), "rel": "references"})

    for layer_node in layer_nodes:
        if layer_node["id"] in seen:
            raise ValueError(f"{layer_node['path']}: duplicate knowledge id '{layer_node['id']}'")
        seen.add(layer_node["id"])
    nodes.extend(layer_nodes)

    # Composition, not an authored architectural relationship. An entity *is* its layers, so this
    # adjacency is inherent in the entity model and is materialized here rather than being
    # something an author has to restate in `relations[]`.
    for layer_node in layer_nodes:
        edges.append(
            {"from": str(layer_node["entity"]), "to": str(layer_node["id"]), "rel": "has-layer"}
        )

    # An unresolved relation is a dead edge in the graph every command traverses, and the entity
    # schema requires a resolvable entity id, so it fails the build. A `links.references` target
    # may legitimately point at knowledge not yet authored, so it is reported rather than fatal.
    known_ids = seen | set(entity_by_id)
    path_by_id = {node["id"]: node["path"] for node in nodes}
    for entity in entity_nodes:
        for relation in entity["relations"]:
            target = relation_target(relation)
            if not target:
                raise ValueError(
                    f".knowledge/entities/{entity['id']}/_entity.yaml: every relation requires an "
                    f"'object' (or 'to') entity id"
                )
            if target not in entity_by_id:
                raise ValueError(
                    f".knowledge/entities/{entity['id']}/_entity.yaml: relation targets unknown entity '{target}'"
                )
            edges.append(
                {"from": entity["id"], "to": target, "rel": relation_predicate(relation)}
            )
    dangling_references = [
        {"from": edge["from"], "to": edge["to"], "path": path_by_id.get(edge["from"], edge["from"])}
        for edge in edges
        if edge["to"] not in known_ids
    ]

    # Reciprocity: an entity an existing decision constrains must point back via `constrained_by`,
    # and an entity must not claim `constrained_by` for a decision that doesn't actually constrain it.
    for decision_id, constrains in decision_constrains.items():
        for entity_id in constrains:
            if decision_id not in entity_by_id[entity_id]["constrained_by"]:
                raise ValueError(
                    f".knowledge/entities/{entity_id}/_entity.yaml: missing 'constrained_by: [{decision_id}]' "
                    f"required by governance decision '{decision_id}'"
                )
    for entity_id, entity in entity_by_id.items():
        for decision_id in entity["constrained_by"]:
            if entity_id not in decision_constrains.get(decision_id, []):
                raise ValueError(
                    f".knowledge/entities/{entity_id}/_entity.yaml: constrained_by references "
                    f"'{decision_id}', which does not list '{entity_id}' in its constrains"
                )
            edges.append({"from": decision_id, "to": entity_id, "rel": "constrains"})

    return {
        "generated": {"by": "build-knowledge-index", "at": generated_at(timestamp)},
        "roots": list(ROOTS),
        "nodes": nodes,
        "edges": sorted(edges, key=lambda edge: (edge["from"], edge["to"])),
        "dangling_references": sorted(
            dangling_references, key=lambda item: (item["from"], item["to"])
        ),
        "entities": entity_nodes,
        "stale_entities": stale_entities,
    }


def build_coverage_report(index: dict[str, Any], timestamp: str | None = None) -> dict[str, Any]:
    entities = index.get("entities", [])
    gaps = [
        {"entity": entity["id"], "missing_layers": entity["missing_layers"]}
        for entity in entities
        if entity["missing_layers"]
    ]
    return {
        "generated": {"by": "build-knowledge-index", "at": generated_at(timestamp)},
        "entity_count": len(entities),
        "fully_covered": len(entities) - len(gaps),
        "gaps": gaps,
    }


def _drift(existing_path: Path, fresh_payload: dict[str, Any]) -> str | None:
    """Compare fresh output to the committed file, ignoring the generated.at timestamp."""
    if not existing_path.exists():
        return f"{existing_path} does not exist (run the generator to create it)"
    try:
        existing_payload = json.loads(existing_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        return f"{existing_path} is not valid JSON: {exc}"

    def _normalize(payload: dict[str, Any]) -> dict[str, Any]:
        return {**payload, "generated": {"by": payload.get("generated", {}).get("by")}}

    if _normalize(existing_payload) != _normalize(fresh_payload):
        return f"{existing_path} is stale relative to .knowledge/ source content"
    return None


class InputsUncertain(RuntimeError):
    """A build dependency could not be fingerprinted with confidence, so reuse is not safe."""


def _tree_or_file_digest(path: Path) -> str:
    try:
        if path.is_dir():
            digest = hashlib.sha256()
            for member in sorted((p for p in path.rglob("*") if p.is_file()), key=stable_path_sort_key):
                digest.update(member.relative_to(path).as_posix().encode("utf-8"))
                digest.update(hashlib.sha256(member.read_bytes()).digest())
            return "sha256:" + digest.hexdigest()
        return "sha256:" + hashlib.sha256(path.read_bytes()).hexdigest()
    except FileNotFoundError:
        return "missing"
    except OSError as exc:
        raise InputsUncertain(f"{path}: {exc}") from exc


def _git_head(root: Path) -> str:
    try:
        done = subprocess.run(
            ["git", "rev-parse", "HEAD"], cwd=root, capture_output=True, text=True, timeout=5, check=False
        )
    except (OSError, subprocess.SubprocessError) as exc:
        raise InputsUncertain(f"git history identity unavailable: {exc}") from exc
    return done.stdout.strip() if done.returncode == 0 and done.stdout.strip() else "unavailable"


def knowledge_base_inventory(root: Path) -> dict[str, str]:
    """Content digests of what `build_index` reads, to prove a build is still current.

    Digests are of content, never timestamps: an edit that preserves the modification time still
    changes a digest, and an added or deleted file changes the key set. It covers Knowledge
    documents, entity definitions, the registry, the committed generated artifacts that
    `INDEX_FRESH` compares against, and Git history identity. The inventory is returned to the
    caller and never stored.
    """
    inventory: dict[str, str] = {}
    for path in knowledge_markdown_paths(root):
        inventory[path.relative_to(root).as_posix()] = _tree_or_file_digest(path)
    entities_root = root / ".knowledge/entities"
    if entities_root.is_dir():
        for definition in sorted(entities_root.glob("*/_entity.yaml"), key=stable_path_sort_key):
            inventory[definition.relative_to(root).as_posix()] = _tree_or_file_digest(definition)
    for generated in (".knowledge/taxonomy-registry.json", ".knowledge/index.json", ".knowledge/ontology/coverage.json"):
        inventory[generated] = _tree_or_file_digest(root / generated)
    inventory["git:HEAD"] = _git_head(root)
    return inventory


def knowledge_source_inventory(root: Path) -> dict[str, str]:
    """Content digests of the source files Knowledge documents cite.

    Read from the same documents `build_index` reads (via `knowledge_markdown_paths` and
    `parse_frontmatter`), so it can be taken before a build and compared after it.
    """
    inventory: dict[str, str] = {}
    for path in knowledge_markdown_paths(root):
        try:
            metadata, _ = parse_frontmatter(path)
        except (OSError, ValueError) as exc:
            raise InputsUncertain(f"{path}: {exc}") from exc
        for claim in metadata.get("source_of_truth") or []:
            source = str(claim.get("path", "")) if isinstance(claim, dict) else str(claim)
            if source and not re.search(r"[*?\[]", source):
                inventory[source] = _tree_or_file_digest(root / source)
    return inventory


def inventory_changes(before: dict[str, str], after: dict[str, str]) -> list[str]:
    """Sorted keys that differ, were added, or were deleted between two inventories."""
    return sorted(key for key in set(before) | set(after) if before.get(key) != after.get(key))


def _pattern_covers(pattern: str, changed_file: str) -> bool:
    if fnmatch.fnmatchcase(changed_file, pattern):
        return True
    # A directory named as source_of_truth claims the files beneath it.
    return changed_file.startswith(pattern.rstrip("/") + "/")


def find_unmapped_files(index: dict[str, Any], changed_files: list[str]) -> list[str]:
    """Return the subset of changed_files that no knowledge node claims.

    Used by /devspark-pr-review as a computed hint for its PRD5 ("new behavior with no
    knowledge") judgment call — never a mechanical gate on its own, just a real list backing
    the reviewer's read of the diff instead of leaving it to eyeballing.

    Entity layer docs claim code through `source_of_truth` rather than `appliesTo`, so reading
    only the latter reports entity-covered files as unmapped.
    """
    patterns = [
        pattern
        for node in index.get("nodes", [])
        for key in ("appliesTo", "source_of_truth")
        for pattern in node.get(key) or []
        if isinstance(pattern, str)  # object-form source claims are matched by drift detection, not glob
    ]
    return [
        changed_file
        for changed_file in changed_files
        if not any(_pattern_covers(pattern, changed_file) for pattern in patterns)
    ]


def match_file_owners(index: dict[str, Any], files: list[str]) -> dict[str, Any]:
    """Typed Knowledge lookup for each file: which nodes apply to it or cite it as a source.

    `appliesTo` says a node governs a path; `source_of_truth` says the path is evidence for the
    node. Applicability is not exclusive ownership, so every match is reported with its
    relationship and the pattern that matched, never collapsed to a single owner. Object-form
    source claims match through their `path`. A file no node claims is `unmapped`, a computed hint
    and not a defect.
    """
    results = []
    unmapped = []
    for changed_file in files:
        owners: dict[tuple[str, str, str], dict[str, Any]] = {}
        for node in index.get("nodes", []):
            for relationship in ("appliesTo", "source_of_truth"):
                for claim in node.get(relationship) or []:
                    pattern = str(claim.get("path", "")) if isinstance(claim, dict) else str(claim)
                    if not pattern or not _pattern_covers(pattern, changed_file):
                        continue
                    owners[(str(node["id"]), relationship, pattern)] = {
                        "id": node["id"],
                        "path": node.get("path"),
                        "form": node.get("form"),
                        "entity": node.get("entity"),
                        "relationship": relationship,
                        "pattern": pattern,
                    }
        ordered = [owners[key] for key in sorted(owners)]
        results.append({"path": changed_file, "owners": ordered})
        if not ordered:
            unmapped.append(changed_file)
    return {"files": results, "unmapped_files": unmapped}


# --- Knowledge drift detection: document scan, scope resolution, evaluation, migration ------


class EnforcementConfigError(ValueError):
    """Raised when `.devspark.work/devspark.json`'s `knowledge_drift.enforcement` is malformed."""


class RoutineScopeError(ValueError):
    """Raised when `--base`/`--head` cannot be resolved into a complete, validated change set."""


def _load_repo_config(root: Path) -> dict[str, Any]:
    path = root / ".devspark.work" / "devspark.json"
    if not path.is_file():
        return {}
    try:
        config = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise EnforcementConfigError(f"{path}: cannot read repository configuration ({exc})") from exc
    return config if isinstance(config, dict) else {}


def enforcement_mode(root: Path) -> str | None:
    """Return `"pinned-claims"` when explicitly opted in, or `None` when the key is absent.

    A present key with any other value is a hard configuration error -- never treated as opt-out.
    """
    config = _load_repo_config(root)
    section = config.get("knowledge_drift")
    if section is None:
        return None
    if not isinstance(section, dict) or "enforcement" not in section:
        return None
    value = section["enforcement"]
    if value == "pinned-claims":
        return "pinned-claims"
    raise EnforcementConfigError(
        f"knowledge_drift.enforcement is set to {value!r}; the only valid value is "
        "'pinned-claims'. Remove the key to keep legacy behavior, or fix the value."
    )


def iter_source_claim_documents(root: Path) -> list[tuple[Path, dict[str, Any]]]:
    """Return every `.knowledge/**/*.md` document whose frontmatter declares `source_of_truth`."""
    knowledge_root = root / ".knowledge"
    documents: list[tuple[Path, dict[str, Any]]] = []
    if not knowledge_root.exists():
        return documents
    for path in sorted(knowledge_root.rglob("*.md"), key=stable_path_sort_key):
        relative = path.relative_to(root).as_posix()
        if relative in {".knowledge/index.md", ".knowledge/README.md"}:
            continue
        metadata, _ = parse_frontmatter(path)
        if metadata.get("source_of_truth"):
            validate_source_claims(path, metadata)
            for claim in metadata["source_of_truth"]:
                if isinstance(claim, dict):
                    source_path = root / claim["path"]
                    if not source_path.is_file():
                        raise ObjectClaimError(
                            f"{path}: source claim '{claim['path']}' must name an existing regular file"
                        )
            documents.append((path, metadata))
    return documents


def resolve_routine_scope(root: Path, base: str, head: str) -> set[str]:
    """Return the complete add/delete/rename path set for `base...head`, or raise `RoutineScopeError`.

    Uses the merge base of the two explicit refs (never inferred) and a three-dot comparison, per
    data-model.md's Routine Scope contract. `base` is not required to be a direct ancestor of `head`.
    """
    for ref in (base, head):
        result = run_git(root, ["rev-parse", "--verify", f"{ref}^{{commit}}"])
        if result.returncode != 0:
            raise RoutineScopeError(f"'{ref}' does not resolve to an available commit object")

    merge_base_result = run_git(root, ["merge-base", base, head])
    if merge_base_result.returncode != 0 or not merge_base_result.stdout.strip():
        raise RoutineScopeError(f"no merge base exists between '{base}' and '{head}'")

    shallow_result = run_git(root, ["rev-parse", "--is-shallow-repository"])
    if shallow_result.returncode == 0 and shallow_result.stdout.strip() == "true":
        boundary_result = run_git(root, ["rev-list", "--boundary", f"{base}...{head}"])
        if boundary_result.returncode != 0:
            raise RoutineScopeError(
                f"could not validate complete history between '{base}' and '{head}' "
                f"({boundary_result.stderr.strip() or 'git rev-list failed'})"
            )
        if any(line.startswith("-") for line in boundary_result.stdout.splitlines()):
            raise RoutineScopeError(
                f"history between '{base}' and '{head}' is truncated; fetch complete history before retrying"
            )

    diff_result = run_git(
        root,
        ["diff", "--find-renames=50%", "--name-status", f"{base}...{head}"],
    )
    if diff_result.returncode != 0:
        raise RoutineScopeError(
            f"could not compute the change set between '{base}' and '{head}' "
            f"({diff_result.stderr.strip() or 'git diff failed'})"
        )

    changed: set[str] = set()
    for line in diff_result.stdout.splitlines():
        if not line.strip():
            continue
        parts = line.split("\t")
        status = parts[0]
        if status.startswith("R") or status.startswith("C"):
            changed.update(parts[1:3])
        else:
            changed.update(parts[1:2])
    return changed


def run_git(root: Path, args: list[str], timeout: int = 30) -> subprocess.CompletedProcess[str]:
    try:
        return subprocess.run(
            ["git", *args],
            cwd=root,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except (subprocess.SubprocessError, OSError) as exc:
        raise RoutineScopeError(f"git invocation failed: {exc}") from exc


def _default_profile_for(path: Path) -> str:
    if path.suffix == ".py":
        return "python-token-v1"
    try:
        path.read_bytes().decode("utf-8")
    except UnicodeDecodeError:
        return "binary-v1"
    return "exact-text-v1"


def evaluate_document(
    root: Path, metadata: dict[str, Any]
) -> tuple[str, list[dict[str, Any]], bool, int]:
    """Return (status, changed_claims, region_widened, object_claim_count) for one document.

    status is one of: verified-current, drifted, pinned-unverified, unpinned. Raises
    `CanonicalizationError`/`OSError` when any claimed source cannot be trusted -- callers must
    treat that as a whole-run error, never a partial finding.
    """
    items = metadata.get("source_of_truth") or []
    object_claims = [item for item in items if isinstance(item, dict)]
    if not object_claims:
        return "unpinned", [], False, 0

    changed_claims: list[dict[str, Any]] = []
    region_widened = False
    for claim in object_claims:
        source_path = root / claim["path"]
        if not source_path.is_file():
            raise CanonicalizationError(f"{claim['path']}: claimed source no longer exists")
        canonical_bytes, widened = canonicalize_source(source_path, claim["profile"], claim.get("region"))
        region_widened = region_widened or widened
        current_digest = compute_digest(canonical_bytes)
        if current_digest != claim["digest"]:
            changed_claims.append(
                {
                    "path": claim["path"],
                    "profile": claim["profile"],
                    "region": claim.get("region"),
                    "digest_before": claim["digest"],
                    "digest_after": current_digest,
                }
            )

    verification = metadata.get("verification") or {}
    is_verified = verification.get("state") == "verified"
    if changed_claims:
        status = "drifted" if is_verified else "pinned-unverified"
    else:
        status = "verified-current" if is_verified else "pinned-unverified"
    return status, changed_claims, region_widened, len(object_claims)


# --- Explain-workflow claim rotation: retained-baseline diff display and confirmed re-verify --

def _binary_diff_ranges(before: bytes, after: bytes) -> list[dict[str, int]]:
    """Return deterministic changed-offset/length ranges between two byte streams."""
    matcher = difflib.SequenceMatcher(a=before, b=after, autojunk=False)
    ranges = []
    for tag, i1, i2, j1, j2 in matcher.get_opcodes():
        if tag == "equal":
            continue
        ranges.append(
            {
                "op": tag,
                "before_offset": i1,
                "before_length": i2 - i1,
                "after_offset": j1,
                "after_length": j2 - j1,
            }
        )
    return ranges


def diff_canonical_bytes(before: bytes, after: bytes, profile: str) -> dict[str, Any]:
    """Return a deterministic diff of two canonical byte streams for human confirmation.

    `binary-v1` (and anything that fails UTF-8 decoding) reports changed-offset/length ranges;
    everything else reports a deterministic unified diff. Token-canonical text (python-token-v1)
    uses `\\x1f` between tokens rather than newlines, so it is split into one line per token for a
    readable diff instead of one undifferentiated line.
    """
    if profile == "binary-v1":
        return {"kind": "binary", "ranges": _binary_diff_ranges(before, after)}
    try:
        before_text = before.decode("utf-8")
        after_text = after.decode("utf-8")
    except UnicodeDecodeError:
        return {"kind": "binary", "ranges": _binary_diff_ranges(before, after)}
    before_lines = before_text.replace("\x1f", "\n").splitlines(keepends=True)
    after_lines = after_text.replace("\x1f", "\n").splitlines(keepends=True)
    unified = list(
        difflib.unified_diff(before_lines, after_lines, fromfile="baseline", tofile="current", lineterm="")
    )
    return {"kind": "text", "unified_diff": unified}


def enumerate_claim_states(root: Path, doc_path: Path, metadata: dict[str, Any]) -> list[dict[str, Any]]:
    """Return one entry per object-form source claim in `metadata`, read-only.

    Enumerates every claim (not only the first) so a document with multiple source claims can be
    confirmed one at a time. Raises `CanonicalizationError` if any claimed source or its retained
    baseline cannot be trusted -- callers must treat that as a whole-document error, never a
    partial listing.
    """
    verification = metadata.get("verification") or {}
    is_verified = verification.get("state") == "verified"
    entries: list[dict[str, Any]] = []
    for claim in metadata.get("source_of_truth") or []:
        if not isinstance(claim, dict):
            continue
        source_path = root / claim["path"]
        if not source_path.is_file():
            raise CanonicalizationError(f"{claim['path']}: claimed source no longer exists")
        baseline_path = root / claim["baseline"]
        if not baseline_path.is_file():
            raise CanonicalizationError(f"{claim['baseline']}: retained baseline is missing")
        baseline_bytes = baseline_path.read_bytes()
        if compute_digest(baseline_bytes) != claim["digest"]:
            raise CanonicalizationError(f"{claim['baseline']}: retained baseline does not match its recorded digest")

        canonical_bytes, region_widened = canonicalize_source(source_path, claim["profile"], claim.get("region"))
        current_digest = compute_digest(canonical_bytes)
        changed = current_digest != claim["digest"]
        if changed:
            status = "drifted" if is_verified else "pinned-unverified"
        else:
            status = "verified-current" if is_verified else "pinned-unverified"
        entries.append(
            {
                "document": doc_path.relative_to(root).as_posix(),
                "path": claim["path"],
                "profile": claim["profile"],
                "region": claim.get("region"),
                "digest_before": claim["digest"],
                "digest_after": current_digest,
                "status": status,
                "region_widened": region_widened,
                "diff": diff_canonical_bytes(baseline_bytes, canonical_bytes, claim["profile"]) if changed else None,
            }
        )
    return entries


def rotate_claim(root: Path, doc_path: Path, claim_path: str, profile: str, region: str | None = None) -> dict[str, Any]:
    """Apply a confirmed rotation transaction for exactly one source claim.

    Callers MUST already have displayed the diff from `enumerate_claim_states` and obtained
    explicit human confirmation -- this function performs no confirmation itself. Writes/dedupes
    the new baseline, atomically replaces only the targeted claim plus the document's verification
    record (`state: verified`, `confirmed_on: <today>`), then prunes only baselines no claim
    references. A failure before the document write leaves the claim unchanged; a failure during
    the document write restores the document's original bytes.
    """
    metadata, _ = parse_frontmatter(doc_path)
    items = metadata.get("source_of_truth") or []
    target_index = None
    for index, item in enumerate(items):
        if (
            isinstance(item, dict)
            and item.get("path") == claim_path
            and item.get("profile") == profile
            and item.get("region") == region
        ):
            target_index = index
            break
    if target_index is None:
        raise ObjectClaimError(
            f"{doc_path}: no source claim (path={claim_path}, profile={profile}, region={region})"
        )

    claim = items[target_index]
    source_path = root / claim["path"]
    if not source_path.is_file():
        raise CanonicalizationError(f"{claim['path']}: claimed source no longer exists")
    baseline_path = root / claim["baseline"]
    if not baseline_path.is_file():
        raise CanonicalizationError(f"{claim['baseline']}: retained baseline is missing")
    if compute_digest(baseline_path.read_bytes()) != claim["digest"]:
        raise CanonicalizationError(f"{claim['baseline']}: retained baseline does not match its recorded digest")

    canonical_bytes, region_widened = canonicalize_source(source_path, profile, region)
    new_digest = compute_digest(canonical_bytes)
    new_baseline_relpath = baseline_address(new_digest)
    new_baseline_path = root / new_baseline_relpath

    if not new_baseline_path.exists():
        new_baseline_path.parent.mkdir(parents=True, exist_ok=True)
        handle, temp_name = tempfile.mkstemp(prefix=f".{new_baseline_path.name}.", dir=new_baseline_path.parent)
        try:
            with os.fdopen(handle, "wb") as stream:
                stream.write(canonical_bytes)
                stream.flush()
                os.fsync(stream.fileno())
            os.replace(temp_name, new_baseline_path)
        except OSError:
            try:
                os.unlink(temp_name)
            except OSError:
                pass
            raise

    updated_items = list(items)
    updated_items[target_index] = {**claim, "digest": new_digest, "baseline": new_baseline_relpath}
    metadata["source_of_truth"] = updated_items
    metadata["verification"] = {"state": "verified", "confirmed_on": date.today().isoformat()}

    original_bytes = doc_path.read_bytes()
    text = doc_path.read_text(encoding="utf-8-sig")
    normalized = text.replace("\r\n", "\n")
    _, _, remainder = normalized.split("---\n", 2)
    new_frontmatter = yaml.safe_dump(metadata, sort_keys=False, allow_unicode=True)
    new_bytes = f"---\n{new_frontmatter}---\n{remainder}".encode("utf-8")
    try:
        doc_path.write_bytes(new_bytes)
    except OSError:
        doc_path.write_bytes(original_bytes)
        raise

    prune_result = prune_unreferenced_baselines(root, write=True)
    return {
        "document": doc_path.relative_to(root).as_posix(),
        "path": claim_path,
        "profile": profile,
        "region": region,
        "digest_before": claim["digest"],
        "digest_after": new_digest,
        "baseline": new_baseline_relpath,
        "region_widened": region_widened,
        "pruned": prune_result["pruned"],
    }


def run_drift_detection(
    root: Path,
    *,
    mode: str,
    changed_paths: set[str] | None,
    breadth_guidance: int | None,
    enforcement: str | None,
) -> dict[str, Any]:
    """Evaluate every in-scope document's source claims and return a complete report payload.

    Never emits a partial finding: any evaluation error aborts the whole run with `status: error`
    and zero findings, per the fail-closed contract.
    """
    summary = {"verified-current": 0, "drifted": 0, "pinned-unverified": 0, "unpinned": 0}
    findings: list[dict[str, Any]] = []
    advisories: list[dict[str, Any]] = []

    for path, metadata in iter_source_claim_documents(root):
        relative = path.relative_to(root).as_posix()
        object_claims = [item for item in (metadata.get("source_of_truth") or []) if isinstance(item, dict)]
        if mode == "routine" and changed_paths is not None:
            if not any(claim["path"] in changed_paths for claim in object_claims):
                continue

        status, changed_claims, region_widened, claim_count = evaluate_document(root, metadata)
        summary[status] = summary.get(status, 0) + 1

        if status == "drifted":
            findings.append({"document": relative, "source_claims": changed_claims})
        elif status == "pinned-unverified" and claim_count:
            advisories.append({"type": "pinned-unverified", "document": relative})
        elif status == "unpinned" and enforcement == "pinned-claims" and metadata.get("source_of_truth"):
            advisories.append({"type": "unpinned-authoring-error", "document": relative})

        if region_widened:
            advisories.append({"type": "region-widened", "document": relative})
        if breadth_guidance and claim_count > breadth_guidance:
            advisories.append({"type": "breadth", "document": relative, "count": claim_count})

    report = detection_report(mode, "findings" if findings else "ok")
    report["summary"] = summary
    report["findings"] = findings
    report["advisories"] = advisories
    return report


def migrate_source_claims(root: Path, *, write: bool) -> dict[str, Any]:
    """Preview (default) or apply (`write=True`) the legacy-string-to-object-claim migration.

    Validates and stages the full corpus before touching any document -- a single unreadable or
    directory source aborts the whole migration with no partial document updates.
    """
    plan: list[dict[str, Any]] = []
    staged: list[tuple[Path, str, str, bytes]] = []
    for path, metadata in iter_source_claim_documents(root):
        legacy_items = [item for item in (metadata.get("source_of_truth") or []) if isinstance(item, str)]
        if not legacy_items:
            continue
        document_plan = {"document": path.relative_to(root).as_posix(), "claims": []}
        for legacy_path in legacy_items:
            source_path = root / legacy_path
            if source_path.is_dir():
                raise ObjectClaimError(f"{legacy_path}: cannot migrate a directory claim")
            if not source_path.is_file():
                raise ObjectClaimError(f"{legacy_path}: claimed source does not exist")
            profile = _default_profile_for(source_path)
            canonical_bytes, _ = canonicalize_source(source_path, profile)
            digest = compute_digest(canonical_bytes)
            baseline = baseline_address(digest)
            document_plan["claims"].append(
                {"path": legacy_path, "profile": profile, "digest": digest, "baseline": baseline}
            )
            staged.append((root / baseline, digest, baseline, canonical_bytes))
        plan.append(document_plan)

    if not write:
        return {"write": False, "documents": plan}

    for baseline_path, _digest, _baseline, canonical_bytes in staged:
        if baseline_path.exists():
            continue
        baseline_path.parent.mkdir(parents=True, exist_ok=True)
        handle, temp_name = tempfile.mkstemp(prefix=f".{baseline_path.name}.", dir=baseline_path.parent)
        with os.fdopen(handle, "wb") as stream:
            stream.write(canonical_bytes)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temp_name, baseline_path)

    # Compute every document's new frontmatter text up front, then write them all atomically. If
    # any single write fails partway through, restore the documents already replaced from the
    # original bytes captured here -- a partial migration must never leave a mixed-state corpus.
    claims_by_document = {entry["document"]: entry["claims"] for entry in plan}
    rewrites: list[tuple[Path, bytes, bytes]] = []  # (path, original_bytes, new_bytes)
    for path, metadata in iter_source_claim_documents(root):
        relative = path.relative_to(root).as_posix()
        new_claims = claims_by_document.get(relative)
        if not new_claims:
            continue
        claims_by_path = {claim["path"]: claim for claim in new_claims}
        items = metadata.get("source_of_truth") or []
        updated_items = [
            claims_by_path[item] if isinstance(item, str) and item in claims_by_path else item
            for item in items
        ]
        metadata["source_of_truth"] = updated_items
        metadata["verification"] = {"state": "pinned-unverified"}
        original_bytes = path.read_bytes()
        text = path.read_text(encoding="utf-8-sig")
        normalized = text.replace("\r\n", "\n")
        _, _, remainder = normalized.split("---\n", 2)
        new_frontmatter = yaml.safe_dump(metadata, sort_keys=False, allow_unicode=True)
        new_bytes = f"---\n{new_frontmatter}---\n{remainder}".encode("utf-8")
        rewrites.append((path, original_bytes, new_bytes))

    originals_by_path = {path: original_bytes for path, original_bytes, _new_bytes in rewrites}
    written: list[Path] = []
    try:
        for path, _original_bytes, new_bytes in rewrites:
            path.write_bytes(new_bytes)
            written.append(path)
    except OSError:
        for path in written:
            path.write_bytes(originals_by_path[path])
        raise

    return {"write": True, "documents": plan}


def scan_referenced_baselines(root: Path) -> set[str]:
    """Return every baseline path (repo-relative, POSIX) any document's object claim still cites.

    A baseline under `.knowledge/evidence/source-baselines/` is safe to remove only when no
    document's `source_of_truth` object claim references it -- content addressing means a single
    baseline file may legitimately be shared by more than one claim (identical canonical bytes),
    so this is a set of referenced addresses, not a one-to-one map.
    """
    referenced: set[str] = set()
    for _path, metadata in iter_source_claim_documents(root):
        for item in metadata.get("source_of_truth") or []:
            if isinstance(item, dict) and item.get("baseline"):
                referenced.add(item["baseline"])
    return referenced


def prune_unreferenced_baselines(root: Path, *, write: bool) -> dict[str, Any]:
    """Preview (default) or delete (`write=True`) baselines no document's object claim references.

    Scans the complete corpus of `source_of_truth` object claims before removing anything --
    a baseline is pruned only when it is provably unreferenced, never based on a partial scan.
    """
    evidence_root = root / ".knowledge" / "evidence" / "source-baselines"
    if not evidence_root.exists():
        return {"write": write, "pruned": [], "kept": []}

    referenced = scan_referenced_baselines(root)
    pruned: list[str] = []
    kept: list[str] = []
    for baseline_path in sorted(evidence_root.rglob("*.baseline"), key=stable_path_sort_key):
        relative = baseline_path.relative_to(root).as_posix()
        if relative in referenced:
            kept.append(relative)
            continue
        pruned.append(relative)
        if write:
            baseline_path.unlink()

    return {"write": write, "pruned": pruned, "kept": kept}


def currency_report(root: Path, *, currency_days: int) -> dict[str, Any]:
    """Advisory-only report of `verified` documents whose `confirmed_on` exceeds `currency_days`."""
    stale: list[dict[str, Any]] = []
    today = datetime.now(UTC).date()
    for path, metadata in iter_source_claim_documents(root):
        verification = metadata.get("verification") or {}
        if verification.get("state") != "verified":
            continue
        confirmed_on = verification.get("confirmed_on")
        if not confirmed_on:
            continue
        age_days = (today - date.fromisoformat(str(confirmed_on))).days
        if age_days > currency_days:
            stale.append(
                {
                    "document": path.relative_to(root).as_posix(),
                    "confirmed_on": str(confirmed_on),
                    "age_days": age_days,
                }
            )
    return {"schema_version": "1", "currency_days": currency_days, "stale": stale}


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate the current BSW.DevSpark knowledge index")
    parser.add_argument("--repo-root", default=".")
    parser.add_argument("--output")
    parser.add_argument("--coverage-output")
    parser.add_argument("--stdout", action="store_true")
    parser.add_argument("--generated-at")
    parser.add_argument(
        "--check",
        action="store_true",
        help="Fail if committed index.json/coverage.json are stale; never writes files.",
    )
    parser.add_argument(
        "--fail-on-stale",
        action="store_true",
        help=(
            "Exit 1 if any entity-layer doc's last_verified is older than its "
            "source_of_truth's newest commit. Off by default so a routine build never blocks "
            "on an unrelated edit to a cited file; use this flag for a dedicated, periodic "
            "knowledge-currency check."
        ),
    )
    parser.add_argument(
        "--unmapped-files",
        nargs="+",
        metavar="PATH",
        help="Print (as a JSON array) the subset of given paths matching no node's appliesTo pattern; never writes files.",
    )
    parser.add_argument("--detect-drift", action="store_true")
    scope = parser.add_mutually_exclusive_group()
    scope.add_argument("--full-inventory", action="store_true")
    scope.add_argument("--base")
    parser.add_argument("--head")
    parser.add_argument("--report-output")
    parser.add_argument("--breadth-guidance", type=int)
    parser.add_argument("--migrate-source-claims", action="store_true")
    parser.add_argument(
        "--prune-baselines",
        action="store_true",
        help="Preview (default) or delete (--write) baselines no document's object claim references.",
    )
    parser.add_argument("--write", action="store_true")
    parser.add_argument("--currency-report", action="store_true")
    parser.add_argument("--currency-days", type=int, default=90)
    parser.add_argument(
        "--explain-claim",
        metavar="DOC_PATH",
        help="Enumerate every source claim in DOC_PATH (repo-relative) with its status and diff; never writes.",
    )
    parser.add_argument(
        "--rotate-claim",
        metavar="DOC_PATH",
        help="Apply a confirmed rotation for one claim in DOC_PATH; requires --claim-path and --claim-profile.",
    )
    parser.add_argument("--claim-path", metavar="PATH")
    parser.add_argument("--claim-profile", metavar="PROFILE")
    parser.add_argument("--claim-region", metavar="REGION")
    args = parser.parse_args()
    root = Path(args.repo_root).resolve()

    if args.breadth_guidance is not None and args.breadth_guidance <= 0:
        print("--breadth-guidance must be a positive integer", file=sys.stderr)
        return 2

    if args.migrate_source_claims:
        try:
            result = migrate_source_claims(root, write=args.write)
        except (ObjectClaimError, OSError) as exc:
            print(f"migration failed, no document was updated: {exc}", file=sys.stderr)
            return 2
        print(json.dumps(result, indent=2))
        return 0

    if args.prune_baselines:
        try:
            result = prune_unreferenced_baselines(root, write=args.write)
        except OSError as exc:
            print(f"baseline prune failed: {exc}", file=sys.stderr)
            return 2
        print(json.dumps(result, indent=2))
        return 0

    if args.explain_claim:
        doc_path = (root / args.explain_claim).resolve()
        try:
            metadata, _ = parse_frontmatter(doc_path)
            entries = enumerate_claim_states(root, doc_path, metadata)
        except (CanonicalizationError, ObjectClaimError, OSError, ValueError) as exc:
            print(f"claim enumeration failed: {exc}", file=sys.stderr)
            return 2
        print(json.dumps(entries, indent=2))
        return 0

    if args.rotate_claim:
        if not args.claim_path or not args.claim_profile:
            print("--rotate-claim requires --claim-path and --claim-profile", file=sys.stderr)
            return 2
        doc_path = (root / args.rotate_claim).resolve()
        try:
            result = rotate_claim(root, doc_path, args.claim_path, args.claim_profile, args.claim_region)
        except (CanonicalizationError, ObjectClaimError, OSError) as exc:
            print(f"claim rotation failed, document was not changed: {exc}", file=sys.stderr)
            return 2
        print(json.dumps(result, indent=2))
        return 0

    if args.currency_report:
        if args.currency_days <= 0:
            print("--currency-days must be a positive integer", file=sys.stderr)
            return 2
        report = currency_report(root, currency_days=args.currency_days)
        if args.report_output:
            try:
                write_report_atomically(Path(args.report_output).resolve(), report)
            except OSError as exc:
                print(f"report write failed: {exc}", file=sys.stderr)
                return 2
        print(json.dumps(report, indent=2))
        return 0

    if args.detect_drift:
        report_path = Path(args.report_output).resolve() if args.report_output else None
        mode = "full-inventory" if args.full_inventory else "routine"
        if bool(args.base) != bool(args.head) and not args.full_inventory:
            errors = ["routine drift detection requires both --base and --head"]
        elif not args.full_inventory and not (args.base and args.head):
            errors = ["drift detection requires --full-inventory or both --base and --head"]
        else:
            reset_canonicalization_cache()
            errors = run_self_test(root)

        changed_paths: set[str] | None = None
        enforcement: str | None = None
        if not errors:
            try:
                enforcement = enforcement_mode(root)
            except EnforcementConfigError as exc:
                errors = [str(exc)]
            if not errors and mode == "routine":
                try:
                    changed_paths = resolve_routine_scope(root, args.base, args.head)
                except RoutineScopeError as exc:
                    errors = [str(exc)]

        if errors:
            if report_path:
                try:
                    write_report_atomically(report_path, detection_report(mode, "error", errors))
                except OSError as exc:
                    print(f"report write failed: {exc}", file=sys.stderr)
            for error in errors:
                print(error, file=sys.stderr)
            return 2

        try:
            report = run_drift_detection(
                root,
                mode=mode,
                changed_paths=changed_paths,
                breadth_guidance=args.breadth_guidance,
                enforcement=enforcement,
            )
        except (CanonicalizationError, ObjectClaimError, OSError) as exc:
            report = detection_report(mode, "error", [str(exc)])
            if report_path:
                try:
                    write_report_atomically(report_path, report)
                except OSError as write_exc:
                    print(f"report write failed: {write_exc}", file=sys.stderr)
            print(str(exc), file=sys.stderr)
            return 2

        if report_path:
            try:
                write_report_atomically(report_path, report)
            except OSError as exc:
                print(f"report write failed: {exc}", file=sys.stderr)
                return 2

        summary = report["summary"]
        pinned_count = summary.get("pinned-unverified", 0)
        pinned_items = [a["document"] for a in report["advisories"] if a["type"] == "pinned-unverified"]
        suffix = f"; {pinned_count} pinned-unverified {pinned_items}" if pinned_count else ""
        if report["findings"]:
            print(f"Knowledge drift detection found {len(report['findings'])} finding(s){suffix}", file=sys.stderr)
            return 1
        print(f"Knowledge drift detection passed: 0 findings{suffix}")
        return 0

    index = build_index(root, args.generated_at)
    stale_entities = index.get("stale_entities", [])
    output = Path(args.output) if args.output else root / ".knowledge/index.json"
    coverage_output = Path(args.coverage_output) if args.coverage_output else root / ".knowledge/ontology/coverage.json"

    if args.unmapped_files is not None:
        print(json.dumps(find_unmapped_files(index, args.unmapped_files)))
        return 0

    if stale_entities:
        for finding in stale_entities:
            print(
                f"STALE: {finding['path']}: source_of_truth '{finding['source']}' changed "
                f"({finding['source_date']}) after last_verified ({finding['last_verified']}) "
                "-- confirm the doc still matches the code before bumping last_verified "
                "(prefer running /devspark-explain <topic> over a bare hand-edit of the date)",
                file=sys.stderr,
            )
        if args.fail_on_stale:
            print(
                f"{len(stale_entities)} entity-layer doc(s) have a stale last_verified; "
                "failing because --fail-on-stale was set.",
                file=sys.stderr,
            )

    if args.check:
        problems = [
            _drift(output, index),
            _drift(coverage_output, build_coverage_report(index, args.generated_at)),
        ]
        problems = [p for p in problems if p]
        if problems:
            for problem in problems:
                print(f"DRIFT: {problem}", file=sys.stderr)
            print(
                "Regenerate with: python3 scripts/build_knowledge_index.py --repo-root .",
                file=sys.stderr,
            )
            return 1
        return 1 if (args.fail_on_stale and stale_entities) else 0

    payload = json.dumps(index, indent=2, ensure_ascii=False) + "\n"
    if args.stdout:
        # Indexed body text carries whatever the docs contain; a cp1252 console must not decide
        # whether the index can be emitted.
        if hasattr(sys.stdout, "reconfigure"):
            sys.stdout.reconfigure(encoding="utf-8")
        print(payload, end="")
    else:
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(payload, encoding="utf-8")

    # Written on every run, including the zero-entity case -- otherwise removing the final entity
    # leaves the previous report in place as apparent current truth.
    if not args.stdout:  # --stdout emits only the index, matching legacy behavior
        coverage = build_coverage_report(index, args.generated_at)
        coverage_payload = json.dumps(coverage, indent=2, ensure_ascii=False) + "\n"
        coverage_output.parent.mkdir(parents=True, exist_ok=True)
        coverage_output.write_text(coverage_payload, encoding="utf-8")
    return 1 if (args.fail_on_stale and stale_entities) else 0


if __name__ == "__main__":
    raise SystemExit(main())

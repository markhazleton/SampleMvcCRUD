#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Evidence-based `.knowledge` maintenance discovery for `/devspark.discover-knowledge`.

Discovery proposes, evidence supports, humans approve: this script never promotes inferred
information into authoritative `.knowledge` on its own initiative. It inspects the same signals
`build_knowledge_index.py` already validates -- `source_of_truth`, `appliesTo`, entity
`relations`/`constrained_by`, `links.references`, and object-claim drift -- plus repository code
and test layout, and reports gaps, weak mappings, relationship/alias candidates, contradictions,
and historical leakage as structured, evidence-backed findings for a human (or a chaining agent,
with confirmation) to act on.

This is an authoring/maintenance aid, not agent memory, not automatic documentation generation,
not historical reconstruction, not a replacement for `/devspark-explain`, and not a runtime
retrieval mechanism -- it never becomes the Plan Context Resolution engine itself.

The two mechanical, schema-conformant edits it can apply on request (`--apply-alias`,
`--apply-relation-from`/`--apply-relation-to`) still require an explicit `--write` confirmation,
mirroring `explain-context.py --rotate-claim`. Every other finding is left for a human or a
chaining agent to draft, per the command template's targeted-application workflow.
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from collections import Counter
from pathlib import Path
from typing import Any

import yaml

sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_knowledge_index import (  # noqa: E402
    CanonicalizationError,
    ObjectClaimError,
    build_index,
    evaluate_document,
    iter_source_claim_documents,
    parse_frontmatter,
)
from knowledge_findings import make_finding  # noqa: E402

STOPWORDS = {
    "how", "is", "the", "a", "an", "of", "in", "to", "and", "or", "does", "do", "are", "for",
    "on", "with", "what", "when", "where", "why", "this", "that", "was", "were", "be", "it",
    "test", "tests", "spec", "specs", "impl", "index", "main", "base", "core", "util", "utils",
}
SOURCE_EXTENSIONS = {".py", ".cs", ".ts", ".tsx", ".js", ".jsx", ".go", ".rs", ".java", ".rb", ".ps1", ".sh"}
# The two ways Knowledge may reference a repository path. They are not interchangeable: ownership
# asserts "changing this file may make me stale", applicability asserts "I govern this file".
# Only ownership can conflict, so only ownership feeds overlapping-ownership analysis.
CLAIM_OWNERSHIP = "source_of_truth"
CLAIM_APPLICABILITY = "appliesTo"
EXCLUDED_DIRS = {
    ".git", ".devspark.work", ".archive", ".devspark", "node_modules", "__pycache__",
    ".venv", "venv", "dist", "build", "bin", "obj",
}
TEST_PATH_RE = re.compile(r"(?i)(^|/)(tests?|__tests__|spec)(/|$)|(_test\.|\.test\.|_spec\.|\.spec\.)")
GLOB_CHARS = re.compile(r"[*?\[]")
IDENTIFIER_RE = re.compile(r"[A-Za-z_][A-Za-z0-9_]{3,}")
CAMEL_RE = re.compile(r"(?<=[a-z0-9])(?=[A-Z])")
# Durable code/tests/`.knowledge` may never carry a spec/plan/task/quickfix identifier (Command
# Preamble Contract §0) -- an identifier match here is a contract violation, not a style nit.
EPHEMERAL_ID_RE = re.compile(r"\b(?:FR|NFR)-\d+\b|\bspec[- ]?\d{2,}\b|\bT\d{3,}\b|\bQF-\d{4}-\d+\b", re.IGNORECASE)
HISTORICAL_HEADING_RE = re.compile(r"(?i)^(history|historical|background|previously|legacy|migration|superseded|deprecated)\b")

DEFAULT_MIN_CLUSTER_FILES = 3
DEFAULT_OVERBROAD_THRESHOLD = 25
DEFAULT_MIN_ALIAS_OCCURRENCES = 3
MAX_ENTITY_PAIR_SCAN = 300  # bounded: pairwise co-reference evidence is O(n^2)
# A test file referenced by this many entities, or this fraction of all entities (whichever is
# larger), is treated as a shared/hub fixture (e.g. `conftest.py`, a generic startup test) whose
# co-occurrence is never, by itself, evidence of a relationship between any specific pair.
HUB_TEST_MIN_ENTITIES = 4
HUB_TEST_ENTITY_RATIO = 0.4
# Generic English/programming vocabulary that is never a useful alias candidate on its own,
# regardless of repository. Deliberately framework-generic (booleans, primitive/type names,
# common single-token programming nouns) -- never Triage- or repository-specific terminology.
GENERIC_ALIAS_STOPWORDS = {
    "true", "false", "none", "null", "self", "this", "args", "kwargs",
    "list", "dict", "tuple", "set", "str", "int", "float", "bool", "bytes",
    "object", "objects", "array", "index", "value", "values", "key", "keys",
    "data", "item", "items", "name", "names", "type", "types", "id", "ids",
    "base", "model", "models", "cache", "config", "configs", "active",
    "total", "count", "counts", "result", "results", "response", "request",
    "error", "errors", "status", "state", "states", "context", "contexts",
    "option", "options", "param", "params", "default", "defaults", "helper",
    "helpers", "util", "utils", "common", "shared", "internal", "public",
    "private", "async", "await", "return", "func", "function", "functions",
    "class", "classes", "module", "modules", "field", "fields", "record",
    "records", "flag", "flags", "entry", "entries",
}
# A term is a repo-wide "hub" alias candidate (appears broadly rather than distinctively) when it
# occurs in source files owned by at least this many OTHER entities, or this fraction of all
# entities, whichever is larger -- mirrors the relationship hub-signal suppression rationale.
HUB_ALIAS_MIN_ENTITIES = 3
HUB_ALIAS_ENTITY_RATIO = 0.3


def repository_root() -> Path:
    # Same convention as `scripts/knowledge-integrity.py`: default to git-toplevel detection, but
    # an explicit `--repo-root` (see `main()`) always wins, so this tool can be pointed at a
    # disposable fixture repo in tests/CI without relying on `.git` discovery.
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"], check=False, capture_output=True, text=True, encoding="utf-8"
    )
    return Path(result.stdout.strip()) if result.returncode == 0 else Path.cwd().resolve()


def relative(repo: Path, path: Path) -> str:
    return path.relative_to(repo).as_posix()


def _looks_like_glob(pattern: str) -> bool:
    return bool(GLOB_CHARS.search(pattern))


def _split_identifier(identifier: str) -> str:
    spaced = CAMEL_RE.sub(" ", identifier).replace("_", " ")
    words = [w for w in spaced.lower().split() if w not in STOPWORDS and len(w) > 2]
    return " ".join(words)


def _filename_tokens(rel_path: str) -> list[str]:
    stem = CAMEL_RE.sub("_", Path(rel_path).stem)
    parts = re.split(r"[_\-.]+", stem.lower())
    return [p for p in parts if len(p) > 2 and p not in STOPWORDS]


def _dominant_concept(files: list[str]) -> str | None:
    counter: Counter[str] = Counter()
    for file_path in files:
        counter.update(set(_filename_tokens(file_path)))
    if not counter:
        return None
    term, count = counter.most_common(1)[0]
    if count < 2:
        return None
    return term.replace("_", " ").title()


def resolve_scope(repo: Path, index: dict[str, Any], scope_arg: str | None) -> dict[str, Any]:
    """`repository` (no argument), `entity` (arg matches a known entity id), or `path` (best-effort
    prefix filter -- accepted even when nothing exists at that exact path yet, since the caller may
    be scoping to an area not yet under any knowledge mapping)."""
    if not scope_arg:
        return {"type": "repository", "value": None}
    entity_ids = {str(entity["id"]) for entity in index.get("entities", [])}
    if scope_arg in entity_ids:
        return {"type": "entity", "value": scope_arg}
    return {"type": "path", "value": scope_arg.rstrip("/")}


def path_in_scope(scope: dict[str, Any], path_str: str, entity_files: set[str] | None = None) -> bool:
    """`entity_files` is the resolved set of files owned by an `entity`-type scope (see
    `entity_scope_files()`). Passing it turns entity scoping from a no-op (pre-7.6.1: any
    category not already entity-aware silently matched everything) into a real membership test.
    Callers that only ever filter by path-type scope may omit it.
    """
    if scope["type"] == "path":
        if not path_str:
            return True
        prefix = str(scope["value"])
        return path_str == prefix or path_str.startswith(prefix + "/")
    if scope["type"] == "entity":
        if not path_str or entity_files is None:
            return False
        return path_str in entity_files
    return True


def entity_scope_files(repo: Path, index: dict[str, Any], entity_id: str) -> set[str]:
    """Every file an `entity`-type scope should be treated as covering: its own layer documents
    plus every concrete file their declared `source_of_truth` entries expand to (a directory or
    glob claim, like a claim-record `appliesTo` pattern, must resolve to the individual files it
    covers -- not remain an opaque literal string -- so that per-file comparisons in
    `discover_mapping_ambiguities()` and other path-oriented categories actually overlap). Used to
    make path-oriented discovery categories (contradictions, historical-leakage, broad-mapping,
    overlapping-ownership) respect entity scoping instead of silently returning the
    repository-wide result (Fix #4).
    """
    layer_docs = _entity_layer_docs(index, entity_id)
    files = {str(doc.get("path")) for doc in layer_docs if doc.get("path")}
    for source in _entity_source_files(layer_docs):
        expanded = expand_pattern(repo, source)
        files.update(expanded or [source])
    return files


# Categories whose findings describe undocumented file clusters with no owning entity at all --
# these can never be attributed to a specific `entity`-type scope, so under entity scoping they
# are reported separately as global findings rather than silently included in (or silently
# excluded without explanation from) the scoped result set (Fix #4, Fixture #14).
ENTITY_UNSCOPABLE_CATEGORIES = {"knowledge-gap", "entity-candidate"}


def expand_pattern(repo: Path, pattern: str) -> list[str]:
    """Expand a declared `appliesTo`/`source_of_truth` pattern (glob, directory, or exact file) to
    the concrete files it currently matches. Shared by `collect_claims()` (entity/node ownership)
    and `discover_governance_relationships()` (governance-decision source-path overlap).
    """
    if _looks_like_glob(pattern):
        try:
            candidates = set(repo.glob(pattern))
            # A bare trailing '**' is directories-only on Python 3.11/3.12 but also matches
            # files on 3.13+ (pathlib's recursive-glob semantics changed). Union in the
            # explicit '**/*' form so file matches are found on every supported Python version.
            if pattern.endswith("**"):
                candidates |= set(repo.glob(pattern[:-2] + "**/*"))
            return sorted(relative(repo, p) for p in candidates if p.is_file())
        except (ValueError, OSError):
            return []
    target = repo / pattern
    if target.is_dir():
        return sorted(relative(repo, p) for p in target.rglob("*") if p.is_file())
    if target.is_file():
        return [pattern]
    return []


def collect_claims(
    repo: Path, index: dict[str, Any]
) -> tuple[dict[str, list[str]], list[dict[str, Any]], dict[str, list[str]]]:
    """Return (claims_by_file, claim_records, ownership_by_file) by expanding every declared
    `appliesTo` mapping (all node types) and every entity-layer `source_of_truth` claim -- never
    inferred, only what the repository already declares.

    The two claim kinds answer different questions and are kept apart. `source_of_truth` is
    evidence ownership ("changing this file may make the document stale"); `appliesTo` is
    applicability scope ("this document governs that file"). `ownership_by_file` therefore holds
    ownership claims only, because a governance document applying to a file an entity owns is
    normal and must never read as an ownership conflict. `claims_by_file` keeps both kinds and is
    used only for "is this file described by anything at all?" questions such as gap detection.
    `claim_records` lists each declared mapping with its kind and match count.
    """
    claims_by_file: dict[str, list[str]] = {}
    ownership_by_file: dict[str, list[str]] = {}
    claim_records: list[dict[str, Any]] = []

    def _add(node_id: str, kind: str, pattern: str) -> None:
        matches = expand_pattern(repo, pattern)
        claim_records.append({"node": node_id, "kind": kind, "pattern": pattern, "match_count": len(matches)})
        for match in matches:
            claims_by_file.setdefault(match, []).append(node_id)
            if kind == CLAIM_OWNERSHIP:
                ownership_by_file.setdefault(match, []).append(node_id)

    for node in index.get("nodes", []):
        node_id = str(node["id"])
        for pattern in node.get("appliesTo", []) or []:
            _add(node_id, CLAIM_APPLICABILITY, str(pattern))
        # Ownership is a property of the entity layer form, not of any role a document declares.
        if (node.get("form") or node.get("type")) == "entity-layer":
            for claim in node.get("source_of_truth", []) or []:
                path_value = claim.get("path") if isinstance(claim, dict) else claim
                if path_value:
                    _add(node_id, CLAIM_OWNERSHIP, str(path_value))

    return claims_by_file, claim_records, ownership_by_file


def _flat_or_entity(production: list[str], tests: list[str]) -> str:
    """Do not encourage entity proliferation: a cluster needs at least two independent signals of
    durable, multi-layered domain behavior before it is worth the entity's coverage/drift
    overhead. A single-purpose guide or architectural note should remain flat."""
    signals = 0
    if len(production) >= 3:
        signals += 1
    if len(tests) >= 3:
        signals += 1
    if len({Path(p).parent.as_posix() for p in production}) > 1:
        signals += 1
    return "entity_candidate" if signals >= 2 else "flat_knowledge_document"


def discover_knowledge_gaps(
    repo: Path, claims_by_file: dict[str, list[str]], scope: dict[str, Any], min_cluster_files: int
) -> list[dict[str, Any]]:
    clusters: dict[str, dict[str, list[str]]] = {}
    for path in repo.rglob("*"):
        if not path.is_file() or any(part in EXCLUDED_DIRS for part in path.parts):
            continue
        rel = relative(repo, path)
        if rel.startswith(".knowledge/"):
            continue
        if path.suffix.lower() not in SOURCE_EXTENSIONS or rel in claims_by_file:
            continue
        if not path_in_scope(scope, rel):
            continue
        cluster_key = path.parent.relative_to(repo).as_posix() or "."
        bucket = clusters.setdefault(cluster_key, {"production": [], "tests": []})
        bucket["tests" if TEST_PATH_RE.search(rel) else "production"].append(rel)

    findings: list[dict[str, Any]] = []
    for cluster_key in sorted(clusters):
        bucket = clusters[cluster_key]
        production = sorted(bucket["production"])
        tests = sorted(bucket["tests"])
        if len(production) < min_cluster_files:
            continue
        concept = _dominant_concept(production + tests)
        area_label = concept or cluster_key
        recommended_kind = _flat_or_entity(production, tests)
        confidence = "high" if tests and len(production) >= min_cluster_files * 2 else "medium"
        evidence = [
            {"type": "cluster", "area": f"{cluster_key}/**", "production_file_count": len(production), "test_file_count": len(tests)},
            {"type": "repeated-concept", "value": concept},
            {"type": "sample-files", "paths": production[:5]},
        ]
        if recommended_kind == "entity_candidate":
            findings.append(
                make_finding(
                    category="entity-candidate",
                    severity="low",
                    confidence=confidence,
                    subject=f"{cluster_key}/**",
                    summary=f"'{cluster_key}/**' shows multiple independent signals of durable domain behavior with no existing knowledge node.",
                    evidence=evidence,
                    recommendation=f"Consider creating an entity for {area_label} rather than a single flat document.",
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("entity-candidate", cluster_key),
                )
            )
        else:
            findings.append(
                make_finding(
                    category="knowledge-gap",
                    severity="low",
                    confidence=confidence,
                    subject=f"{cluster_key}/**",
                    summary=f"'{cluster_key}/**' has {len(production)} production file(s) and no current knowledge document.",
                    evidence=evidence,
                    recommendation=f"Consider creating current knowledge for {area_label}.",
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("knowledge-gap", cluster_key),
                )
            )
    return findings


def discover_mapping_gaps(index: dict[str, Any], scope: dict[str, Any]) -> list[dict[str, Any]]:
    path_by_id = {str(node["id"]): str(node.get("path", "")) for node in index.get("nodes", [])}
    findings: list[dict[str, Any]] = []
    for item in index.get("dangling_references", []):
        if scope["type"] == "entity" and scope["value"] not in (item["from"], item["to"]):
            continue
        if scope["type"] == "path" and not path_in_scope(scope, path_by_id.get(item["from"], item["path"])):
            continue
        findings.append(
            make_finding(
                category="missing-mapping",
                severity="medium",
                confidence="high",
                subject=f"{item['from']}->{item['to']}",
                summary=f"'{item['from']}' links.references '{item['to']}', which does not resolve to any current node or entity.",
                evidence=[{"type": "document", "path": item["path"]}],
                recommendation=f"Author the missing knowledge for '{item['to']}' or correct the reference in '{item['from']}'.",
                changes_authoritative_truth=False,
                source="discover-knowledge",
                id_parts=("missing-mapping", item["from"], item["to"]),
            )
        )
    return findings


def discover_mapping_ambiguities(
    claim_records: list[dict[str, Any]],
    claims_by_file: dict[str, list[str]],
    overbroad_threshold: int,
    scope: dict[str, Any],
    entity_files: set[str] | None,
    ownership_by_file: dict[str, list[str]],
) -> list[dict[str, Any]]:
    # Fix #4: prior to 7.6.1, this function ignored `scope` entirely -- broad-mapping and
    # overlapping-ownership findings were identical across every scoped invocation. Both
    # categories describe claimed *files*, so they are scoped by whether the claimed/matched
    # files fall inside the scope: `entity_files` (pre-resolved to the scoped entity's own layer
    # docs + declared source files) for entity scope, `path_in_scope` for path scope. Comparing
    # `record["node"]` to the scope's entity id directly would never match, because claim records
    # are keyed by the owning DOCUMENT's id (e.g. "checkout-architecture"), not the entity id
    # ("checkout") itself.
    findings: list[dict[str, Any]] = []
    for record in claim_records:
        if record["match_count"] <= overbroad_threshold:
            continue
        if scope["type"] in ("entity", "path"):
            matched_paths = [p for p, owners in claims_by_file.items() if record["node"] in owners]
            in_scope = record["node"] in (entity_files or set()) or any(
                path_in_scope(scope, p, entity_files) for p in matched_paths
            )
            if not in_scope:
                continue
        findings.append(
            make_finding(
                category="broad-mapping",
                severity="low",
                confidence="medium",
                subject=f"{record['node']}:{record['pattern']}",
                summary=f"'{record['node']}' claims {record['match_count']} files via '{record['pattern']}'.",
                evidence=[{"type": "mapping", "node": record["node"], "pattern": record["pattern"], "matched_file_count": record["match_count"]}],
                recommendation=f"Narrow '{record['pattern']}' if only a subset of matched files actually supports '{record['node']}'.",
                changes_authoritative_truth=False,
                source="discover-knowledge",
                id_parts=("broad-mapping", record["node"], record["pattern"]),
            )
        )

    groups: dict[tuple[str, ...], list[str]] = {}
    for path, owners in ownership_by_file.items():
        key = tuple(sorted(set(owners)))
        if len(key) > 1:
            groups.setdefault(key, []).append(path)
    for owners, paths in sorted(groups.items()):
        if scope["type"] in ("entity", "path") and not any(path_in_scope(scope, p, entity_files) for p in paths):
            continue
        findings.append(
            make_finding(
                category="overlapping-ownership",
                severity="low",
                confidence="medium",
                subject="|".join(owners),
                summary=f"{', '.join(owners)} jointly claim {len(paths)} file(s) as source_of_truth.",
                evidence=[
                    {"type": "shared-paths", "paths": sorted(paths)[:10], "shared_path_count": len(paths)},
                    {"type": "claim-kind", "value": CLAIM_OWNERSHIP},
                ],
                recommendation=(
                    f"Confirm whether the overlap between {', '.join(owners)} reflects genuinely "
                    "shared behavior or should be narrowed to a single owner."
                ),
                changes_authoritative_truth=False,
                source="discover-knowledge",
                id_parts=("overlapping-ownership", *owners),
            )
        )
    return findings



def _entity_layer_docs(index: dict[str, Any], entity_id: str) -> list[dict[str, Any]]:
    return [node for node in index.get("nodes", []) if node.get("entity") == entity_id]


def _entity_source_files(layer_docs: list[dict[str, Any]]) -> set[str]:
    files: set[str] = set()
    for doc in layer_docs:
        for claim in doc.get("source_of_truth", []) or []:
            path_value = claim.get("path") if isinstance(claim, dict) else claim
            if path_value:
                files.add(str(path_value))
    return files


def _entity_path(index: dict[str, Any], entity_id: str) -> str:
    for entity in index.get("entities", []):
        if entity["id"] == entity_id:
            return str(entity.get("path", ""))
    return ""


def discover_relationship_candidates(repo: Path, index: dict[str, Any], scope: dict[str, Any]) -> list[dict[str, Any]]:
    entities = index.get("entities", [])
    if len(entities) > MAX_ENTITY_PAIR_SCAN:
        return []
    existing_edges = {(edge["from"], edge["to"]) for edge in index.get("edges", [])}
    existing_edges |= {(target, source) for source, target in existing_edges}

    entity_terms: dict[str, dict[str, Any]] = {}
    for entity in entities:
        layer_docs = _entity_layer_docs(index, str(entity["id"]))
        aliases = {str(entity["id"]).lower()}
        for doc in layer_docs:
            aliases.update(str(a).lower() for a in doc.get("aliases", []) or [])
        entity_terms[str(entity["id"])] = {
            "aliases": aliases,
            "layer_docs": layer_docs,
            "source_files": _entity_source_files(layer_docs),
        }

    test_files = [
        path
        for path in repo.rglob("*")
        if path.is_file()
        and path.suffix.lower() in SOURCE_EXTENSIONS
        and not any(part in EXCLUDED_DIRS for part in path.parts)
        and TEST_PATH_RE.search(relative(repo, path))
    ]
    test_text_cache: dict[str, str] = {}

    def _test_text(path: Path) -> str:
        rel = relative(repo, path)
        if rel not in test_text_cache:
            try:
                test_text_cache[rel] = path.read_text(encoding="utf-8", errors="ignore").lower()
            except OSError:
                test_text_cache[rel] = ""
        return test_text_cache[rel]

    # Fix #5/#6/#7 (hub-signal suppression + distinctiveness): precompute, once, which entities'
    # tokens each test file exercises. A test referenced by a large fraction of all entities (a
    # shared `conftest.py`, a generic startup/bootstrap test) is a "hub" -- its co-occurrence with
    # any two entities is not distinctive evidence of a relationship between THOSE two entities
    # specifically, so hub-test co-occurrence is excluded from relationship evidence entirely
    # rather than merely down-weighted. This also turns an O(entities^2 * tests) scan into
    # O(entities * tests) + O(pairs), which matters once MAX_ENTITY_PAIR_SCAN is large.
    entity_tokens = {
        entity_id: {Path(p).stem.lower() for p in terms["source_files"]} | {entity_id.lower()}
        for entity_id, terms in entity_terms.items()
    }
    test_entity_hits: dict[str, set[str]] = {}
    for test_path in test_files:
        rel = relative(repo, test_path)
        text = _test_text(test_path)
        hits = {
            entity_id
            for entity_id, tokens in entity_tokens.items()
            if any(t in text for t in tokens if len(t) > 2)
        }
        if hits:
            test_entity_hits[rel] = hits
    total_entities = len(entity_terms) or 1
    hub_tests = {
        rel
        for rel, hits in test_entity_hits.items()
        if len(hits) >= HUB_TEST_MIN_ENTITIES and len(hits) >= total_entities * HUB_TEST_ENTITY_RATIO
    }

    findings: list[dict[str, Any]] = []
    ids = sorted(entity_terms)
    for index_a, entity_a in enumerate(ids):
        for entity_b in ids[index_a + 1 :]:
            if (entity_a, entity_b) in existing_edges:
                continue
            if scope["type"] == "entity" and scope["value"] not in (entity_a, entity_b):
                continue
            if scope["type"] == "path" and not (
                path_in_scope(scope, _entity_path(index, entity_a)) or path_in_scope(scope, _entity_path(index, entity_b))
            ):
                continue

            # Direct evidence: an explicit textual cross-reference (heading mention) -- always
            # distinctive, never suppressed as a hub signal.
            heading_evidence: list[str] = []
            for doc in entity_terms[entity_a]["layer_docs"]:
                headings = " ".join(str(h) for h in doc.get("headings", []) or []).lower()
                if any(term in headings for term in entity_terms[entity_b]["aliases"] if len(term) > 3):
                    heading_evidence.append(f"{doc['id']} heading mentions '{entity_b}'")
            for doc in entity_terms[entity_b]["layer_docs"]:
                headings = " ".join(str(h) for h in doc.get("headings", []) or []).lower()
                if any(term in headings for term in entity_terms[entity_a]["aliases"] if len(term) > 3):
                    heading_evidence.append(f"{doc['id']} heading mentions '{entity_a}'")

            narrow_test_evidence = sorted(
                rel for rel, hits in test_entity_hits.items() if entity_a in hits and entity_b in hits and rel not in hub_tests
            )
            suppressed_hub_count = sum(
                1 for rel, hits in test_entity_hits.items() if entity_a in hits and entity_b in hits and rel in hub_tests
            )

            heading_evidence = sorted(set(heading_evidence))
            signals = (heading_evidence + [f"{t} exercises both" for t in narrow_test_evidence])[:5]
            if not signals:
                # Hub-only co-occurrence (e.g. a shared conftest.py) is never treated as evidence
                # of a relationship between this specific pair, no matter how many hub tests both
                # entities share (Fix #6: "suppress hub-signal-only pairs, not just downgrade").
                continue

            # Confidence reflects evidence specificity/distinctiveness, not raw signal count
            # (Fix #7): a direct textual cross-reference plus a narrowly scoped test outranks many
            # shared-fixture co-occurrences, which no longer count as signal at all.
            if heading_evidence and narrow_test_evidence:
                confidence = "high"
            elif heading_evidence or len(narrow_test_evidence) >= 2:
                confidence = "medium"
            else:
                confidence = "low"

            evidence_payload = [{"type": "co-reference", "signals": signals}]
            if suppressed_hub_count:
                evidence_payload.append(
                    {"type": "suppressed-hub-evidence", "suppressed_shared_fixture_count": suppressed_hub_count}
                )

            findings.append(
                make_finding(
                    category="missing-relationship",
                    severity="low",
                    confidence=confidence,
                    subject=f"{entity_a}:{entity_b}",
                    summary=f"'{entity_a}' and '{entity_b}' show co-reference evidence but no recorded relationship.",
                    evidence=evidence_payload,
                    recommendation=f"Consider recording a relationship between '{entity_a}' and '{entity_b}'.",
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("missing-relationship", entity_a, entity_b),
                )
            )
    return findings


def discover_governance_relationships(repo: Path, index: dict[str, Any], scope: dict[str, Any]) -> list[dict[str, Any]]:
    """Fix #8: candidate entity <-> governance/ADR relationships, using only evidence the
    repository already declares -- a governance-decision node's own `appliesTo`/source-path
    references overlapping an entity's `source_of_truth` files. An entity already listed in the
    decision's `constrains` is a recorded relationship, not a candidate, and is skipped.
    """
    findings: list[dict[str, Any]] = []
    decisions = [
        node
        for node in index.get("nodes", [])
        if (node.get("form") or node.get("type")) == "governance-decision"
    ]
    if not decisions:
        return findings

    entities = index.get("entities", [])
    entity_files_by_id = {
        str(entity["id"]): _entity_source_files(_entity_layer_docs(index, str(entity["id"]))) for entity in entities
    }
    # `constrains` is not a field on the governance-decision node itself -- `build_index()`
    # normalizes it into `{"from": decision_id, "to": entity_id, "rel": "constrains"}` edges once
    # entity reciprocity (`constrained_by`) is confirmed. Read it back from there.
    constrains_by_decision: dict[str, set[str]] = {}
    for edge in index.get("edges", []):
        if edge.get("rel") == "constrains":
            constrains_by_decision.setdefault(str(edge["from"]), set()).add(str(edge["to"]))

    for decision in decisions:
        decision_id = str(decision["id"])
        constrains = constrains_by_decision.get(decision_id, set())
        decision_paths: set[str] = set()
        for pattern in decision.get("appliesTo", []) or []:
            decision_paths.update(expand_pattern(repo, str(pattern)))

        for entity in entities:
            entity_id = str(entity["id"])
            if entity_id in constrains:
                continue  # already a recorded relationship, not a candidate
            if scope["type"] == "entity" and scope["value"] not in (entity_id, decision_id):
                continue
            if scope["type"] == "path" and not (
                path_in_scope(scope, _entity_path(index, entity_id)) or path_in_scope(scope, str(decision.get("path", "")))
            ):
                continue

            overlap = sorted(entity_files_by_id.get(entity_id, set()) & decision_paths)
            if not overlap:
                continue
            confidence = "medium" if len(overlap) >= 2 else "low"
            findings.append(
                make_finding(
                    category="governance-relationship-candidate",
                    severity="low",
                    confidence=confidence,
                    subject=f"{entity_id}:{decision_id}",
                    summary=(
                        f"'{entity_id}' shares {len(overlap)} source path(s) with governance decision "
                        f"'{decision_id}' but is not listed in its 'constrains'."
                    ),
                    evidence=[{"type": "shared-source-path", "decision": decision_id, "entity": entity_id, "paths": overlap[:5]}],
                    recommendation=(
                        f"Confirm whether '{decision_id}' governs '{entity_id}' and, if so, add '{entity_id}' "
                        f"to its 'constrains' list (with the reciprocal 'constrained_by' on the entity)."
                    ),
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("governance-relationship-candidate", entity_id, decision_id),
                )
            )
    return findings


def discover_alias_candidates(
    repo: Path, index: dict[str, Any], scope: dict[str, Any], min_occurrences: int
) -> list[dict[str, Any]]:
    entities = index.get("entities", [])
    total_entities = len(entities) or 1

    # Pass 1: collect every candidate term per entity (unfiltered) plus, across ALL entities, how
    # many DISTINCT entities each term appears in. This is the distinctiveness signal (Fix #9/#10)
    # -- a term appearing broadly across many unrelated entities' source files (a hub/generic
    # programming word) is not a useful alias for any single one of them, no matter how often it
    # occurs. Deliberately no repository-specific vocabulary: this is pure frequency/distinctiveness
    # plus the framework-generic `GENERIC_ALIAS_STOPWORDS` list.
    per_entity_terms: dict[str, dict[str, Any]] = {}
    term_entity_spread: dict[str, set[str]] = {}
    for entity in entities:
        entity_id = str(entity["id"])
        layer_docs = _entity_layer_docs(index, entity_id)
        known_terms = {entity_id.lower()}
        for doc in layer_docs:
            known_terms.add(str(doc.get("title", "")).lower())
            known_terms.update(str(a).lower() for a in doc.get("aliases", []) or [])
            known_terms.update(str(h).lower() for h in doc.get("headings", []) or [])
        source_files = _entity_source_files(layer_docs)

        counter: Counter[str] = Counter()
        occurrence_sources: dict[str, set[str]] = {}
        for rel_path in source_files:
            full_path = repo / rel_path
            if not full_path.is_file():
                continue
            try:
                text = full_path.read_text(encoding="utf-8", errors="ignore")
            except OSError:
                continue
            for identifier in set(IDENTIFIER_RE.findall(text)):
                term = _split_identifier(identifier)
                if term and term not in known_terms:
                    counter[term] += 1
                    occurrence_sources.setdefault(term, set()).add(rel_path)

        per_entity_terms[entity_id] = {"counter": counter, "sources": occurrence_sources}
        for term in counter:
            term_entity_spread.setdefault(term, set()).add(entity_id)

    hub_threshold = max(HUB_ALIAS_MIN_ENTITIES, total_entities * HUB_ALIAS_ENTITY_RATIO)

    findings: list[dict[str, Any]] = []
    for entity in entities:
        entity_id = str(entity["id"])
        if scope["type"] == "entity" and scope["value"] != entity_id:
            continue
        if scope["type"] == "path" and not path_in_scope(scope, str(entity.get("path", ""))):
            continue

        counter = per_entity_terms[entity_id]["counter"]
        occurrence_sources = per_entity_terms[entity_id]["sources"]
        for term, occurrences in sorted(counter.items(), key=lambda item: (-item[1], item[0])):
            if occurrences < min_occurrences:
                continue
            words = term.split()
            is_single_generic_word = len(words) == 1 and (len(term) <= 3 or term in GENERIC_ALIAS_STOPWORDS)
            if is_single_generic_word:
                continue  # generic English/programming vocabulary is never a useful alias
            spread = len(term_entity_spread.get(term, set()))
            is_hub_term = spread >= hub_threshold
            if is_hub_term and len(words) < 2:
                # A single-token term shared broadly across many entities is non-distinctive
                # noise (Fix #9); a multi-word phrase shared broadly is still worth surfacing at
                # reduced confidence since phrase specificity is itself evidence of value.
                continue
            confidence = (
                "high" if occurrences >= min_occurrences * 3 and not is_hub_term
                else "low" if is_hub_term
                else "medium" if occurrences >= min_occurrences * 2
                else "low"
            )
            findings.append(
                make_finding(
                    category="alias-candidate",
                    severity="low",
                    confidence=confidence,
                    subject=f"{entity_id}:{term}",
                    summary=f"'{term}' appears {occurrences} time(s) in '{entity_id}''s source files but is not a known alias.",
                    evidence=[{
                        "type": "alias-occurrences",
                        "source_occurrences": occurrences,
                        "sources": sorted(occurrence_sources.get(term, set()))[:5],
                        "entity_spread": spread,
                    }],
                    recommendation=f"Consider adding '{term}' as an alias for '{entity_id}'.",
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("alias-candidate", entity_id, term),
                )
            )
    return findings


def discover_contradictions(repo: Path, scope: dict[str, Any], entity_files: set[str] | None = None) -> list[dict[str, Any]]:
    # NOTE: this detects pinned object-claim drift (a `source_of_truth` claim marked verified whose
    # canonical bytes have since changed) -- a mechanical, deterministic proxy for "this document's
    # verified status can no longer be trusted". It does NOT perform cross-document semantic
    # contradiction detection (e.g. two documents asserting different values for the same fact);
    # that would require natural-language comparison this tool does not attempt. See
    # `.knowledge/governance/devspark-philosophy.md` ("Contradiction judgment is a human call").
    findings: list[dict[str, Any]] = []
    for path, metadata in iter_source_claim_documents(repo):
        rel = relative(repo, path)
        if scope["type"] in ("path", "entity") and not path_in_scope(scope, rel, entity_files):
            continue
        node_id = str(metadata.get("id") or path.stem)
        try:
            status, changed_claims, _widened, claim_count = evaluate_document(repo, metadata)
        except (CanonicalizationError, ObjectClaimError) as exc:
            findings.append(
                make_finding(
                    category="contradiction",
                    severity="medium",
                    confidence="low",
                    subject=node_id,
                    summary=f"'{node_id}' has an object-claim error and its verified status cannot currently be evaluated.",
                    evidence=[{"type": "error", "document": rel, "message": str(exc)}],
                    recommendation="Resolve the object-claim error before trusting this document's current status.",
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("contradiction-error", node_id),
                )
            )
            continue
        if status == "drifted":
            findings.append(
                make_finding(
                    category="contradiction",
                    severity="high",
                    confidence="high",
                    subject=node_id,
                    summary=f"'{node_id}' is marked verified but its pinned source claim(s) have drifted.",
                    evidence=[{"type": "claim-drift", "document": rel, "changed_claims": changed_claims}],
                    recommendation=(
                        f"'{node_id}' is marked verified but its pinned source claim(s) have "
                        "drifted; re-verify via /devspark-explain --rotate-claim or update the document."
                    ),
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("contradiction", node_id),
                )
            )
        elif status == "pinned-unverified" and claim_count:
            findings.append(
                make_finding(
                    category="contradiction",
                    severity="medium",
                    confidence="medium",
                    subject=node_id,
                    summary=f"'{node_id}' has pinned source claim(s) never confirmed verified.",
                    evidence=[{"type": "unverified-claims", "document": rel, "claim_count": claim_count}],
                    recommendation=(
                        f"'{node_id}' has pinned source claim(s) never confirmed verified; human "
                        "review is required to confirm current accuracy."
                    ),
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("inconsistency", node_id),
                )
            )
    return findings


def discover_historical_leakage(
    repo: Path, index: dict[str, Any], scope: dict[str, Any], entity_files: set[str] | None = None
) -> list[dict[str, Any]]:
    # Deterministic detection only: an ephemeral planning identifier is a regex match on exact
    # syntax (FR-###, spec-##, T###, QF-####-##), and a historical heading is a regex match
    # against the heading text itself. Neither inspects body prose for historical-sounding
    # language -- that would be heuristic and prone to false positives (e.g. the word "legacy"
    # mentioned in passing), which this tool deliberately does not attempt.
    findings: list[dict[str, Any]] = []
    for node in index.get("nodes", []):
        rel = str(node.get("path", ""))
        if scope["type"] in ("path", "entity") and not path_in_scope(scope, rel, entity_files):
            continue
        full_path = repo / rel
        if not full_path.is_file():
            continue
        try:
            _metadata, body = parse_frontmatter(full_path)
        except (OSError, ValueError):
            continue
        ephemeral_hits = sorted(set(EPHEMERAL_ID_RE.findall(body)))
        if ephemeral_hits:
            findings.append(
                make_finding(
                    category="historical-leakage",
                    severity="medium",
                    confidence="high",
                    subject=str(node["id"]),
                    summary=f"'{node['id']}' contains ephemeral planning identifier(s) that should not appear in current knowledge.",
                    evidence=[{"type": "identifiers", "document": rel, "identifiers": ephemeral_hits}],
                    recommendation=(
                        "Remove these ephemeral planning identifiers; Git history already "
                        "preserves the record that produced this current knowledge."
                    ),
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("leakage-identifier", str(node["id"])),
                )
            )
        historical_headings = [h for h in node.get("headings", []) or [] if HISTORICAL_HEADING_RE.match(str(h))]
        if historical_headings:
            findings.append(
                make_finding(
                    category="historical-leakage",
                    severity="low",
                    confidence="medium",
                    subject=str(node["id"]),
                    summary=f"'{node['id']}' has section heading(s) that read as historical/background rather than current behavior.",
                    evidence=[{"type": "headings", "document": rel, "headings": historical_headings}],
                    recommendation=(
                        "Review whether this section is necessary to understand current behavior, "
                        "should move to the documentation/history repository, or can be removed."
                    ),
                    changes_authoritative_truth=False,
                    source="discover-knowledge",
                    id_parts=("leakage-heading", str(node["id"])),
                )
            )
    return findings


def _rewrite_frontmatter(repo: Path, doc_path: Path, metadata: dict[str, Any], body: str) -> None:
    original_bytes = doc_path.read_bytes()
    new_frontmatter = yaml.safe_dump(metadata, sort_keys=False, allow_unicode=True)
    doc_path.write_bytes(f"---\n{new_frontmatter}---\n{body}".encode("utf-8"))
    try:
        build_index(repo)
    except ValueError:
        doc_path.write_bytes(original_bytes)
        raise


def apply_alias(repo: Path, node_id: str, alias_text: str, *, write: bool) -> dict[str, Any]:
    index = build_index(repo)
    node = next((n for n in index.get("nodes", []) if n["id"] == node_id), None)
    if node is None:
        raise ValueError(f"unknown knowledge id '{node_id}'")
    doc_path = repo / str(node["path"])
    metadata, body = parse_frontmatter(doc_path)
    aliases = list(metadata.get("aliases") or [])
    result = {"node": node_id, "document": node["path"], "alias": alias_text, "before": aliases, "written": False}
    if alias_text in aliases:
        result["already_present"] = True
        return result
    proposed = aliases + [alias_text]
    result["after"] = proposed
    if not write:
        return result
    metadata["aliases"] = proposed
    _rewrite_frontmatter(repo, doc_path, metadata, body)
    result["written"] = True
    return result


def apply_relation(repo: Path, from_entity: str, to_entity: str, relation: str, *, write: bool) -> dict[str, Any]:
    index = build_index(repo)
    entity_ids = {str(entity["id"]) for entity in index.get("entities", [])}
    if from_entity not in entity_ids:
        raise ValueError(f"unknown entity '{from_entity}'")
    if to_entity not in entity_ids:
        raise ValueError(f"unknown entity '{to_entity}'")
    entity_path = repo / ".knowledge" / "entities" / from_entity / "_entity.yaml"
    data = yaml.safe_load(entity_path.read_text(encoding="utf-8")) or {}
    relations = list(data.get("relations") or [])
    for existing in relations:
        if isinstance(existing, dict) and (existing.get("object") or existing.get("to")) == to_entity:
            return {"from": from_entity, "to": to_entity, "already_present": True, "written": False}
    proposed = relations + [{"predicate": relation, "object": to_entity}]
    result = {
        "from": from_entity,
        "to": to_entity,
        "relation": relation,
        "document": (Path(".knowledge/entities") / from_entity / "_entity.yaml").as_posix(),
        "before": relations,
        "after": proposed,
        "written": False,
    }
    if not write:
        return result
    original_text = entity_path.read_text(encoding="utf-8")
    data["relations"] = proposed
    entity_path.write_text(yaml.safe_dump(data, sort_keys=False, allow_unicode=True), encoding="utf-8")
    try:
        build_index(repo)
    except ValueError:
        entity_path.write_text(original_text, encoding="utf-8")
        raise
    result["written"] = True
    return result


CONFIDENCE_RANK = {"low": 0, "medium": 1, "high": 2}


def _filter_by_min_confidence(findings: list[dict[str, Any]], min_confidence: str | None) -> list[dict[str, Any]]:
    if not min_confidence:
        return findings
    floor = CONFIDENCE_RANK[min_confidence]
    return [f for f in findings if CONFIDENCE_RANK[f["confidence"]] >= floor]


def _limit_per_category(findings: list[dict[str, Any]], limit: int | None) -> list[dict[str, Any]]:
    # Fix #11: bound output volume without silently discarding the highest-confidence findings --
    # each category is sorted by confidence (descending) before truncation, so a hard limit always
    # keeps the most actionable findings in that category.
    if not limit:
        return findings
    grouped: dict[str, list[dict[str, Any]]] = {}
    for item in findings:
        grouped.setdefault(item["category"], []).append(item)
    limited: list[dict[str, Any]] = []
    for category in grouped:
        ordered = sorted(grouped[category], key=lambda f: -CONFIDENCE_RANK[f["confidence"]])
        limited.extend(ordered[:limit])
    return limited


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("scope", nargs="?", default=None, help="Entity id or repo-relative path to scope discovery to; omit for repository-wide discovery.")
    parser.add_argument("--repo-root", default=None, help="Repository root to scan; defaults to git-toplevel detection (same convention as knowledge-integrity.py).")
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--min-cluster-files", type=int, default=DEFAULT_MIN_CLUSTER_FILES)
    parser.add_argument("--overbroad-threshold", type=int, default=DEFAULT_OVERBROAD_THRESHOLD)
    parser.add_argument("--min-alias-occurrences", type=int, default=DEFAULT_MIN_ALIAS_OCCURRENCES)
    parser.add_argument("--min-confidence", choices=sorted(CONFIDENCE_RANK), default=None, help="Drop findings below this confidence level.")
    parser.add_argument("--limit-per-category", type=int, default=None, help="Cap findings per category, keeping the highest-confidence ones first.")
    parser.add_argument(
        "--include-global",
        action="store_true",
        help="When scoped, merge repository-global findings (categories that cannot be attributed to the scope) into FINDINGS instead of reporting them only under GLOBAL_FINDINGS.",
    )
    parser.add_argument("--apply-alias", metavar="NODE_ID")
    parser.add_argument("--alias-text", metavar="TEXT")
    parser.add_argument("--apply-relation-from", metavar="ENTITY_ID")
    parser.add_argument("--apply-relation-to", metavar="ENTITY_ID")
    parser.add_argument("--relation", default="relates-to")
    parser.add_argument("--write", action="store_true")
    args, _ = parser.parse_known_args()

    repo = Path(args.repo_root).resolve() if args.repo_root else repository_root()

    if args.apply_alias:
        if not args.alias_text:
            print(json.dumps({"ERROR": "--apply-alias requires --alias-text"}))
            return 2
        try:
            result = apply_alias(repo, args.apply_alias, args.alias_text, write=args.write)
        except (ValueError, OSError) as exc:
            print(json.dumps({"ERROR": str(exc)}))
            return 2
        print(json.dumps(result, separators=(",", ":")))
        return 0

    if args.apply_relation_from or args.apply_relation_to:
        if not (args.apply_relation_from and args.apply_relation_to):
            print(json.dumps({"ERROR": "--apply-relation-from and --apply-relation-to are both required"}))
            return 2
        try:
            result = apply_relation(repo, args.apply_relation_from, args.apply_relation_to, args.relation, write=args.write)
        except (ValueError, OSError) as exc:
            print(json.dumps({"ERROR": str(exc)}))
            return 2
        print(json.dumps(result, separators=(",", ":")))
        return 0

    try:
        index = build_index(repo)
    except ValueError as exc:
        print(json.dumps({"REPO_ROOT": str(repo), "ERROR": str(exc)}))
        return 1

    scope = resolve_scope(repo, index, args.scope)
    claims_by_file, claim_records, ownership_by_file = collect_claims(repo, index)
    entity_files = entity_scope_files(repo, index, scope["value"]) if scope["type"] == "entity" else None

    # Fix #4: scoped discovery now actually scopes every category. Categories that can never be
    # attributed to a specific `entity`-type scope (a cluster of undocumented files has no owning
    # entity) are computed against the full repository and reported separately as GLOBAL_FINDINGS,
    # instead of being silently identical across every scoped invocation.
    scoped_findings: list[dict[str, Any]] = []
    scoped_findings.extend(discover_mapping_gaps(index, scope))
    scoped_findings.extend(discover_mapping_ambiguities(claim_records, claims_by_file, args.overbroad_threshold, scope, entity_files, ownership_by_file))
    scoped_findings.extend(discover_relationship_candidates(repo, index, scope))
    scoped_findings.extend(discover_governance_relationships(repo, index, scope))
    scoped_findings.extend(discover_alias_candidates(repo, index, scope, args.min_alias_occurrences))
    scoped_findings.extend(discover_contradictions(repo, scope, entity_files))
    scoped_findings.extend(discover_historical_leakage(repo, index, scope, entity_files))

    global_findings: list[dict[str, Any]] = []
    if scope["type"] == "entity":
        repository_scope = {"type": "repository", "value": None}
        global_findings.extend(discover_knowledge_gaps(repo, claims_by_file, repository_scope, args.min_cluster_files))
    else:
        scoped_findings.extend(discover_knowledge_gaps(repo, claims_by_file, scope, args.min_cluster_files))

    scoped_findings = _limit_per_category(_filter_by_min_confidence(scoped_findings, args.min_confidence), args.limit_per_category)
    global_findings = _limit_per_category(_filter_by_min_confidence(global_findings, args.min_confidence), args.limit_per_category)

    findings = scoped_findings + global_findings if args.include_global else scoped_findings

    by_category: dict[str, int] = {}
    for item in findings:
        by_category[item["category"]] = by_category.get(item["category"], 0) + 1

    payload = {
        "REPO_ROOT": str(repo),
        "SCOPE": scope,
        "FINDINGS": findings,
        "SCOPED_FINDINGS": scoped_findings,
        "GLOBAL_FINDINGS": global_findings,
        "SUMMARY": {
            "total_findings": len(findings),
            "by_category": by_category,
            "entity_unscopable_categories": sorted(ENTITY_UNSCOPABLE_CATEGORIES),
        },
    }
    print(json.dumps(payload, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

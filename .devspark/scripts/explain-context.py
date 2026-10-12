#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Resolve a free-text topic against `.knowledge/` and code for `/devspark.explain`.

Reuses `build_knowledge_index.py` (frontmatter parsing, git-log dates, the built index) rather
than duplicating that logic — per constitution Principle VI, it is the single source of truth
for knowledge structure across languages.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import re
import subprocess
import sys
from functools import lru_cache
from pathlib import Path
from typing import Any

sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_knowledge_index import (  # noqa: E402
    CanonicalizationError,
    InputsUncertain,
    ObjectClaimError,
    _drift,
    _git_last_commit_date,
    build_coverage_report,
    build_index,
    enumerate_claim_states,
    inventory_changes,
    knowledge_base_inventory,
    knowledge_source_inventory,
    match_file_owners,
    normalize_search_text,
    parse_frontmatter,
    rotate_claim,
)

# A word earns a place here only when its *independent* presence carries little
# repository-specific meaning. The operational test is word class: closed-class function words
# (pronouns, determiners, auxiliaries, prepositions, conjunctions, particles) are glue in every
# repository and are removed. Open-class words -- nouns, verbs, adjectives -- stay, even when they
# feel generic, because "system", "work", "check", "order" and "claim" all name real things here.
STOPWORDS = {
    # determiners, articles, demonstratives
    "a", "an", "the", "this", "that", "these", "those", "each", "every", "any", "all", "both",
    "some", "such", "same", "other", "others", "no", "none", "own",
    # pronouns and possessives
    "i", "me", "my", "we", "us", "our", "you", "your", "he", "him", "his", "she", "her", "hers",
    "it", "its", "they", "them", "their", "there", "who", "whom", "whose", "which",
    # auxiliaries and modals
    "am", "is", "are", "was", "were", "be", "been", "being", "do", "does", "did", "have", "has",
    "had", "can", "could", "will", "would", "shall", "should", "may", "might", "must", "let",
    # prepositions and conjunctions
    "about", "across", "after", "against", "and", "as", "at", "because", "before", "between",
    "but", "by", "during", "for", "from", "if", "in", "into", "nor", "of", "off", "on", "onto",
    "or", "out", "over", "per", "so", "than", "then", "through", "to", "under", "until", "up",
    "upon", "via", "while", "with", "within", "without", "yet",
    # interrogatives and residual particles
    "how", "what", "when", "where", "why", "again", "also", "else", "just", "only", "too", "very",
    "not",
}
TEXT_EXTENSIONS = {
    ".md", ".py", ".ps1", ".sh", ".js", ".jsx", ".ts", ".tsx", ".cs", ".go", ".rs",
    ".json", ".yaml", ".yml", ".toml",
}
TEST_PATH_RE = re.compile(r"(?i)(^|/)(tests?|__tests__|spec)(/|$)|(_test\.|\.test\.|_spec\.|\.spec\.)")
GLOB_CHARS = re.compile(r"[*?\[]")
WORD_BOUNDARY_RE = re.compile(r"[^a-z0-9]+")
TOKEN_SPLIT_RE = re.compile(r"[^A-Za-z0-9]+")
# `buildKnowledgeIndex` and `HTTPServer` both split into their constituent words.
CAMEL_BOUNDARY_RE = re.compile(r"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")
MIN_KEYWORD_LENGTH = 3
# Singular nouns that merely happen to end in `s`. Without this, `alias` would normalize to
# `alia` and stop matching `aliases`. The `ss`/`us`/`is` suffix rules below cover the rest.
IRREGULAR_SINGULARS = {
    "alias", "atlas", "bias", "canvas", "gas", "lens", "news", "series", "species",
}
# Deterministic retrieval weights, strongest signal first. A keyword scores once per evidence
# class per query term, so relevance comes from *where* a term appears, never from how often or
# how many fields of the same class repeat it (ten matching headings score like one). Identical
# for every agent.
WEIGHT_EXACT = 100  # the whole query is this node's id or title
WEIGHT_ALIAS_PHRASE = 14  # a declared alias appears verbatim in the query
WEIGHT_ID = 12
WEIGHT_TITLE = 10
WEIGHT_ALIAS = 8
WEIGHT_HEADING = 6
WEIGHT_METADATA = 3  # path, appliesTo, source_of_truth
WEIGHT_BODY = 1
MAX_EVIDENCE_HEADINGS = 3
EXCLUDED_DIRS = {
    ".git", ".devspark.work", ".archive", ".devspark", "node_modules", "__pycache__",
    ".venv", "venv", "dist", "build", "bin", "obj",
}


def repository_root() -> Path:
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"], check=False, capture_output=True, text=True, encoding="utf-8"
    )
    return Path(result.stdout.strip()) if result.returncode == 0 else Path.cwd().resolve()


def relative(repo: Path, path: Path) -> str:
    return path.relative_to(repo).as_posix()


def repo_relative_path(repo: Path, value: str) -> str:
    """Forward-slash, repo-relative form of a user-supplied path (absolute paths are made relative)."""
    candidate = Path(value)
    if candidate.is_absolute():
        try:
            candidate = candidate.resolve().relative_to(repo.resolve())
        except ValueError:
            return candidate.as_posix()
    text = candidate.as_posix()
    return text[2:] if text.startswith("./") else text


def normalize_token(token: str) -> str:
    """Fold a token to its singular form. Inflectional number only -- deliberately not a stemmer.

    `findings` and `finding` are the same word and must match. `plan` and `planner`, `run` and
    `runtime`, `config` and `configuration` are different words: relating those is a vocabulary
    decision an author makes with an alias, not something retrieval may infer.
    """
    if len(token) <= MIN_KEYWORD_LENGTH or token in IRREGULAR_SINGULARS:
        return token
    if token.endswith("ies") and len(token) > 4:
        return token[:-3] + "y"
    if token.endswith(("ses", "xes", "zes", "ches", "shes")) and len(token) > 4:
        return token[:-2]
    if token.endswith(("ss", "us", "is")):
        return token
    if token.endswith("s"):
        return token[:-1]
    return token


def tokenize_text(text: str) -> frozenset[str]:
    """Every matchable token in a haystack, normalized.

    Splits on punctuation and case boundaries so `scripts/build_knowledge_index.py` yields
    `build`, `knowledge`, `index`, `py`. Because tokens are whole words, `system` no longer
    matches `subsystem` the way substring containment did.
    """
    tokens: set[str] = set()
    for chunk in TOKEN_SPLIT_RE.split(text):
        if not chunk:
            continue
        tokens.add(normalize_token(chunk.lower()))
        for part in CAMEL_BOUNDARY_RE.sub(" ", chunk).split():
            tokens.add(normalize_token(part.lower()))
    tokens.discard("")
    return frozenset(tokens)


@lru_cache(maxsize=2048)
def _cached_tokens(text: str) -> frozenset[str]:
    return tokenize_text(text)


# Stopwords are matched after normalization, so `does` must be excluded as `doe` too.
NORMALIZED_STOPWORDS = STOPWORDS | {normalize_token(word) for word in STOPWORDS}


def tokenize(topic: str) -> list[str]:
    tokens = tokenize_text(topic)
    keywords = sorted(
        token
        for token in tokens
        if len(token) >= MIN_KEYWORD_LENGTH
        and token[0].isalpha()
        and token not in NORMALIZED_STOPWORDS
    )
    return keywords or sorted(token for token in tokens if token[0].isalpha())


def index_is_fresh(repo: Path, index: dict[str, Any]) -> bool:
    output = repo / ".knowledge/index.json"
    coverage_output = repo / ".knowledge/ontology/coverage.json"
    problems = [
        _drift(output, index),
        _drift(coverage_output, build_coverage_report(index)),
    ]
    return not any(problems)


def node_haystack(node: dict[str, Any]) -> str:
    parts = [str(node.get("id", "")), str(node.get("title", "")), str(node.get("path", ""))]
    parts.extend(str(item) for item in node.get("appliesTo", []) or [])
    parts.extend(_source_path(item) for item in node.get("source_of_truth", []) or [])
    return " ".join(parts).lower()


def keyword_hits(haystack: str, keywords: list[str]) -> list[str]:
    """Keywords present in a field as whole tokens. Presence, never occurrence count -- a long
    document must not outrank a precise one by repeating a term."""
    tokens = _cached_tokens(haystack)
    return [kw for kw in keywords if normalize_token(kw) in tokens]


def _source_path(claim: Any) -> str:
    return str(claim.get("path", "")) if isinstance(claim, dict) else str(claim)


def _normalize_phrase(text: str) -> str:
    return WORD_BOUNDARY_RE.sub(" ", text.lower()).strip()


def score_node(
    node: dict[str, Any], keywords: list[str], topic: str = "", body_text: str = ""
) -> tuple[int, list[str]]:
    """Deterministic weighted relevance for one knowledge node, with its own match evidence.

    Field precedence is fixed and model-independent: exact id/title, then aliases, headings,
    source/path metadata, and finally body content. See WEIGHT_* above. Each evidence class
    contributes its weight at most once per query term: a term repeated across ten headings, or
    across ten `source_of_truth` entries, scores exactly like one -- evidence richness improves
    `matched_on` explainability, it never multiplies relevance.
    """
    score = 0
    matched_on: list[str] = []
    node_id = str(node.get("id", ""))
    title = str(node.get("title", ""))
    normalized_topic = _normalize_phrase(topic)

    if normalized_topic and normalized_topic in {
        _normalize_phrase(node_id),
        _normalize_phrase(title),
    }:
        score += WEIGHT_EXACT
        matched_on.append(f"exact: {title or node_id}")

    for label, value, weight in (
        ("id", node_id, WEIGHT_ID),
        ("title", title, WEIGHT_TITLE),
    ):
        hits = keyword_hits(value.lower(), keywords)
        if hits:
            score += weight * len(hits)
            matched_on.append(f"{label}: {value}")

    alias_phrase_matched = False
    alias_term_hits: set[str] = set()
    for alias in node.get("aliases", []) or []:
        alias_text = str(alias)
        if normalized_topic and _normalize_phrase(alias_text) in normalized_topic:
            alias_phrase_matched = True
            matched_on.append(f"alias: {alias_text}")
            continue
        hits = keyword_hits(alias_text.lower(), keywords)
        if hits:
            alias_term_hits.update(hits)
            matched_on.append(f"alias: {alias_text}")
    if alias_phrase_matched:
        score += WEIGHT_ALIAS_PHRASE
    if alias_term_hits:
        score += WEIGHT_ALIAS * len(alias_term_hits)

    heading_term_hits: set[str] = set()
    heading_evidence_count = 0
    for heading in node.get("headings", []) or []:
        hits = keyword_hits(str(heading).lower(), keywords)
        if not hits:
            continue
        heading_term_hits.update(hits)
        if heading_evidence_count < MAX_EVIDENCE_HEADINGS:
            matched_on.append(f"heading: {heading}")
        heading_evidence_count += 1
    if heading_term_hits:
        score += WEIGHT_HEADING * len(heading_term_hits)

    metadata_values = [("path", str(node.get("path", "")))]
    metadata_values += [("appliesTo", str(item)) for item in node.get("appliesTo", []) or []]
    metadata_values += [
        ("source_of_truth", _source_path(item)) for item in node.get("source_of_truth", []) or []
    ]
    metadata_term_hits: set[str] = set()
    for label, value in metadata_values:
        hits = keyword_hits(value.lower(), keywords)
        if hits:
            metadata_term_hits.update(hits)
            matched_on.append(f"{label}: {value}")
    if metadata_term_hits:
        score += WEIGHT_METADATA * len(metadata_term_hits)

    body_hits = sorted(set(keyword_hits(body_text, keywords)))
    if body_hits:
        score += WEIGHT_BODY * len(body_hits)
        matched_on.append(f"body: {', '.join(body_hits)}")

    return score, matched_on


def load_body_text(repo: Path, nodes: list[dict[str, Any]]) -> dict[str, str]:
    """Normalized body text for every indexed document, rebuilt from the Markdown on each run.

    Reading the corpus costs well under a second, while committing the same text would inflate
    `index.json` roughly tenfold and dirty it on every documentation edit. The files on disk stay
    the single source of truth, so this can never go stale.
    """
    texts: dict[str, str] = {}
    for node in nodes:
        path = repo / str(node.get("path", ""))
        if not path.is_file():
            continue
        try:
            _, body = parse_frontmatter(path)
        except Exception:
            # A malformed document is the index build's problem to report, not retrieval's.
            continue
        texts[str(node.get("id", ""))] = normalize_search_text(body)
    return texts


def match_knowledge_nodes(
    index: dict[str, Any],
    keywords: list[str],
    limit: int,
    topic: str = "",
    body_text: dict[str, str] | None = None,
) -> list[dict[str, Any]]:
    body_text = body_text or {}
    scored: list[dict[str, Any]] = []
    for node in index.get("nodes", []):
        node_score, matched_on = score_node(
            node, keywords, topic, body_text.get(str(node.get("id", "")), "")
        )
        if node_score <= 0:
            continue
        entry = dict(node)
        entry["score"] = node_score
        entry["matched_on"] = matched_on
        scored.append(entry)
    scored.sort(key=lambda entry: (-entry["score"], entry["id"]))
    return scored[:limit]


def layer_nodes_by_entity(index: dict[str, Any]) -> dict[str, list[dict[str, Any]]]:
    grouped: dict[str, list[dict[str, Any]]] = {}
    for node in index.get("nodes", []):
        entity_id = str(node.get("entity", ""))
        if entity_id:
            grouped.setdefault(entity_id, []).append(node)
    return grouped


def entity_haystack(entity: dict[str, Any], layer_nodes: list[dict[str, Any]]) -> str:
    """Meaning-bearing text for an entity: its id plus its layers' titles, aliases, and headings.

    Storage location and ownership are deliberately excluded. Every entity lives beneath
    `.knowledge/entities/`, so scoring `path` lets one generic corpus word ("knowledge") match
    every entity at once; scoring `owner` lets a team name do the same. Relation targets are
    excluded too -- adjacency is Context Projection's job, not lexical relevance's.
    """
    entity_id = str(entity.get("id", ""))
    parts = [entity_id, _normalize_phrase(entity_id)]
    for node in layer_nodes:
        parts.append(str(node.get("title", "")))
        parts.extend(str(alias) for alias in node.get("aliases", []) or [])
        parts.extend(str(heading) for heading in node.get("headings", []) or [])
    return " ".join(parts).lower()


def match_entities(index: dict[str, Any], keywords: list[str], limit: int) -> list[dict[str, Any]]:
    grouped = layer_nodes_by_entity(index)
    scored = []
    for entity in index.get("entities", []):
        haystack = entity_haystack(entity, grouped.get(str(entity.get("id", "")), []))
        entity_score = len(keyword_hits(haystack, keywords))
        if entity_score > 0:
            scored.append((entity_score, entity))
    scored.sort(key=lambda pair: (-pair[0], pair[1]["id"]))
    return [entity for _, entity in scored[:limit]]


def _source_exists(repo: Path, source: str) -> bool:
    """`appliesTo` entries are globs, which never resolve as a literal path."""
    if GLOB_CHARS.search(source):
        try:
            return any(repo.glob(source))
        except (ValueError, OSError):
            return False
    return (repo / source).exists()


def _claim_states_for_node(repo: Path, node: dict[str, Any]) -> tuple[list[dict[str, Any]], str | None]:
    """Return (claim_states, error) for a matched node's own object-form source claims, if any.

    Re-reads the document's own frontmatter directly (rather than the flattened index entry) since
    an object claim's `digest`/`baseline`/`profile` fields aren't part of the index's node shape.
    """
    node_path = node.get("path")
    if not node_path:
        return [], None
    doc_path = repo / node_path
    if not doc_path.is_file():
        return [], None
    try:
        metadata, _ = parse_frontmatter(doc_path)
    except (OSError, ValueError):
        return [], None
    if not any(isinstance(item, dict) for item in metadata.get("source_of_truth") or []):
        return [], None
    try:
        return enumerate_claim_states(repo, doc_path, metadata), None
    except (CanonicalizationError, ObjectClaimError) as exc:
        return [], str(exc)


def enrich_node(repo: Path, node: dict[str, Any], entity_lookup: dict[str, dict[str, Any]]) -> dict[str, Any]:
    entry = dict(node)
    sources = node.get("source_of_truth") or node.get("appliesTo") or []
    entry["source_status"] = [
        {
            "path": source,
            "exists": _source_exists(repo, source),
            "last_commit_date": _git_last_commit_date(repo, source),
        }
        for source in sources
    ]
    claim_states, claim_error = _claim_states_for_node(repo, node)
    entry["claim_states"] = claim_states
    if claim_error:
        entry["claim_states_error"] = claim_error
    if (node.get("form") or node.get("type")) == "governance-decision":
        metadata, _ = parse_frontmatter(repo / node["path"])
        constrains = [str(c) for c in (metadata.get("constrains") or [])]
        entry["constrains"] = constrains
        entry["constrained_by_reciprocal"] = {
            entity_id: node["id"] in entity_lookup.get(entity_id, {}).get("constrained_by", [])
            for entity_id in constrains
        }
    entity_id = node.get("entity")
    if entity_id and entity_id in entity_lookup:
        entity = entity_lookup[entity_id]
        entry["entity_missing_layers"] = entity.get("missing_layers", [])
        entry["entity_constrained_by"] = entity.get("constrained_by", [])
    return entry


def bounded_files(root: Path, limit: int) -> list[Path]:
    if not root.is_dir():
        return []
    files = [
        path
        for path in root.rglob("*")
        if path.is_file() and not any(part in EXCLUDED_DIRS for part in path.parts)
    ]
    return sorted(files, key=lambda path: path.as_posix())[:limit]


def grep_code_hits(repo: Path, keywords: list[str], limit: int) -> list[dict[str, Any]]:
    if not keywords:
        return []
    pattern = re.compile("|".join(re.escape(kw) for kw in keywords), re.IGNORECASE)
    hits: list[dict[str, Any]] = []
    for path in bounded_files(repo, limit * 20):
        if path.suffix.lower() not in TEXT_EXTENSIONS:
            continue
        try:
            lines = path.read_text(encoding="utf-8").splitlines()
        except (OSError, UnicodeDecodeError):
            continue
        rel = relative(repo, path)
        for number, line in enumerate(lines, start=1):
            if pattern.search(line):
                hits.append({
                    "file": rel,
                    "line": number,
                    "text": line.strip()[:240],
                    "is_test": bool(TEST_PATH_RE.search(rel)),
                })
                if len(hits) >= limit:
                    return hits
    return hits


def _retry_required(repo: Path, topic: str, keywords: list[str], changed: list[str]) -> int:
    """Discard combined output: an input changed or could not be verified, so the run must be repeated."""
    payload = {
        "REPO_ROOT": str(repo),
        "TOPIC": topic,
        "KEYWORDS": keywords,
        "RETRY_REQUIRED": True,
        "CHANGED_INPUTS": changed,
    }
    print(json.dumps(payload, separators=(",", ":")))
    return 3


def _projection_module():
    spec = importlib.util.spec_from_file_location(
        "context_projection", Path(__file__).resolve().parent / "context-projection.py"
    )
    module = importlib.util.module_from_spec(spec)
    assert spec.loader is not None
    spec.loader.exec_module(module)
    return module


def knowledge_snapshot(repo: Path) -> dict[str, str]:
    return {**knowledge_base_inventory(repo), **knowledge_source_inventory(repo)}


def project_verified(
    repo: Path,
    index: dict[str, Any],
    before: dict[str, str],
    seeds: list[str],
    max_hops: int | None,
) -> tuple[dict[str, Any] | None, list[str]]:
    """Project over the already-built index only while its inputs are proven unchanged.

    Returns `(projection, [])` when the Knowledge inputs, cited sources and Git history identity
    match their state before the build and again just before publication, otherwise
    `(None, changed_inputs)`. Content digests decide this, never timestamps. Nothing is stored.
    """
    try:
        after = knowledge_snapshot(repo)
        changed = inventory_changes(before, after)
        if changed:
            return None, changed
        module = _projection_module()
        hops = module.DEFAULT_MAX_HOPS if max_hops is None else max(0, max_hops)
        projection = module.project_context(index, seeds, max_hops=hops)
        changed = inventory_changes(after, knowledge_snapshot(repo))
    except InputsUncertain as exc:
        return None, [f"uncertain: {exc}"]
    return (None, changed) if changed else (projection, [])


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("topic", nargs="*", help="Free-text topic/question, e.g. 'how is auth done'")
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--match-limit", type=int, default=8)
    parser.add_argument("--hit-limit", type=int, default=40)
    parser.add_argument(
        "--rotate-claim",
        metavar="DOC_PATH",
        help=(
            "Apply a confirmed baseline rotation for one source claim in DOC_PATH (repo-relative). "
            "Requires --claim-path and --claim-profile. Never call this without first showing the "
            "claim's diff (via a topic match's claim_states) and getting explicit human confirmation."
        ),
    )
    parser.add_argument("--claim-path", metavar="PATH")
    parser.add_argument("--claim-profile", metavar="PROFILE")
    parser.add_argument("--claim-region", metavar="REGION")
    parser.add_argument(
        "--files",
        nargs="+",
        metavar="PATH",
        help="Files mode: report which Knowledge nodes apply to or cite each file, and which files are unmapped.",
    )
    parser.add_argument(
        "--project",
        action="store_true",
        help=(
            "Also project the matched ids across accepted relationships, reusing this run's one verified "
            "build. If any input changed during the run, emit RETRY_REQUIRED instead of combined output."
        ),
    )
    parser.add_argument("--max-hops", type=int, default=None, help="Hop budget for --project (default: the projection default).")
    args, _ = parser.parse_known_args()

    repo = repository_root()

    if args.rotate_claim:
        if not args.claim_path or not args.claim_profile:
            print(
                json.dumps({"ERROR": "--rotate-claim requires --claim-path and --claim-profile"}),
            )
            return 2
        try:
            result = rotate_claim(
                repo, repo / args.rotate_claim, args.claim_path, args.claim_profile, args.claim_region
            )
        except (CanonicalizationError, ObjectClaimError, OSError) as exc:
            print(json.dumps({"ERROR": str(exc)}))
            return 2
        print(json.dumps(result, separators=(",", ":")))
        return 0

    topic = " ".join(args.topic).strip()
    keywords = tokenize(topic)
    before: dict[str, str] | None = None
    if args.project and not args.files:
        try:
            before = knowledge_snapshot(repo)
        except InputsUncertain as exc:
            return _retry_required(repo, topic, keywords, [f"uncertain: {exc}"])
    try:
        index = build_index(repo)
    except ValueError as exc:
        payload = {
            "REPO_ROOT": str(repo), "TOPIC": topic, "KEYWORDS": keywords,
            "INDEX_FRESH": False, "ERROR": str(exc),
        }
        print(json.dumps(payload, separators=(",", ":")))
        return 1

    fresh = index_is_fresh(repo, index)
    if args.files:
        located = match_file_owners(index, [repo_relative_path(repo, item) for item in args.files])
        payload = {
            "REPO_ROOT": str(repo),
            "INDEX_FRESH": fresh,
            "FILES": located["files"],
            "UNMAPPED_FILES": located["unmapped_files"],
            "SUMMARY": {
                "file_count": len(located["files"]),
                "mapped_file_count": len(located["files"]) - len(located["unmapped_files"]),
                "unmapped_file_count": len(located["unmapped_files"]),
            },
        }
        print(json.dumps(payload, separators=(",", ":")))
        return 0
    entity_lookup = {entity["id"]: entity for entity in index.get("entities", [])}
    body_text = load_body_text(repo, index.get("nodes", []))
    matched_nodes = [
        enrich_node(repo, node, entity_lookup)
        for node in match_knowledge_nodes(index, keywords, args.match_limit, topic, body_text)
    ]
    matched_entities = match_entities(index, keywords, args.match_limit)
    code_hits = grep_code_hits(repo, keywords, args.hit_limit)

    payload = {
        "REPO_ROOT": str(repo),
        "TOPIC": topic,
        "KEYWORDS": keywords,
        "INDEX_FRESH": fresh,
        "MATCHED_KNOWLEDGE": matched_nodes,
        "MATCHED_ENTITIES": matched_entities,
        "CANDIDATE_CODE_HITS": code_hits,
        "SUMMARY": {
            "matched_knowledge_count": len(matched_nodes),
            "matched_entity_count": len(matched_entities),
            "code_hit_count": len(code_hits),
            "test_hit_count": sum(1 for hit in code_hits if hit["is_test"]),
        },
    }
    if args.project and before is not None:
        seeds = [str(entity["id"]) for entity in matched_entities] + [str(node["id"]) for node in matched_nodes]
        projection, changed = project_verified(repo, index, before, seeds, args.max_hops)
        if projection is None:
            return _retry_required(repo, topic, keywords, changed)
        payload["PROJECTION"] = projection
        payload["INDEX_BUILDS"] = 1
    print(json.dumps(payload, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

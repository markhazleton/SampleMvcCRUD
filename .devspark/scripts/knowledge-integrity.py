#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Knowledge integrity validation (DevSpark 7.6): one repository, one coherent `.knowledge`.

Answers "does every DevSpark knowledge consumer in this repository operate against one coherent
interpretation of `.knowledge`?" without turning `.knowledge` into agent memory or adding
retrieval infrastructure. Every finding uses the shared schema in `scripts/knowledge_findings.py`
so `/devspark-site-audit --scope=knowledge` (KNOW7/KNOW8/KNOW9) and CI can consume the same
structure. Never mutates `.knowledge` or any generated artifact; `--verify-sync` is read-only.

Checks implemented (see `.knowledge/governance/devspark-philosophy.md` for the full model):
  A. Engine divergence   -- two installed copies of the knowledge-index implementation disagree.
  B. Artifact consistency -- index.json/coverage.json stale, missing, or orphaned.
  C. Taxonomy consistency -- explicit frontmatter `type` disagrees with the taxonomy registry.
  D. Mapping integrity    -- broken build (bad source_of_truth/appliesTo) and dangling references.
  E. Root reachability    -- a `.knowledge` directory exists on disk but no indexed node sees it.
  F. Schema/tooling       -- taxonomy-registry.json names a nodeType the schema does not allow.

Exit codes: 0 = no hard structural failure, 1 = hard structural failure present, 2 = usage error.
Hard failures (engine-divergence, generated-artifact-drift, dangling-reference,
unreachable-knowledge-root at severity=high) are mechanical and CI-safe to gate on; every other
category is advisory and must never become a mandatory build gate.
"""

from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
import subprocess
import sys
from pathlib import Path
from typing import Any

sys.path.insert(0, str(Path(__file__).resolve().parent))
from knowledge_findings import is_hard_failure, make_finding  # noqa: E402

import build_knowledge_index as primary_engine  # noqa: E402

# Known secondary install locations where a copy of the engine might drift from the canonical
# implementation this validator resolves at import time (`primary_engine`, whichever copy sits
# next to this script). `scripts/build_knowledge_index.py` is the pre-`.devspark/` legacy
# repository-root location (the exact drift reported in production: a stale repo-root copy next
# to a newer framework-managed `.devspark/scripts/` copy) and must always be checked regardless
# of which of the two copies this script itself happens to be running from. Order matters only
# for stable evidence ordering, not for correctness.
CANDIDATE_ENGINE_PATHS = (
    "scripts/build_knowledge_index.py",
    ".devspark/scripts/build_knowledge_index.py",
    ".devspark/scripts/python/build_knowledge_index.py",
)


def repository_root() -> Path:
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"], check=False, capture_output=True, text=True, encoding="utf-8"
    )
    return Path(result.stdout.strip()) if result.returncode == 0 else Path.cwd().resolve()


def _hash_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def _load_engine_module(path: Path, module_name: str):
    spec = importlib.util.spec_from_file_location(module_name, path)
    if spec is None or spec.loader is None:
        raise ImportError(f"cannot load engine module from {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def _normalize_index(index: dict[str, Any]) -> dict[str, Any]:
    return {**index, "generated": {"by": index.get("generated", {}).get("by")}}


def check_engine_divergence(repo: Path) -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    primary_path = Path(primary_engine.__file__).resolve()
    for candidate in CANDIDATE_ENGINE_PATHS:
        secondary_path = repo / candidate
        if not secondary_path.is_file() or secondary_path.resolve() == primary_path:
            continue
        primary_hash = _hash_file(primary_path)
        secondary_hash = _hash_file(secondary_path)
        if primary_hash == secondary_hash:
            continue  # identical bytes: same implementation installed twice, not a divergence

        entry_points = {
            str(primary_path.relative_to(repo)) if primary_path.is_relative_to(repo) else str(primary_path): "success",
            candidate: "success",
        }
        primary_result: dict[str, Any] | None = None
        secondary_result: dict[str, Any] | None = None
        try:
            primary_result = _normalize_index(primary_engine.build_index(repo))
        except ValueError as exc:
            entry_points[str(primary_path.relative_to(repo))] = f"error: {exc}"
        try:
            secondary_module = _load_engine_module(secondary_path, "knowledge_integrity_secondary_engine")
            secondary_result = _normalize_index(secondary_module.build_index(repo))
        except Exception as exc:  # noqa: BLE001 - any secondary-engine failure is evidence, not a crash
            entry_points[candidate] = f"error: {exc}"

        contradictory = primary_result is not None and secondary_result is not None and primary_result != secondary_result
        findings.append(
            make_finding(
                category="engine-divergence",
                severity="high",
                confidence="high",
                subject=candidate,
                summary=f"Installed knowledge-index engine at '{candidate}' differs from the canonical implementation.",
                evidence=[
                    {"type": "source-path", "path": str(primary_path.relative_to(repo)) if primary_path.is_relative_to(repo) else str(primary_path), "sha256": primary_hash},
                    {"type": "source-path", "path": candidate, "sha256": secondary_hash},
                    {"type": "entry-point-results", "results": entry_points},
                    {"type": "contradictory-results", "value": contradictory},
                ],
                recommendation="Reconcile installed engines: replace the framework-owned copy from the canonical source, or confirm the divergence is an intentional, reviewed customization.",
                changes_authoritative_truth=False,
                source="knowledge-integrity",
            )
        )
    return findings


def check_artifact_consistency(repo: Path, index: dict[str, Any]) -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    index_path = repo / ".knowledge/index.json"
    coverage_path = repo / ".knowledge/ontology/coverage.json"
    fresh_coverage = primary_engine.build_coverage_report(index)

    for path, fresh_payload, label in (
        (index_path, index, "index.json"),
        (coverage_path, fresh_coverage, "coverage.json"),
    ):
        problem = primary_engine._drift(path, fresh_payload)  # noqa: SLF001 - reuse canonical drift comparator
        if problem is None:
            continue
        missing = not path.exists()
        findings.append(
            make_finding(
                category="generated-artifact-drift",
                severity="high",
                confidence="high",
                subject=label,
                summary=problem,
                evidence=[{"type": "generated-artifact", "path": str(path.relative_to(repo)), "missing": missing}],
                recommendation="Regenerate with: python3 scripts/build_knowledge_index.py --repo-root .",
                changes_authoritative_truth=False,
                source="knowledge-integrity",
            )
        )

    knowledge_root = repo / ".knowledge"
    if coverage_path.exists() and not (knowledge_root.exists() and any(knowledge_root.iterdir())):
        findings.append(
            make_finding(
                category="generated-artifact-drift",
                severity="medium",
                confidence="high",
                subject="coverage.json",
                summary="'.knowledge/ontology/coverage.json' exists with no '.knowledge' source content to have produced it.",
                evidence=[{"type": "generated-artifact", "path": "coverage.json", "orphaned": True}],
                recommendation="Delete the orphaned generated artifact or restore its source '.knowledge' content.",
                changes_authoritative_truth=False,
                source="knowledge-integrity",
                id_parts=("generated-artifact-drift", "coverage.json", "orphaned"),
            )
        )
    return findings


def check_taxonomy_consistency(repo: Path, index: dict[str, Any]) -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    mappings = primary_engine.load_registry(repo)
    for node in index.get("nodes", []):
        rel = str(node.get("path", ""))
        full_path = repo / rel
        if not full_path.is_file():
            continue
        try:
            metadata, _body = primary_engine.parse_frontmatter(full_path)
        except (OSError, ValueError):
            continue
        explicit_type = metadata.get("type")
        if not explicit_type:
            continue  # nothing to disagree with; inferred type is the only signal
        # Only a flat document's role is assigned by the taxonomy. An entity layer's role is its
        # `layer` and a decision's is `governance` -- both structural, so neither can disagree
        # with a path mapping. Comparing them here is what manufactured permanent false mismatches.
        form = str(node.get("form") or primary_engine.infer_form(rel))
        if form != primary_engine.FORM_FLAT:
            continue
        inferred_type, pattern = primary_engine.resolve_role(rel, mappings)
        if str(explicit_type) == inferred_type:
            continue
        # A glob maps a directory's default; declaring a different role for one document inside it
        # is the precedence rule working, not a defect, so it is not worth a standing finding.
        # An exact-path mapping is a statement about *this* file, and two deliberate statements
        # disagreeing is a real contradiction someone has to settle.
        if pattern is None or primary_engine.GLOB_CHARS_RE.search(pattern):
            continue
        findings.append(
            make_finding(
                category="taxonomy-mismatch",
                severity="low",
                confidence="high",
                subject=node["id"],
                summary=f"'{node['id']}' declares role '{explicit_type}' but the taxonomy registry maps its exact path to '{inferred_type}'.",
                evidence=[
                    {"type": "knowledge-node", "id": node["id"], "path": rel},
                    {"type": "explicit-type", "value": str(explicit_type)},
                    {"type": "inferred-type", "value": inferred_type},
                    {"type": "taxonomy-pattern", "value": pattern},
                ],
                recommendation="Two deliberate statements disagree about one file: correct the frontmatter role or the exact-path taxonomy mapping so only one of them stands.",
                changes_authoritative_truth=False,
                source="knowledge-integrity",
            )
        )
    return findings


def check_mapping_integrity(repo: Path, index: dict[str, Any]) -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    path_by_id = {str(node["id"]): str(node.get("path", "")) for node in index.get("nodes", [])}
    for item in index.get("dangling_references", []):
        findings.append(
            make_finding(
                category="dangling-reference",
                severity="high",
                confidence="high",
                subject=f"{item['from']}->{item['to']}",
                summary=f"'{item['from']}' references '{item['to']}', which does not resolve to any current node or entity.",
                evidence=[
                    {"type": "knowledge-node", "id": item["from"], "path": path_by_id.get(item["from"], item["path"])},
                    {"type": "relation", "from": item["from"], "to": item["to"]},
                ],
                recommendation="Author the missing knowledge, or correct/remove the reference.",
                changes_authoritative_truth=False,
                source="knowledge-integrity",
            )
        )
    return findings


def check_build_failure(repo: Path, error: ValueError) -> list[dict[str, Any]]:
    return [
        make_finding(
            category="missing-mapping",
            severity="high",
            confidence="high",
            subject="knowledge-index-build",
            summary=f"'.knowledge' failed to build a valid index: {error}",
            evidence=[{"type": "build-error", "message": str(error)}],
            recommendation="Fix the reported mapping (source_of_truth/appliesTo/relation) before trusting any other knowledge finding.",
            changes_authoritative_truth=False,
            source="knowledge-integrity",
            id_parts=("missing-mapping", str(error)),
        )
    ]


def check_root_reachability(repo: Path, index: dict[str, Any]) -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    excluded = {".git", ".devspark.work", ".archive", ".devspark", "node_modules", "__pycache__", ".venv", "venv"}
    indexed_prefixes = {str(node.get("path", "")).rsplit("/.knowledge/", 1)[0] for node in index.get("nodes", [])}
    for candidate in sorted(repo.rglob(".knowledge"), key=primary_engine.stable_path_sort_key):
        if not candidate.is_dir() or any(part in excluded for part in candidate.relative_to(repo).parts):
            continue
        rel = candidate.relative_to(repo).as_posix()
        if rel == ".knowledge" or rel.startswith("tests/fixtures/"):
            continue  # the canonical root is always reachable by definition; test fixtures are data, not roots
        if not any(p.startswith(rel) for p in indexed_prefixes) and not any(str(n.get("path", "")).startswith(rel + "/") for n in index.get("nodes", [])):
            findings.append(
                make_finding(
                    category="unreachable-knowledge-root",
                    severity="high",
                    confidence="high",
                    subject=rel,
                    summary=f"'{rel}' exists on disk but no indexed knowledge node resolves under it.",
                    evidence=[{"type": "source-path", "path": rel}],
                    recommendation="Confirm whether this root is intentionally scaffolded and, if so, include it in indexing; otherwise remove it.",
                    changes_authoritative_truth=False,
                    source="knowledge-integrity",
                )
            )
    return findings


def check_schema_tooling_contradictions(repo: Path) -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    for mapping in primary_engine.load_registry(repo):
        node_type = mapping.get("nodeType")
        if node_type and node_type not in primary_engine.FLAT_ROLES:
            findings.append(
                make_finding(
                    category="schema-tooling-contradiction",
                    severity="medium",
                    confidence="high",
                    subject=str(mapping.get("pathPattern", node_type)),
                    summary=f"taxonomy-registry.json maps '{mapping.get('pathPattern')}' to nodeType '{node_type}', which is not an assignable document role.",
                    evidence=[{"type": "schema-rule", "path": ".knowledge/taxonomy-registry.json", "nodeType": str(node_type)}],
                    recommendation="Map the pattern to a document role; structural forms such as entity-layer are derived from location and cannot be assigned by the registry.",
                    changes_authoritative_truth=False,
                    source="knowledge-integrity",
                )
            )
    return findings


def run_all_checks(repo: Path) -> dict[str, Any]:
    findings: list[dict[str, Any]] = list(check_engine_divergence(repo))
    findings.extend(check_schema_tooling_contradictions(repo))

    try:
        index = primary_engine.build_index(repo)
    except ValueError as exc:
        findings.extend(check_build_failure(repo, exc))
        return {"index_build_ok": False, "findings": findings}

    findings.extend(check_artifact_consistency(repo, index))
    findings.extend(check_taxonomy_consistency(repo, index))
    findings.extend(check_mapping_integrity(repo, index))
    findings.extend(check_root_reachability(repo, index))
    return {"index_build_ok": True, "findings": findings}


def verify_sync(path_a: Path, path_b: Path) -> dict[str, Any]:
    """Read-only post-operation check for upgrade/quickfix flows: prove a claimed engine
    synchronization actually produced identical files rather than trusting the operation's own
    narrative."""
    if not path_a.is_file() or not path_b.is_file():
        return {"match": False, "error": "one or both paths do not exist", "path_a": str(path_a), "path_b": str(path_b)}
    hash_a, hash_b = _hash_file(path_a), _hash_file(path_b)
    return {"match": hash_a == hash_b, "hash_a": hash_a, "hash_b": hash_b, "path_a": str(path_a), "path_b": str(path_b)}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", default=None)
    parser.add_argument("--json", action="store_true")
    parser.add_argument("--verify-sync", nargs=2, metavar=("PATH_A", "PATH_B"))
    args = parser.parse_args()

    repo = Path(args.repo_root).resolve() if args.repo_root else repository_root()

    if args.verify_sync:
        result = verify_sync(repo / args.verify_sync[0], repo / args.verify_sync[1])
        print(json.dumps(result, separators=(",", ":")))
        return 0 if result.get("match") else 1

    result = run_all_checks(repo)
    hard_failures = [f for f in result["findings"] if is_hard_failure(f)]
    payload = {
        "REPO_ROOT": str(repo),
        "index_build_ok": result["index_build_ok"],
        "FINDINGS": result["findings"],
        "SUMMARY": {
            "total_findings": len(result["findings"]),
            "hard_failures": len(hard_failures),
            "advisory_findings": len(result["findings"]) - len(hard_failures),
        },
    }
    print(json.dumps(payload, separators=(",", ":")))
    return 1 if hard_failures else 0


if __name__ == "__main__":
    raise SystemExit(main())

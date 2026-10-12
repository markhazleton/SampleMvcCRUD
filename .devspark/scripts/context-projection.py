#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Deterministic Context Projection for `/devspark-plan` (DevSpark 7.7, Context Projection v1).

Takes one or more already-resolved lexical seed ids (entity ids or knowledge-node ids, typically
`MATCHED_KNOWLEDGE`/`MATCHED_ENTITIES` from `explain-context.py`) and traverses only the
AUTHORITATIVE relationships already persisted in the generated `.knowledge` index -- entity
`relations[]`, `constrained_by`/`constrains` reciprocity edges, and `links.references` edges --
a small, bounded number of hops. It never performs a second lexical search, never consumes
advisory `discover-knowledge` findings as graph truth, and never invents a relationship the
repository has not authored.

Design boundary (see `.knowledge/governance/devspark-philosophy.md` for the full model):

    Knowledge Discovery  -> proposes possible relationships
    Human / repo author  -> accepts authoritative relationships (constrains/relations/links)
    Context Projection   -> traverses ONLY what has been accepted

Context Projection expands the candidate universe Plan considers; it does not decide final
relevance. The planning model remains responsible for what actually belongs in `context_resolved`.
"""

from __future__ import annotations

import argparse
import json
import statistics
import subprocess
import sys
from pathlib import Path
from typing import Any

sys.path.insert(0, str(Path(__file__).resolve().parent))
from build_knowledge_index import build_index  # noqa: E402

DEFAULT_MAX_HOPS = 2
DEFAULT_MAX_CANDIDATES = 100
# A traversed edge's relation, reported from the OPPOSITE direction of how it was persisted, needs
# a human-meaningful inverse label (Section 5/10) rather than a synthetic "<rel>:reverse" string
# for the two accepted relationship families the spec calls out by name. Any other predicate
# (arbitrary entity `relations[]` ontology terms) falls back to a generic, still-deterministic
# reverse label -- Section 6 explicitly forbids inventing special behavior per relation name.
REL_INVERSE_LABELS = {
    "constrains": "constrained_by",
    "constrained_by": "constrains",
    "references": "referenced_by",
    "referenced_by": "references",
    "has-layer": "layer-of",
    "layer-of": "has-layer",
}
GOVERNANCE_RELATIONS = {"constrains", "constrained_by"}
# Composition edges the builder materializes from the entity model. Kept separate in provenance so
# Plan can tell "this is part of that entity" from "someone authored a dependency between them".
STRUCTURAL_RELATIONS = {"has-layer", "layer-of"}


def repository_root() -> Path:
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"], check=False, capture_output=True, text=True, encoding="utf-8"
    )
    return Path(result.stdout.strip()) if result.returncode == 0 else Path.cwd().resolve()


def _inverse_relation(rel: str) -> str:
    return REL_INVERSE_LABELS.get(rel, f"{rel}:reverse")


def build_adjacency(index: dict[str, Any]) -> tuple[dict[str, list[tuple[str, str]]], set[str], dict[str, dict[str, Any]]]:
    """Undirected-traversable adjacency built ONLY from edges `build_knowledge_index.py` already
    persisted as authoritative (entity relations, constrains/constrained_by reciprocity,
    `links.references`) -- never from advisory discovery output. Both endpoints of an edge must
    already be known nodes/entities; a dangling `links.references` target is not a validated
    relationship and is never traversable (Section 4: never invent a relationship).
    """
    entity_ids = {str(entity["id"]) for entity in index.get("entities", [])}
    node_by_id = {str(node["id"]): node for node in index.get("nodes", [])}
    known_ids = entity_ids | set(node_by_id)

    adjacency: dict[str, list[tuple[str, str]]] = {}
    for edge in index.get("edges", []):
        src, dst, rel = str(edge["from"]), str(edge["to"]), str(edge["rel"])
        if src not in known_ids or dst not in known_ids:
            continue
        adjacency.setdefault(src, []).append((dst, rel))
        adjacency.setdefault(dst, []).append((src, _inverse_relation(rel)))

    for node_id in adjacency:
        adjacency[node_id].sort(key=lambda pair: (pair[1], pair[0]))
    return adjacency, entity_ids, node_by_id


def candidate_type(node_id: str, entity_ids: set[str], node_by_id: dict[str, dict[str, Any]]) -> str:
    if node_id in entity_ids:
        return "entity"
    node = node_by_id.get(node_id)
    # Structural questions read `form`. `type` answers the same question for a decision only by
    # coincidence of the compatibility encoding, and falls back for pre-7.8 index files.
    if node and (node.get("form") or node.get("type")) == "governance-decision":
        return "governance"
    return "current-knowledge"


def project_context(
    index: dict[str, Any],
    seeds: list[str],
    max_hops: int = DEFAULT_MAX_HOPS,
    max_candidates: int | None = DEFAULT_MAX_CANDIDATES,
) -> dict[str, Any]:
    adjacency, entity_ids, node_by_id = build_adjacency(index)
    known_ids = entity_ids | set(node_by_id)

    ordered_unique_seeds = list(dict.fromkeys(seeds))
    valid_seeds = [s for s in ordered_unique_seeds if s in known_ids]
    unresolved_seeds = [s for s in ordered_unique_seeds if s not in known_ids]

    distance: dict[str, int] = {seed: 0 for seed in valid_seeds}
    reasons: dict[str, list[dict[str, Any]]] = {seed: [{"type": "seed"}] for seed in valid_seeds}
    valid_seed_set = set(valid_seeds)

    def seed_of(node_id: str) -> str:
        node_reasons = reasons.get(node_id)
        if node_reasons and node_reasons[0].get("seed"):
            return str(node_reasons[0]["seed"])
        return node_id

    frontier = list(valid_seeds)
    hop = 0
    while frontier and hop < max_hops:
        hop += 1
        next_frontier: list[str] = []
        for node_id in sorted(frontier):
            for neighbor, rel in adjacency.get(node_id, []):
                if rel in GOVERNANCE_RELATIONS:
                    reason_type = "governance_constraint"
                elif rel in STRUCTURAL_RELATIONS:
                    reason_type = "structural_composition"
                else:
                    reason_type = "graph_relation"
                reason = {
                    "type": reason_type,
                    "seed": seed_of(node_id),
                    "via": node_id,
                    "relation": rel,
                    "distance": hop,
                }
                if neighbor not in distance:
                    distance[neighbor] = hop
                    reasons[neighbor] = []
                    next_frontier.append(neighbor)
                if distance[neighbor] == hop:
                    reasons[neighbor].append(reason)
        frontier = next_frontier

    non_seed_ids = sorted(
        (nid for nid in distance if nid not in valid_seed_set),
        key=lambda n: (distance[n], n),
    )

    total_unique_candidates = len(non_seed_ids)
    hop1_candidate_count = sum(1 for nid in non_seed_ids if distance[nid] == 1)
    hop2_candidate_count = sum(1 for nid in non_seed_ids if distance[nid] == 2)

    truncated = bool(max_candidates) and total_unique_candidates > max_candidates
    kept_non_seed_ids = non_seed_ids[:max_candidates] if truncated else non_seed_ids

    all_ids = valid_seeds + kept_non_seed_ids
    candidates = [
        {
            "id": nid,
            "candidate_type": candidate_type(nid, entity_ids, node_by_id),
            "distance": distance[nid],
            "reasons": reasons[nid],
        }
        for nid in all_ids
    ]

    channels: dict[str, list[str]] = {"entity": [], "governance": [], "other": []}
    for item in candidates:
        key = item["candidate_type"] if item["candidate_type"] in ("entity", "governance") else "other"
        channels[key].append(item["id"])

    guardrails = {
        "seed_count": len(valid_seeds),
        "hop1_candidate_count": hop1_candidate_count,
        "hop2_candidate_count": hop2_candidate_count,
        "total_unique_candidates": total_unique_candidates,
        "max_candidates": max_candidates,
        "truncated": truncated,
        "truncation_reason": (
            f"total_unique_candidates ({total_unique_candidates}) exceeds configured max_candidates ({max_candidates})"
            if truncated else None
        ),
        "ordering": "distance-ascending,id-ascending",
    }

    return {
        "SEEDS": valid_seeds,
        "UNRESOLVED_SEEDS": unresolved_seeds,
        "MAX_HOPS": max_hops,
        "CANDIDATES": candidates,
        "CHANNELS": channels,
        "GUARDRAILS": guardrails,
        "SUMMARY": {
            "candidate_count": len(candidates),
            "by_channel": {key: len(value) for key, value in channels.items()},
        },
    }


def audit_fan_out(index: dict[str, Any]) -> dict[str, Any]:
    """Repository-wide fan-out audit (Section 17/24): projects from every entity independently
    (uncapped) so unusually high-degree nodes/hubs can be identified BEFORE Context Projection is
    enabled as default Plan behavior. This is the framework capability; the actual production
    numbers/decision belong to whichever repository runs it (e.g. Triage), not this tool.
    """
    entity_ids = sorted(str(entity["id"]) for entity in index.get("entities", []))
    adjacency, all_entity_ids, node_by_id = build_adjacency(index)
    per_entity: list[dict[str, Any]] = []
    for entity_id in entity_ids:
        result = project_context(index, [entity_id], max_hops=2, max_candidates=None)
        hop1 = result["GUARDRAILS"]["hop1_candidate_count"]
        hop2 = result["GUARDRAILS"]["hop2_candidate_count"]
        governance_count = len(result["CHANNELS"]["governance"])
        entity_count = len(result["CHANNELS"]["entity"])
        # A cycle exists whenever an edge from a reached node points back at an already-visited
        # node (harmless to correctness -- the visited-set BFS already de-duplicates -- but worth
        # surfacing as a fan-out-audit signal).
        visited = {c["id"] for c in result["CANDIDATES"]}
        cycles_detected = any(
            neighbor in visited
            for node_id in visited
            for neighbor, _rel in adjacency.get(node_id, [])
        ) and len(visited) < sum(len(adjacency.get(node_id, [])) for node_id in visited)
        per_entity.append(
            {
                "entity": entity_id,
                "hop1_candidate_count": hop1,
                "hop2_candidate_count": hop2,
                "governance_candidate_count": governance_count,
                "entity_candidate_count": entity_count,
                "cycles_detected": cycles_detected,
            }
        )

    hop1_sizes = [item["hop1_candidate_count"] for item in per_entity]
    hop2_sizes = [item["hop2_candidate_count"] for item in per_entity]
    summary = {
        "entity_count": len(per_entity),
        "median_hop1_size": statistics.median(hop1_sizes) if hop1_sizes else 0,
        "max_hop1_size": max(hop1_sizes) if hop1_sizes else 0,
        "median_hop2_size": statistics.median(hop2_sizes) if hop2_sizes else 0,
        "max_hop2_size": max(hop2_sizes) if hop2_sizes else 0,
        "high_fanout_entities": sorted(
            (item["entity"] for item in per_entity if item["hop2_candidate_count"] >= max(10, len(entity_ids) // 2 or 10)),
        ),
    }
    return {"FAN_OUT_AUDIT": per_entity, "SUMMARY": summary}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", default=None, help="Repository root to scan; defaults to git-toplevel detection.")
    parser.add_argument("--seed", action="append", default=[], help="Seed entity/knowledge-node id; repeatable.")
    parser.add_argument("--max-hops", type=int, default=DEFAULT_MAX_HOPS)
    parser.add_argument(
        "--max-candidates", type=int, default=DEFAULT_MAX_CANDIDATES,
        help="Cap on non-seed candidates; 0 means unlimited.",
    )
    parser.add_argument("--audit-fan-out", action="store_true", help="Repository-wide fan-out audit across every indexed entity (Section 17/24).")
    parser.add_argument("--json", action="store_true")
    args, _ = parser.parse_known_args()

    repo = Path(args.repo_root).resolve() if args.repo_root else repository_root()

    try:
        index = build_index(repo)
    except ValueError as exc:
        print(json.dumps({"REPO_ROOT": str(repo), "ERROR": str(exc)}))
        return 1

    if args.audit_fan_out:
        payload = {"REPO_ROOT": str(repo), **audit_fan_out(index)}
        print(json.dumps(payload, separators=(",", ":")))
        return 0

    if not args.seed:
        print(json.dumps({"REPO_ROOT": str(repo), "ERROR": "--seed is required (repeatable) unless --audit-fan-out is used"}))
        return 2

    max_candidates = args.max_candidates if args.max_candidates and args.max_candidates > 0 else None
    result = project_context(index, args.seed, max_hops=max(0, args.max_hops), max_candidates=max_candidates)
    payload = {"REPO_ROOT": str(repo), **result}
    print(json.dumps(payload, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

#!/usr/bin/env python3
# BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com
"""Canonical `.knowledge` retrieval evaluation harness (DevSpark 7.6.1).

Reuses `/devspark-explain`'s own retrieval engine (`scripts/explain-context.py`) unmodified --
this harness never widens the candidate limit, removes governance/ADR nodes, or special-cases
entity collapsing without saying so. It exists so a retrieval regression fixture measured today
means the same thing when re-measured after a future release, instead of depending on disposable,
subtly-different ad-hoc benchmark scripts.

Canonical definitions (7.6.1 -- see also `.knowledge/governance/devspark-philosophy.md`):
  - raw node: one scored document as returned by `match_knowledge_nodes`, before any entity
    collapsing. Its `id` is a layer-document id (e.g. `conversations-integration`).
  - entity identity: the `entity` attribute an entity-layer raw node carries (e.g.
    `conversations`). An `expected` identity in a fixture matches a raw node either by exact raw
    `id` or by exact `entity` match -- both are recorded as `matched_as` in results.
  - consumer candidate: one of the first `limit` raw nodes, in ranked order. This is EXACTLY what
    a real `/devspark-explain` consumer receives -- no entity collapsing ever adds a candidate
    beyond this boundary.
  - collapsed consumer view: the consumer candidates, deduplicated to one entry per entity. It can
    only ever *reduce* the visible set (fewer, deduplicated entries) -- it never promotes a
    candidate that was not already inside the consumer boundary.
  - full ranking: the complete scored candidate list, unbounded by `limit`. Used only for
    diagnostics (was the right answer present somewhere deeper in the deterministic ranking?).
  - consumer recall@N: computed ONLY from consumer candidates (raw, first N). Never backfilled
    from deeper full-ranking positions after collapsing.
  - full-ranking recall: computed from the complete, unbounded ranking (optionally collapsed).
    This is a diagnostic signal about ranking quality, never a substitute for consumer recall, and
    must never be reported as "Top-N retrieval" on its own.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import subprocess
from pathlib import Path
from typing import Any

import yaml

DEFAULT_LIMIT = 8  # matches /devspark-explain's own --match-limit default
FULL_RANKING_PREVIEW_CAP = 50  # diagnostics only; bounds output size on large corpora


def repository_root() -> Path:
    result = subprocess.run(
        ["git", "rev-parse", "--show-toplevel"], check=False, capture_output=True, text=True, encoding="utf-8"
    )
    return Path(result.stdout.strip()) if result.returncode == 0 else Path.cwd().resolve()


def load_explain_engine():
    path = Path(__file__).resolve().parent / "explain-context.py"
    spec = importlib.util.spec_from_file_location("knowledge_benchmark_explain_engine", path)
    if spec is None or spec.loader is None:
        raise ImportError(f"cannot load retrieval engine from {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def load_fixture(path: Path) -> list[dict[str, Any]]:
    text = path.read_text(encoding="utf-8")
    data = yaml.safe_load(text) if path.suffix in {".yaml", ".yml"} else json.loads(text)
    cases = data.get("cases", []) if isinstance(data, dict) else data
    normalized = []
    for case in cases:
        expected = case["expected"]
        normalized.append(
            {
                "query": case["query"],
                "expected": expected if isinstance(expected, list) else [expected],
                "limit": case.get("limit"),
            }
        )
    return normalized


def _collapse(scored: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Dedupe entity-layer docs owned by the same entity to their best-ranked occurrence.

    `scored` must already be in descending-score order (as `match_knowledge_nodes` returns), so
    "first occurrence wins" is equivalent to "best-ranked occurrence wins". Collapsing NEVER
    changes which raw nodes were in `scored` to begin with -- callers control that boundary by
    slicing before calling this, which is how consumer-visible collapsing stays bounded.
    """
    seen: set[str] = set()
    collapsed: list[dict[str, Any]] = []
    for entry in scored:
        key = str(entry.get("entity") or entry["id"])
        if key in seen:
            continue
        seen.add(key)
        collapsed.append({**entry, "collapsed_id": key, "raw_id": entry["id"]})
    return collapsed


def _match_identity(entry: dict[str, Any], expected_id: str) -> str | None:
    """Recognize both raw layer-document ids and entity identities as valid matches (Fix #2)."""
    if expected_id == entry.get("raw_id", entry["id"]):
        return "raw_node_id"
    entity_val = entry.get("entity") or entry.get("collapsed_id")
    if entity_val is not None and expected_id == str(entity_val):
        return "entity_id"
    return None


def _find_rank(entries: list[dict[str, Any]], expected_id: str) -> tuple[int | None, dict[str, Any] | None, str | None]:
    for position, entry in enumerate(entries, start=1):
        matched_as = _match_identity(entry, expected_id)
        if matched_as:
            return position, entry, matched_as
    return None, None, None


def _candidate_summary(entries: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "id": e.get("raw_id", e["id"]),
            "entity": e.get("entity"),
            "collapsed_id": e.get("collapsed_id"),
            "type": e.get("type"),
            "score": e["score"],
            "matched_on": e.get("matched_on", []),
        }
        for e in entries
    ]


def evaluate_case(
    engine: Any,
    index: dict[str, Any],
    body_text: dict[str, str],
    case: dict[str, Any],
    default_limit: int,
    collapse_entities: bool,
) -> dict[str, Any]:
    limit = case.get("limit") or default_limit
    keywords = engine.tokenize(case["query"])
    node_count = len(index.get("nodes", [])) or 1
    full_raw = engine.match_knowledge_nodes(index, keywords, node_count, case["query"], body_text)

    # Consumer boundary: exactly the first `limit` raw nodes -- the real candidate set a consumer
    # receives. Collapsing is applied ONLY to this bounded slice, so it can never promote a
    # deeper-ranked candidate into visibility (Fix #3).
    consumer_raw = full_raw[:limit]
    consumer_collapsed = _collapse(consumer_raw)
    full_collapsed = _collapse(full_raw)

    per_expected: list[dict[str, Any]] = []
    for expected_id in case["expected"]:
        full_rank, full_entry, matched_as = _find_rank(full_raw, expected_id)
        consumer_rank = full_rank if full_rank is not None and full_rank <= limit else None
        consumer_visible = consumer_rank is not None
        collapsed_full_rank, _cfe, _cfm = _find_rank(full_collapsed, expected_id)
        collapsed_consumer_rank, _cce, _ccm = _find_rank(consumer_collapsed, expected_id)

        per_expected.append(
            {
                "expected_id": expected_id,
                "expected_identity": expected_id,
                "raw_node_id": full_entry["id"] if full_entry else None,
                "entity_id": full_entry.get("entity") if full_entry else None,
                "matched_as": matched_as,
                # Back-compat fields (7.6): now computed with the corrected consumer boundary.
                "rank": full_rank,
                "candidates_above_expected": (full_rank - 1) if full_rank is not None else None,
                "visible_within_limit": consumer_visible,
                "candidate_type": full_entry.get("type") if full_entry else None,
                "score": full_entry.get("score") if full_entry else None,
                "matched_on": full_entry.get("matched_on", []) if full_entry else [],
                # 7.6.1 canonical consumer-visible vs. full-ranking split (never conflated).
                "consumer": {
                    "rank": consumer_rank,
                    "visible": consumer_visible,
                    "collapsed_rank": collapsed_consumer_rank,
                    "collapsed_visible": collapsed_consumer_rank is not None,
                },
                "full_ranking": {
                    "rank": full_rank,
                    "collapsed_rank": collapsed_full_rank,
                },
            }
        )

    result: dict[str, Any] = {
        "query": case["query"],
        "limit_used": limit,
        "expected": per_expected,
        "raw_top_candidates": _candidate_summary(consumer_raw),
        "consumer": {
            "raw_candidates": _candidate_summary(consumer_raw),
            "collapsed_entities": _candidate_summary(consumer_collapsed),
        },
        "full_ranking": {
            "raw_candidates": _candidate_summary(full_raw[:FULL_RANKING_PREVIEW_CAP]),
            "collapsed_entities": _candidate_summary(full_collapsed[:FULL_RANKING_PREVIEW_CAP]),
            "total_candidate_count": len(full_raw),
        },
    }
    if len(case["expected"]) > 1:
        consumer_relevant = sum(1 for item in per_expected if item["consumer"]["visible"])
        full_relevant = sum(1 for item in per_expected if item["full_ranking"]["collapsed_rank"] is not None)
        result["recall"] = {
            "relevant_expected": len(case["expected"]),
            "relevant_returned": consumer_relevant,
            "recall_pct": round(100 * consumer_relevant / len(case["expected"]), 2),
        }
        result["consumer_recall"] = dict(result["recall"])
        result["full_ranking_recall"] = {
            "relevant_expected": len(case["expected"]),
            "relevant_returned": full_relevant,
            "recall_pct": round(100 * full_relevant / len(case["expected"]), 2),
        }
    return result


def evaluate_fixture(
    repo: Path, cases: list[dict[str, Any]], default_limit: int = DEFAULT_LIMIT, collapse_entities: bool = False
) -> dict[str, Any]:
    engine = load_explain_engine()
    index = engine.build_index(repo)
    body_text = engine.load_body_text(repo, index.get("nodes", []))
    case_results = [evaluate_case(engine, index, body_text, case, default_limit, collapse_entities) for case in cases]

    single_concept = [c for c in case_results if len(c["expected"]) == 1]
    total_single = len(single_concept) or 1
    consumer_rank1 = sum(1 for c in single_concept if c["expected"][0]["consumer"]["rank"] == 1)
    consumer_top3 = sum(
        1 for c in single_concept if (r := c["expected"][0]["consumer"]["rank"]) is not None and r <= 3
    )
    consumer_topn = sum(1 for c in single_concept if c["expected"][0]["consumer"]["visible"])
    full_collapsed_topn = sum(
        1 for c in single_concept if c["expected"][0]["full_ranking"]["collapsed_rank"] is not None
    )

    multi_concept = [c for c in case_results if len(c["expected"]) > 1]
    mean_recall = (
        round(sum(c["recall"]["recall_pct"] for c in multi_concept) / len(multi_concept), 2) if multi_concept else None
    )
    mean_full_recall = (
        round(sum(c["full_ranking_recall"]["recall_pct"] for c in multi_concept) / len(multi_concept), 2)
        if multi_concept
        else None
    )

    return {
        "default_limit": default_limit,
        "entity_collapse": collapse_entities,
        "cases": case_results,
        "summary": {
            "single_concept_case_count": len(single_concept),
            # Back-compat names (7.6): retained, now computed strictly from consumer-visible
            # candidates -- no backfill from deeper full-ranking positions after collapsing.
            "rank1_rate_pct": round(100 * consumer_rank1 / total_single, 2),
            "top3_rate_pct": round(100 * consumer_top3 / total_single, 2),
            "topn_rate_pct": round(100 * consumer_topn / total_single, 2),
            "miss_rate_pct": round(100 * (total_single - consumer_topn) / total_single, 2),
            "multi_concept_case_count": len(multi_concept),
            "multi_concept_mean_recall_pct": mean_recall,
            # 7.6.1 canonical names -- prefer these in new tooling/reporting.
            "consumer_rank_1_pct": round(100 * consumer_rank1 / total_single, 2),
            "consumer_top_3_pct": round(100 * consumer_top3 / total_single, 2),
            "consumer_top_8_pct": round(100 * consumer_topn / total_single, 2),
            "consumer_recall_at_limit_pct": round(100 * consumer_topn / total_single, 2),
            "consumer_miss_pct": round(100 * (total_single - consumer_topn) / total_single, 2),
            "consumer_multi_concept_recall_pct": mean_recall,
            "full_ranking_collapsed_topn_pct": round(100 * full_collapsed_topn / total_single, 2),
            "full_ranking_multi_concept_recall_pct": mean_full_recall,
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--fixture", required=True, metavar="PATH")
    parser.add_argument("--limit", type=int, default=DEFAULT_LIMIT)
    parser.add_argument("--collapse-entities", action="store_true")
    parser.add_argument("--repo-root", default=None)
    args, _ = parser.parse_known_args()

    repo = Path(args.repo_root).resolve() if args.repo_root else repository_root()
    cases = load_fixture(Path(args.fixture))
    payload = evaluate_fixture(repo, cases, args.limit, args.collapse_entities)
    payload["REPO_ROOT"] = str(repo)
    print(json.dumps(payload, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

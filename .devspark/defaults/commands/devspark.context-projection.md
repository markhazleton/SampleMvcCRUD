<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

````markdown
---
description: Deterministic Context Projection — traverses ONLY authoritative `.knowledge` relationships (entity relations, constrains/constrained_by, links.references) a small bounded distance from one or more seed ids, for direct inspection/debugging independent of `/devspark-plan`
handoffs:
  - label: Run Knowledge Discovery
    agent: devspark-discover-knowledge
    prompt: A candidate relationship this projection did not reach may not yet be authored — run `/devspark-discover-knowledge` to propose it for human acceptance first
scripts:
  sh: .devspark/scripts/bash/context-projection.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/context-projection.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). Supported forms:

```text
/devspark-context-projection --seed <id> [--seed <id> ...] [--max-hops 2] [--max-candidates 100]
/devspark-context-projection --audit-fan-out
```

## Purpose

This is a direct, standalone way to inspect Context Projection independently of
`/devspark-plan` (Section 16 diagnostic mode) — primarily for testing and debugging the graph
traversal itself before trusting its output inside planning.

**Context Projection is not a second discovery/ranking pass.** Given one or more seed ids
(typically already-resolved lexical hits from `/devspark-explain`'s `MATCHED_KNOWLEDGE` /
`MATCHED_ENTITIES`), it performs a deterministic, bounded breadth-first traversal of relationships
the repository has already **accepted** into `.knowledge` — entity `relations[]`,
`constrained_by`/`constrains` reciprocity edges, and `links.references` — as already persisted by
`build_knowledge_index.py`. It never runs a second lexical search, and it never treats an
unaccepted `/devspark-discover-knowledge` finding (e.g. a `governance-relationship-candidate` or
`missing-relationship`) as a traversable edge — only an authored, index-validated relationship is
reachable.

## Design boundary

```text
Knowledge Discovery   -> proposes possible relationships
Human / repo author   -> accepts them (constrains/relations/links.references)
Context Projection    -> traverses ONLY what has been accepted
```

## Outline

### 1. Initialize

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md`
(installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1.

Run `{SCRIPT}` and parse its JSON output.

### 2. Report

For a normal projection run, present:

- `SEEDS` / `UNRESOLVED_SEEDS` — which seed ids actually resolved to a known node/entity.
- `CANDIDATES` — each with `id`, `candidate_type` (`entity` | `governance` | `current-knowledge`),
  `distance` (hop count), and `reasons` (provenance: `seed`, `via`, `relation`, `distance` for
  each shortest path that reaches it). Each reason carries a `type` — `structural_composition`
  for an entity/layer composition edge, `governance_constraint` for a binding decision, and
  `graph_relation` for an authored architectural relationship — so a consumer can tell "this is
  part of that entity" from "someone authored a dependency between them".
- `CHANNELS` — candidates grouped into `entity` / `governance` / `other`, never forced into one
  competing lexical score (governance and entity candidates are graph-based, not ranked).
- `GUARDRAILS` — `seed_count`, `hop1_candidate_count`, `hop2_candidate_count`,
  `total_unique_candidates`, `max_candidates`, and whether output was `truncated` (with the
  configured limit and ordering rule, never silent).

For `--audit-fan-out`, present the per-entity `FAN_OUT_AUDIT` list and the `SUMMARY` (median/max
1-hop and 2-hop sizes, `high_fanout_entities`) — this identifies graph hubs before Context
Projection is trusted as default `/devspark-plan` behavior (Section 17/24).

### 3. Candidates are not resolved context

What this command returns is **candidate** context: everything structurally reachable from the
seeds within the hop budget. It is deliberately generous so that nothing relevant is unreachable.
It is not a working set, and it is never recorded wholesale as `context_resolved`.

Narrowing candidates to the smallest set the work actually needs is `/devspark-plan`'s job, and
the provenance above exists to support that judgement — a candidate reached only as a sibling
layer, or only through a shared governance hub, is usually reachable rather than needed. Expect
`context_resolved` to be materially smaller than `CANDIDATES`.

### 4. Never author

Context Projection is read-only. It never writes to `.knowledge`, never authors a relationship,
and never promotes any candidate into authoritative truth on its own initiative — that remains a
human/`/devspark-discover-knowledge` decision.
````

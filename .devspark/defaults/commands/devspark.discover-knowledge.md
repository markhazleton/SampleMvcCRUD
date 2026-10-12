<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

````markdown
---
description: Evidence-based `.knowledge` maintenance discovery — proposes gaps, weak mappings, relationship/alias candidates, contradictions, and historical leakage for human review
handoffs:
  - label: Run Knowledge Integrity
    agent: devspark-site-audit
    prompt: Run `/devspark-site-audit --scope=knowledge` to validate structural integrity of `.knowledge` after acting on discovery findings
scripts:
  sh: .devspark/scripts/bash/discover-knowledge.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/discover-knowledge.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). Supported forms:

```text
/devspark-discover-knowledge
/devspark-discover-knowledge <scope>
```

Where `<scope>` is an already-known entity id, or a repository-relative path prefix, to narrow
discovery to one area. Omit `<scope>` for repository-wide discovery.

## Overview

**Purpose**: this command is a `.knowledge` authoring/maintenance aid. It inspects the same
signals `build_knowledge_index.py` already validates (`source_of_truth`, `appliesTo`, entity
`relations`/`constrained_by`, `links.references`, object-claim drift) plus repository code and
test layout, and reports structured, evidence-backed findings for a human (or a chaining agent,
with confirmation) to act on.

**Knowledge maintenance is continuous; knowledge discovery is periodic.** Most `.knowledge`
authoring happens as a byproduct of `/devspark-plan`, `/devspark-implement`, and `/devspark-explain`
already touching a delta — the best time to author knowledge is when the code makes the truth
evident. This command is the periodic backstop for what that continuous path cannot catch by
construction: gaps, weak mappings, and contradictions with no active delta to surface them.

**Discovery proposes, evidence supports, humans approve.** This command never promotes inferred
information into authoritative `.knowledge` on its own initiative. By default it is entirely
**non-mutating** — it only writes to disk when explicitly invoked with `--write` together with one
of the two narrow, mechanical apply operations described below, and even then only after you
approve the specific proposed change.

**This is NOT**: agent memory, automatic documentation generation, historical reconstruction, a
replacement for `/devspark-explain`, or a runtime retrieval mechanism. It never becomes the Plan
Context Resolution engine itself.

### Evidence sources

- Declared `appliesTo` / `source_of_truth` mappings already present in `.knowledge` (from the
  built index) — used to find files with no current knowledge and mappings that are missing,
  too broad, or contested between nodes.
- Entity `aliases`, `headings`, and source-file identifiers — used to find alias candidates.
- Entity co-reference across headings and shared test coverage — used to find relationship
  candidates.
- Pinned `source_of_truth` object-claim status (`evaluate_document` from
  `build_knowledge_index.py`) — used for contradiction findings.
- Document body/heading text — used for historical-leakage findings.

Every finding uses the shared schema defined in `scripts/knowledge_findings.py` (the same schema
`scripts/knowledge-integrity.py` emits), so findings from both tools can be reported, filtered,
and triaged consistently side by side.

### Output categories

Discovery emits findings in these shared-schema categories (see
`.knowledge/governance/devspark-philosophy.md` for the full model):

| Category | What it detects |
|----------|------------------|
| `knowledge-gap` | A cluster of production files with no current knowledge document. |
| `entity-candidate` | A knowledge gap with enough independent signal (size, tests, multi-directory spread) to warrant an entity rather than a flat document. |
| `missing-mapping` | A `links.references` target that does not resolve to any current node or entity. |
| `broad-mapping` | A declared `appliesTo`/`source_of_truth` pattern matching an unusually large number of files. |
| `overlapping-ownership` | Two or more nodes both claim the same file(s) via their mappings. |
| `missing-relationship` | Two entities show co-reference evidence (shared headings, or shared test coverage that is not a broad, many-entity hub fixture) but no recorded relationship. Hub-only co-occurrence (a fixture or helper touched by many unrelated entities) is suppressed and never produces this finding on its own. |
| `governance-relationship-candidate` | An entity's source files overlap a governance decision's `appliesTo` paths, but the entity is not listed in that decision's `constrains`. |
| `alias-candidate` | A term appears repeatedly in an entity's source files but is not a known alias. Generic single-word terms (stopwords like `id`, `data`, `config`) and terms that recur broadly across many unrelated entities (hub terms) are suppressed unless they form a distinctive multi-word phrase. |
| `contradiction` | A document's pinned `source_of_truth` claim was marked verified but the canonical source has since changed (drift), or has pinned claims never confirmed verified. **This is pinned-claim drift detection, not general cross-document semantic contradiction detection** — this command does not compare the prose of two documents against each other. |
| `historical-leakage` | An ephemeral planning identifier (e.g. `FR-###`, `spec-##`, `T###`) appears in a current-knowledge document body, or a section heading reads as historical/background rather than current behavior. This is a deterministic match on identifier syntax and heading text — not a semantic/heuristic scan of body prose. |

Only categories with implemented, evidence-backed detection are ever emitted; this command does
not claim semantic capabilities (e.g. free-text contradiction detection between two documents'
prose) that it does not perform.

### Human approval boundary

Two mechanical, schema-conformant edits can be applied on request, and only with explicit
confirmation:

- `--apply-alias <node-id> --alias-text <text> --write` — adds one alias to one node's frontmatter.
- `--apply-relation-from <entity> --apply-relation-to <entity> --relation <predicate> --write`
  — adds one relation entry to one entity's `_entity.yaml`.

Without `--write`, both operations report exactly what *would* change (dry-run) and write
nothing. Every other finding (knowledge gaps, entity candidates, broad/overlapping mappings,
contradictions, historical leakage) is left for a human or a chaining agent to draft — this
command never authors `.knowledge` content on its own initiative.

## Prerequisites

- A built `.knowledge` index (`scripts/build_knowledge_index.py` must succeed against the repo).

## Outline

### 1. Initialize Discovery Context

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md`
(installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1.

Run `{SCRIPT}` and parse its JSON output. Expected top-level fields:

- `REPO_ROOT` — resolved repository root.
- `SCOPE` — `{"type": "repository"|"entity"|"path", "value": ...}`.
- `FINDINGS` — a flat list of findings (back-compat: `SCOPED_FINDINGS` plus `GLOBAL_FINDINGS` when
  `--include-global` is passed), each conforming to the shared schema (`finding_id`, `category`,
  `severity`, `confidence`, `subject`, `summary`, `evidence`, `recommendation`,
  `changes_authoritative_truth`, `source`).
- `SCOPED_FINDINGS` — findings attributable to the resolved scope.
- `GLOBAL_FINDINGS` — repository-wide findings (e.g. `knowledge-gap`, `entity-candidate`) that
  cannot be attributed to a single `entity` scope; only populated for `entity` scopes, and only
  included in `FINDINGS` when `--include-global` is passed.
- `SUMMARY` — `{"total_findings": N, "by_category": {category: count, ...},
  "entity_unscopable_categories": [...]}`.

Additional CLI flags to narrow or shape output for large repositories:

- `--min-confidence {low|medium|high}` — drop findings below the given confidence floor.
- `--limit-per-category N` — cap each category to its N highest-confidence findings.
- `--include-global` — for `entity` scopes, merge `GLOBAL_FINDINGS` into `FINDINGS` (omitted by
  default so entity-scoped review does not bleed unrelated repository-wide findings).

If the script reports `{"ERROR": ...}`, the `.knowledge` index itself failed to build — report
this as a blocking issue and recommend running `/devspark-site-audit --scope=knowledge` (or
`scripts/knowledge-integrity.py`) first, rather than attempting to interpret partial discovery.

### 2. Report Findings by Category

Group and present findings using `SUMMARY.by_category` as the table of contents. For each
category present, list the findings with their `subject`, `summary`, and `recommendation`. Do
not silently drop empty categories from the report — state plainly which categories produced no
findings for this scope, so absence of a finding is visibly a fact, not an omission.

### 3. Propose, Do Not Apply

For every finding, present the `recommendation` as a proposal requiring human confirmation before
any follow-up action (authoring a document, running `--apply-alias`, running
`--apply-relation-from`/`--apply-relation-to`) is taken. Never chain directly into applying a
change without the user explicitly approving that specific finding.

### 4. Optional Targeted Application

If the user approves a specific alias or relationship finding, apply it with the corresponding
`--write` invocation, then re-run discovery (or `scripts/build_knowledge_index.py --check`) to
confirm the index still builds cleanly.
````

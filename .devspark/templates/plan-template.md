---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
devspark_version: "<version that authored this plan; omit if undeterminable>"
spec_semantic_hash: null
participants:
  owner: human
  planner: ai
  implementer: ai
  reviewer: human
  critic: ai
  scribe: ai
---

# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/.devspark.work/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/devspark-plan` command. See `.devspark.work/templates/commands/plan.md` for the execution workflow.

## Rationale Summary

### Core Problem

[What problem are we solving?]

### Decision Summary

[What was decided and why (1-3 sentences max)?]

### Key Drivers

- [Business driver]
- [Technical constraint]
- [User/operational impact]

### Source Inputs

- [Spec / ticket / discussion reference]
- [System constraints or prior patterns]
- [Relevant data or telemetry insights]

### Tradeoffs Considered

- Option A: [why not chosen]
- Option B: [why not chosen]
- Selected: [why chosen]

### Architectural Impact

- [What changes in system behavior or structure]
- [Backward compatibility considerations]
- [Dependencies introduced or avoided]

### Reviewer Guidance

[What should reviewers focus on?]

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]
**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]
**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]
**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]
**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]
**Project Type**: [e.g., library/cli/web-service/mobile-app/compiler/desktop-app or NEEDS CLARIFICATION]
**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]
**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]
**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Operating Context and Expected Mechanisms

<!--
  Just enough to reason proportionately (KISS / YAGNI). Not a capacity model.
  Expected counts are NOT limits: they set the expected shape of the change.
  If this plan introduces more than expected, explain why under "Deviations".
-->

```yaml
operating_context:
  expected_scale: "[small / bounded / ...]"
  expected_frequency: "[e.g., a few times a week, per request, nightly]"
  execution: operator-driven # operator-driven | automated
  human_review_acceptable: true
expected_new_mechanisms:
  states: 0
  runtime_settings: 0
  endpoints: 0
  persistent_stores: 0
  caches: 0
  background_workers: 0
```

**Deviations**: [Each mechanism beyond the expectation above, as its shape, the current requirement, invariant, or demonstrated constraint that forces it, and the least-complex constraint that still suffices. "None" is a good answer.]

## Expected Change Surface

<!--
  Orientation for Tasks, Analyze, Implement and owner lookup, not an allow-list. A legitimate
  supporting file outside it is explained, not rejected; new architecture or scope returns to Plan.
  Do not store counts (derive them) or a list of Knowledge nodes to update (owner lookup derives it).
-->

```yaml
expected_change_surface:
  - {path: "[file or bounded area]", change: "[add | modify | remove | inspect]", why: "[reason]"}
```

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

## Context Resolution

*The bounded `.knowledge/` working set for this delta, pinned down here so `/devspark-implement` consumes it as already-resolved and never traverses more than one additional hop itself. `/devspark-analyze` validates every entry resolves against the current corpus; `/devspark-critic` judges whether the set is sufficient for the delta.*

This section records **resolved** context, which is a selection from — not a copy of — the
**candidate** context Context Projection returns. Projection is deliberately generous so that
nothing relevant is unreachable; this list is deliberately small. Record an item because the delta
needs it, not because the graph could reach it.

```yaml
context_resolved:
  - id: [knowledge id, e.g. token-service | azure-devops-work-tracking]
    kind: [knowledge | entity | entity-layer | decision]
    via: [relation or match that reached it, e.g. direct | depends-on -> auth-gateway]
    hop: [0|1|2]
    reason: [why this item informs the delta]
```

Flat Knowledge documents are first-class resolved context. A working set made entirely of
`kind: knowledge` entries is valid and complete — never promote a flat document into an entity
just so it can be listed here.

`hop: 0` is a direct lexical match, `hop: 1` is one authoritative relationship away, `hop: 2` is
two. Planning never records a hop greater than 2.

[Leave empty only if this delta touches no existing `.knowledge/` content — state that explicitly rather than omitting the section.]

## Project Structure

### Documentation (this feature)

```text
.devspark.work/specs/[###-feature]/
├── plan.md              # This file (/devspark-plan command output)
├── research.md          # Only if a planning question needed research
├── data-model.md        # Only if persisted/domain shape changes materially
├── quickstart.md        # Only if validation needs a non-routine procedure
├── qa-acceptance.md     # Only if independent acceptance validation is warranted
├── contracts/           # Only if an interface changes materially
├── gates/               # Persisted gate artifacts from analyze/critic/checklist
└── tasks.md             # Phase 2 output (/devspark-tasks command - NOT created by /devspark-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

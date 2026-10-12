---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Non-destructive cross-artifact consistency, coverage, and traceability analysis across spec.md, plan.md, and tasks.md. The closed-world half of planning assurance — proves the planning artifacts are internally sound; /devspark-critic challenges whether that sound plan is wrong about the real system.
handoffs:
  - label: Implement Project
    agent: devspark-implement
    prompt: Start the implementation in phases
  - label: Revise Plan
    agent: devspark-plan
    prompt: Revise plan to address analysis findings
scripts:
  sh: .devspark/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks
  ps: .devspark/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1. Preamble modules: `finding-fix`, `convergence`, `provenance`.

Load and obey the shared assurance contract at `/.devspark/templates/assurance-contract.md` (installed repos) or `templates/assurance-contract.md` (source repos). It is the canonical source for lane ownership, deduplication, finding quality, and verdict discipline. This command implements that contract; it does not restate it.

> **Analyze establishes that the planning artifacts are internally sound. `/devspark-critic` challenges whether that internally sound plan is wrong about the real system it will run in.**

Analyze is the **closed-world** half of planning assurance. Its test is mechanical: if something can be shown wrong solely because the planning artifacts contradict themselves or fail their declared structural contract, Analyze owns it. If establishing it requires knowing how the running system actually behaves, Analyze does not.

Analyze **owns** (assurance-contract.md §2): spec↔plan↔tasks consistency; requirement coverage and requirement↔task traceability; duplicate, contradictory, or unanchored requirements; terminology drift and wording precision; task dependency and ordering validity; missing task anchors; tasks citing requirement ids that do not exist; completed tasks with no supporting linkage; identifier and field names contradicting a declared contract or schema; Rationale Summary completeness and spec→plan Core-Problem drift; `context_resolved` **validity**; artifact and schema conformance; planning bookkeeping correctness.

Analyze does **NOT** own, and MUST NOT emit findings for (assurance-contract.md §4.1):

| Not Analyze's                                                        | Owner               |
| -------------------------------------------------------------------- | ------------------- |
| Whether stated NFR **targets** are achievable                        | `/devspark-critic`  |
| What operational tasks are **missing entirely** (no requirement asked) | `/devspark-critic`  |
| Failure modes, archetype-specific traps, scale, concurrency hazards  | `/devspark-critic`  |
| Whether the baseline system can actually produce a stated condition  | `/devspark-critic`  |
| `context_resolved` **sufficiency** (is the resolved set complete?)    | `/devspark-critic`  |
| Implementation correctness, runtime proof                            | `/devspark-verify`  |
| Diff review, acceptance of the resulting delta                       | `/devspark-pr-review` |

Noticing one of these anyway is normal. Route it under `routed_findings:` per assurance-contract.md §4.2 — never as an Analyze finding. Do not duplicate a concern another capability already owns.

## Operating Constraints

The analysis itself is non-destructive. Do **not** edit `spec.md`, `plan.md`, `tasks.md`, or source files. The only file write allowed is refreshing the persisted gate artifact at `FEATURE_DIR/gates/analyze.md` with the final report from this command.

Read the YAML frontmatter in `spec.md` (or the quickfix record when `FEATURE_SPEC` resolves to `.devspark.work/quickfixes/<branch>.md`) before analyzing. Treat `classification`, `risk_level`, `required_gates`, `depends_on`, and `supersedes` as authoritative metadata.

**Constitution Authority**: The project constitution (`/.knowledge/governance/constitution.md`) is **non-negotiable** within this analysis scope. Constitution conflicts are automatically CRITICAL and require adjustment of the spec, plan, or tasks—not dilution, reinterpretation, or silent ignoring of the principle. If a principle itself needs to change, that must occur in a separate, explicit constitution update outside `/devspark-analyze`.

## Spec Knowledge Consumption

This command consumes spec knowledge by purpose, as defined once in the consumption map in `spec-validation-contract.md` §8. Analyze reads every element, more broadly than Plan, so it can judge Plan against everything authoritative, and writes none.

```yaml
spec_knowledge_consumption:
  command: analyze
  reads:
    requirements: PROJECT
    domain_invariants: CONSTRAIN
    architectural_constraints: CONSTRAIN
    accepted_limitations: CONSTRAIN
    success_criteria: PROJECT
    clarifications: [PROJECT, CONSTRAIN]
    assumptions: [CONSTRAIN, CONTEXT]
    boundaries: CONSTRAIN
    non_goals: CONSTRAIN
    context: CONTEXT
  writes: []
```

When the feature's `knowledge/feature.md` carries `links.spec_sections`, use it to find these sections; otherwise read the spec directly. Missing spec knowledge never blocks the analysis: fall back to the spec and treat absent sections as having no content, so output for a spec that records none of the new elements is unchanged.

## Outline

Multi-app scope and script resolution are defined by the shared preamble contract.

### 1. Initialize Analysis Context

**Planning Review hand-off.** When invoked by `/devspark-planning-review`, the invocation carries an already-resolved `FEATURE_DIR`, a `DEVSPARK_FEATURE` value, and a `PLANNING_REVIEW_RUN_ID`. Use them: do not re-resolve the feature from the branch name, run any script with `DEVSPARK_FEATURE` set to the handed-off value, and stamp the run id into the gate header as `planning_review.run_id`. Run `{SCRIPT}` only when no hand-off was given.

Run `{SCRIPT}` once from repo root and parse JSON for FEATURE_DIR and AVAILABLE_DOCS. Derive absolute paths:

- SPEC = FEATURE_DIR/spec.md (or the quickfix record file when this branch has no spec — see below)
- PLAN = FEATURE_DIR/plan.md (optional)
- TASKS = FEATURE_DIR/tasks.md (optional)

**Tiered artifact requirements** — do NOT abort on missing plan/tasks; degrade gracefully, mirroring `/devspark-critic`'s tiers:

| Artifacts present   | Analysis scope                                                                   | Degradation label |
| ------------------- | -------------------------------------------------------------------------------- | ----------------- |
| quickfix record only (no `spec.md` for this branch) | Record-level consistency: does the TLDR (WHAT/WHY/HOW) agree with the Affected Components and Constitution Compliance table? | `QUICKFIX-ONLY`    |
| spec only           | Spec-level: internal consistency, requirement wording, duplication, terminology drift | `SPEC-ONLY`       |
| spec + plan         | Above + spec↔plan Core-Problem drift, Rationale Summary completeness             | `SPEC+PLAN`       |
| spec + plan + tasks | Full analysis including requirement↔task coverage                                | `FULL`            |

Abort only if `spec.md` is missing AND no quickfix record exists for this branch under `.devspark.work/quickfixes/` (FEATURE_SPEC resolves there automatically — see the shared preamble's script resolution). Print the degradation label in the Executive Summary so partial reviews aren't mistaken for full ones.
For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot").

### 2. Load Artifacts (Progressive Disclosure)

Load only the minimal necessary context from each artifact:

**From spec.md:**

- Overview/Context
- Functional Requirements
- Non-Functional Requirements
- User Stories
- Edge Cases (if present)
- Recorded decisions and boundaries: `## Clarifications`, Success Criteria, Assumptions, Out of Scope, the Minimum Sufficient Change block (non-goals, `explicitly_not_required`, `acceptable_manual_operations`), and statements labelled `Domain invariant:` or `Architectural constraint:` in Requirements and Key Entities

**From plan.md:**

- Architecture/stack choices
- Data Model references
- Phases
- Technical constraints
- `## Context Resolution` (`context_resolved` list of knowledge ids with their `kind`, `via`, and `hop`)

**From tasks.md:**

- Task IDs
- Descriptions
- Phase grouping
- Parallel markers [P]
- Referenced file paths

**From constitution:**

- Load `/.knowledge/governance/constitution.md` for principle validation

### 3. Build Semantic Models

Create internal representations (do not include raw artifacts in output):

- **Requirements inventory**: Each functional + non-functional requirement with a stable key (derive slug based on imperative phrase; e.g., "User can upload file" → `user-can-upload-file`)
- **User story/action inventory**: Discrete user actions with acceptance criteria
- **Task coverage mapping**: Map each task to the requirement(s) it satisfies. **Prefer the explicit `Implements: FR-###` directive** on the task line as authoritative; fall back to keyword/phrase inference only for tasks that carry no directive.
- **Constitution rule set**: Extract principle names and MUST/SHOULD normative statements

### 4. Detection Passes (Token-Efficient Analysis)

Focus on high-signal findings. Limit to 50 findings total; aggregate remainder in overflow summary.

#### A. Duplication Detection

- Identify near-duplicate requirements
- Mark lower-quality phrasing for consolidation

#### B. Ambiguity Detection (wording precision only)

- Flag vague adjectives (fast, scalable, secure, intuitive, robust) lacking measurable criteria
- Flag unresolved placeholders (TODO, TKTK, ???, `<placeholder>`, etc.)
- For each flagged item, state the behavioral intent the wording must satisfy (command-preamble-contract.md §9.1) so the fix *adds* a measurable criterion rather than merely deleting the flagged word — e.g. "'fast' → state the latency/throughput target", not "remove 'fast'". Deleting the vague word to silence the check is a gamed fix.
- **Scope**: this pass evaluates whether the requirement is *worded precisely enough to act on*. Whether the stated target is *achievable in production* is `/devspark-critic`'s responsibility — do not duplicate.

#### C. Underspecification

- Requirements with verbs but missing object or measurable outcome
- User stories missing acceptance criteria alignment
- Tasks referencing files or components not defined in spec/plan

#### D. Constitution Alignment

- Any requirement or plan element conflicting with a MUST principle
- Missing mandated sections or quality gates from constitution

#### E. Coverage Gaps (requirement↔task mapping only)

- Requirements with zero associated tasks
- Tasks with no mapped requirement/story
- **Traceability hallucination**: a task whose `Implements: FR-###` directive references a requirement ID that does not exist in `spec.md` (CRITICAL — the task claims to satisfy an invented requirement)
- **Missing requirement anchor**: a normative statement in `spec.md` that other artifacts must be able to cite, written without a stable `FR-###`/`SC-###` anchor — or an anchor duplicated across two different statements. An uncitable or ambiguous requirement makes every downstream traceability claim unverifiable (HIGH; CRITICAL when a task already cites the ambiguous anchor).
- **Completion bookkeeping**: a task marked complete whose `code_ref`/`knowledge_ref` is still `pending`, empty, or an unexplained `n/a` — the task claims work that no recorded edit supports (HIGH). Likewise a task marked complete whose cited paths do not exist.
- Non-functional requirements not reflected in tasks (e.g., performance, security)
- **Scope**: this pass only checks whether *stated* requirements have *stated* tasks, and whether the task record is internally honest. Whether the spec is *missing* operational tasks (observability, rollback, backups, runbooks) that no requirement called for is `/devspark-critic`'s responsibility.

#### F. Inconsistency

- Terminology drift (same concept named differently across files)
- Data entities referenced in plan but absent in spec (or vice versa)
- **Declared-contract conformance**: an identifier, field name, enum member, status value, or type used in `spec.md`/`plan.md`/`tasks.md` that contradicts the contract the artifacts themselves declare — `data-model.md`, an API contract, a schema, or a controlled vocabulary named in the plan. A field the artifacts both define and then cite under a different name is a closed-world contradiction (HIGH; CRITICAL when a task would be implemented against the wrong name).
- **Task sequencing validity**: a task whose declared prerequisite runs later in the ordering, a declared dependency on a task id that does not exist, a cycle among declared dependencies, or a task marked parallel-safe (`[P]`) that writes a file another parallel task in the same group also writes. Integration or polish tasks ordered before the foundational setup they require, with no dependency note, remain a finding.
- Conflicting requirements (e.g., one requires Next.js while other specifies Vue)
- **Unsupported mechanism**: a task that introduces a state, setting, endpoint, persisted record, cache, dependency, architectural pattern, rollback design, or behavior the Plan does not establish (HIGH; CRITICAL when the task would be implemented against an invented contract). Compare the task against the Plan's decisions and `expected_change_surface` only. A path outside the surface is not a defect by itself: justified support (a helper, a test, a twin script, an affected Knowledge node) is fine; new architecture or scope is not. This is a closed-world consistency check, not a path allowlist or a new analyzer; route the finding to Plan revise.
- **Plan versus recorded spec decisions**: compare the Plan against the recorded spec knowledge, closed-world and report-only. Report (HIGH; CRITICAL when the Plan would build something the spec excluded) a Plan that adds work for a non-goal, boundary, or `explicitly_not_required` item; contradicts a clarification answer; cannot meet a success criterion; treats `CONTEXT` or a future-only consumer as `PROJECT` work, or treats a `CONSTRAIN` element as `PROJECT` work; or omits `PROJECT` knowledge. When two recorded decisions disagree, report the disagreement and name both statements without choosing one. A Plan that stays compatible with a boundary or `CONTEXT` without building it is not a finding, and a Plan that shapes its design to a `CONSTRAIN` element is not a finding.
- **Semantic fidelity**: report a Plan whose mechanism changes what the spec declared, even when every requirement still looks covered — a declared identity, cardinality, relationship (for example a 1:1 source-to-versioned relationship aggregated into one stored object), fidelity, architectural boundary, accepted limitation, acceptable manual operation, or declared recovery behavior (for example the accepted "repeat the same copy" replaced by a new recovery operation, cleanup step, or lifecycle). State the violated spec statement and the Plan element that changes it, and propose no redesign: the mechanism stays Plan-owned. A Plan that preserves the declared behavior, including one that makes the same accepted operation safely repeatable, yields no finding, and an unrelated blocker is not a fidelity finding. A domain invariant or architectural constraint written only in Rationale prose is explanatory, so flag its location as advisory instead of enforcing it.
- **Overview references**: a `links.spec_sections` entry in `knowledge/feature.md` that no longer resolves to a spec heading is an advisory consistency finding; do not repair it.
- **Scope**: these checks are closed-world — they compare the artifacts against each other and against the contracts the artifacts declare. Whether a *correctly declared* contract matches what the running system actually provides is `/devspark-critic`'s responsibility.

#### G. Rationale & Traceability

- Missing or incomplete **Rationale Summary** in any artifact (HIGH)
- **Core-Problem drift** between spec and plan — the plan solves a meaningfully different problem than the spec describes (CRITICAL)
- Major architectural / stack decisions in plan with no recorded tradeoff or alternatives considered (HIGH)
- Decisions in plan that contradict an explicitly stated tradeoff in spec (CRITICAL)

#### H. Cross-Repo Dependencies & External Contracts

- If `depends_on` or `supersedes` is present in spec frontmatter, verify the references are non-empty strings in `repo:identifier` form and that they are reflected in the body narrative.
- If either `depends_on` or `supersedes` is non-empty, verify the spec includes an `### External Contracts` subsection under `## Rationale Summary`.
- For each dependency/contract listed, verify the body includes a verbatim contract snapshot (field names/shapes, event payload keys, headers, or API signature) rather than only a summary.
- Missing or placeholder `External Contracts` content when cross-repo metadata is declared is HIGH (or CRITICAL if it impacts trust boundaries/data integrity).

#### I. Context Resolution Validity

- **Mechanical, hard-stop** — the same class of check already applied to `relations[].object` in the knowledge index. For every entry in plan.md's `## Context Resolution` `context_resolved` list, verify the cited id actually resolves against the current `.knowledge/` corpus. All four kinds resolve equally: a flat document in the type-folders (`kind: knowledge`), an entity under `.knowledge/entities/` (`kind: entity`), one layer document of an entity (`kind: entity-layer`), or a decision under `.knowledge/governance/decisions/` (`kind: decision`).
- A `context_resolved` entry citing an id that does not exist, or a `via` relation that isn't actually present on the cited item, is a **stale or hallucinated reference** — CRITICAL, same severity class as traceability hallucination in §E.
- Check the declared `kind` against what the id actually is, and check that `hop` is 0, 1, or 2 — planning's projection budget is 2 hops, so a recorded hop above 2 is a budget violation (HIGH), not a resolution failure. An entry whose `kind` contradicts the resolved id is MEDIUM: the item is real, its provenance record is wrong.
- **Never treat a flat document as second-class.** A `context_resolved` list containing only `kind: knowledge` entries is valid and complete; failing it for "no entities resolved" is itself a non-conformance.
- **Scope**: this pass only checks that what `/devspark-plan` claims to have resolved actually resolves. Whether the resolved set is *sufficient* for the delta (nothing obviously relevant was skipped) is `/devspark-critic`'s responsibility — do not duplicate.
- If `plan.md` has no `## Context Resolution` section at all (pre-v4 plan), report it as a MEDIUM finding recommending a `/devspark-plan` re-run, not CRITICAL — this preserves backward compatibility with plans authored before this section existed.

#### J. Current Knowledge and Test Work Coverage

- Identify `.knowledge/` nodes whose `appliesTo` patterns cover files expected to change. Take the files from the Plan's `expected_change_surface` and the tasks' paths, and derive the owners with `explain-context.py --files <paths>` (`.devspark/scripts/bash/explain-context.sh` or `.devspark/scripts/powershell/explain-context.ps1` in installed repos) instead of expecting a Knowledge list in the Plan. Applicability is not exclusive ownership: report each match with its relationship.
- Verify `tasks.md` includes explicit updates to affected current knowledge and proportionate tests.
- Flag missing knowledge or test work as a coverage gap. Do not generate feature, requirement, or
  task knowledge nodes; all planning traceability remains temporary.
- When `qa-acceptance.md` exists, check it mechanically: a scenario whose `Trace` does not resolve to a requirement or success criterion in the spec is an untraced scenario; a scenario that introduces a requirement outside the approved scope is a scope finding (the fix is to remove the scenario or amend the spec, never to adopt it silently); and, when acceptance planning is warranted, an externally important success criterion with no scenario is a coverage gap. Whether the scenarios are good or proportionate is Critic's lane.
- If the report's `unimplemented_requirements` is non-empty: MEDIUM finding per requirement id (a
  requirement with no task referencing it, only reported once at least one task document exists —
  mirrors the FR/task coverage pass in §E and is sourced from the temporary planning artifacts,
  never from durable `.knowledge/` documents).
- This pass never blocks: knowledge-document coverage gaps are advisory (fail-soft, per FR-006), not CRITICAL/constitution findings.

### 5. Consolidate and Route

Run this pass over the candidate findings **before** assigning severity.

**Deduplicate across capabilities** (assurance-contract.md §5.1). Discard any candidate whose substantive concern is already explicitly recognized and appropriately handled by the artifacts themselves or by an open `/devspark-critic` finding. Reference it in narrative if useful; never mint a second id to restate it.

**Deduplicate within Analyze** (assurance-contract.md §5.2). One root concern yields one finding. A single terminology drift that shows up in four files is one finding with four locations, not four findings.

**Route what is not Analyze's** (assurance-contract.md §4.2). Move any candidate that requires knowing how the running system actually behaves — NFR achievability, absent operational controls, baseline behavior, `context_resolved` sufficiency, failure modes — into `routed_findings:` with `owner: critic`. It does not count as an Analyze finding and never affects the gate status.

**Apply the quality bar** (assurance-contract.md §6). Every remaining finding must answer: what is wrong; why it is material to *this* change; what evidence in the artifacts supports it; why Analyze owns it; and what change resolves it.

### 6. Severity Assignment

Use this heuristic to prioritize findings:

- **CRITICAL**: Violates constitution MUST, missing core spec artifact, or requirement with zero coverage that blocks baseline functionality
- **HIGH**: Duplicate or conflicting requirement, ambiguous security/performance attribute, untestable acceptance criterion
- **MEDIUM**: Terminology drift, missing non-functional task coverage, underspecified edge case
- **LOW**: Style/wording improvements, minor redundancy not affecting execution order

### 7. Produce Compact Analysis Report

Output a Markdown report with the following structure and write the same content to `FEATURE_DIR/gates/analyze.md` (create `FEATURE_DIR/gates/` if needed, replace the prior analyze gate artifact if it exists):

```yaml
gate: analyze
devspark_version: "<installed version, or `unknown`>"
generated: "<ISO-8601 timestamp of this run>"
planning_review:
  run_id: "<PLANNING_REVIEW_RUN_ID from the hand-off; omit this block when not invoked by /devspark-planning-review>"
status: pass | warn | fail
blocking: true | false
severity: info | warning | error | showstopper
summary: "<concise outcome>"
reviewed_artifacts:
  # One entry per artifact this analysis actually read (spec.md, plan.md, tasks.md).
  # hash = `git hash-object <path>` (falls back to a sha256 of the file if git is unavailable).
  - path: spec.md
    hash: "<git-hash-object-output>"
```

## Specification Analysis Report

| ID          | Category    | Severity | Classification  | Location(s)      | Summary                      | Recommendation                       |
| ----------- | ----------- | -------- | --------------- | ---------------- | ---------------------------- | ------------------------------------ |
| analyze-001 | Duplication | HIGH     | blocking-defect | spec.md:L120-134 | Two similar requirements ... | Merge phrasing; keep clearer version |

(Add one row per finding. Each row's ID is the same `finding_id` the machine-readable `findings:` block carries — one stable identifier, never a second scheme. The persisted artifact includes that `findings:` block in the same fenced YAML as the gate header.)

**Coverage Summary Table:**

| Requirement Key | Has Task? | Task IDs | Notes |
| --------------- | --------- | -------- | ----- |

**Constitution Alignment Issues:** (if any)

**Cross-Repo Dependencies:**

| Reference | Type (`depends_on`/`supersedes`) | Body Coverage? | External Contract Evidence? | Notes |
| --------- | -------------------------------- | -------------- | --------------------------- | ----- |

**Unmapped Tasks:** (if any)

**Metrics:**

- Total Requirements
- Total Tasks
- Coverage % (requirements with >=1 task)
- Ambiguity Count
- Duplication Count
- Critical Issues Count

### 8. Provide Next Actions

At end of report, output a concise Next Actions block:

- If CRITICAL issues exist: Recommend resolving before `/devspark-implement`
- If only LOW/MEDIUM: User may proceed, but provide improvement suggestions
- Provide explicit command suggestions: e.g., "Run /devspark-specify with refinement", "Run /devspark-plan to adjust architecture", "Manually edit tasks.md to add coverage for 'performance-metrics'"

### 9. Offer Remediation

Ask the user: "Would you like me to suggest concrete remediation edits for the top N issues?" (Do NOT apply them automatically.)

**Autonomy override**: if `--auto` (or a standing autonomy instruction) is in effect, skip the ask. Instead, recommend re-running `/devspark-tasks` — its Gate Remediation Merge step (§2a) reads this report's `findings:` block directly, merges it with `/devspark-critic`'s, and appends concrete fix tasks without an extra round-trip through this command.

### 10. Persist Gate Artifact

Before producing the report, compute `reviewed_artifacts`: for each artifact actually read (typically `spec.md`, `plan.md`, `tasks.md`), run `git hash-object <path>` and record the `path`/`hash` pair. This is mechanical -- do not skip it even when findings are empty.

For each supported planning artifact, also call `scripts/judge.py semantic-hash --path <path>`
(installed: `.devspark/scripts/judge.py`) and record an available `sem1:` value as the additive
`semantic_hash` on that reviewed entry. Retain the raw `hash`. If the helper/interpreter is
unavailable or returns no fingerprint, omit `semantic_hash`; never invent one or fail the review
solely for missing optional metadata. Only this newly produced gate is stamped, never an older gate.

After producing the report:

- Ensure `FEATURE_DIR/gates/` exists
- Save the report as `FEATURE_DIR/gates/analyze.md`
- Treat the YAML gate block as authoritative for downstream commands such as `/devspark-tasks`, `/devspark-implement`, `/devspark-create-pr`, and `/devspark-pr-review`
- When rerun, replace the previous `analyze.md` artifact instead of appending duplicate reports
- `reviewed_artifacts` hashes are what let `/devspark-implement`'s gate pre-flight detect a stale review (artifact changed since this gate ran) instead of trusting a point-in-time verdict forever.

## Guidelines

### Context Efficiency

- **Minimal high-signal tokens**: Focus on actionable findings, not exhaustive documentation
- **Progressive disclosure**: Load artifacts incrementally; don't dump all content into analysis
- **Token-efficient output**: Limit findings table to 50 rows; summarize overflow
- **Deterministic results**: Rerunning without changes should produce consistent IDs and counts

### Analysis Guidelines

- **NEVER modify product artifacts outside the gate report**
- **NEVER hallucinate missing sections** (if absent, report them accurately)
- **Prioritize constitution violations** (these are always CRITICAL)
- **Use examples over exhaustive rules** (cite specific instances, not generic patterns)
- **Report zero issues gracefully** (emit success report with coverage statistics)

## Context

{ARGS}

## Shared Review Resolution Contract Output

When emitting findings (review observations, issues, recommendations), structure each entry to include the shared resolution contract fields so downstream tools (/devspark-address-pr-review, telemetry, release) can act on them deterministically:

```yaml
findings:
  - finding_id: <stable-id-unique-within-this-command-output> # e.g., analyze-001, clarify-002
    owner: analyze
    severity: critical | high | medium | low
    classification: blocking-defect | accepted-limitation | deferred-work | learning # closeout-contract.md §4 — independent of severity
    description: <1-3 sentence problem statement>
    intent_cue: <REQUIRED for ambiguity/underspecification findings that cite a vague adjective, placeholder, or missing measurable outcome: one sentence naming what the requirement must express to be actionable, per command-preamble-contract.md §9.1 (e.g. "'fast' must state a measurable latency target"). Empty string otherwise.>
    recommended_action: <machine-actionable next step>
    execution_mode: auto | selective | manual
    status: open # set to `resolved` after remediation
    outcome: "" # populated post-resolution by address-pr-review

# Observations belonging to another assurance capability (assurance-contract.md §4.2).
# These are NOT Analyze findings: they never affect gate status and are addressed by
# re-running the owning capability. Omit the block when empty.
routed_findings:
  - routed_id: analyze-routed-001
    owner: critic | verify | pr-review
    description: <what was noticed>
    rationale: <why that lane owns it>
```

`finding_id` MUST be stable across re-runs when the underlying issue is unchanged. `classification` uses the canonical convergence vocabulary from `closeout-contract.md` §4 and is **independent of `severity`**: a high-severity finding may be an Accepted Limitation, and a medium one that violates a required invariant blocks. A finding that hits a `closeout-contract.md` §2 expansion trigger is a `blocking-defect`; the rest are classified, not promoted. `execution_mode` MUST be one of: `auto` (safe to apply automatically), `selective` (apply with reviewer approval), `manual` (requires human implementation). The `status` and `outcome` fields are written by `/devspark-address-pr-review` (FR-028).

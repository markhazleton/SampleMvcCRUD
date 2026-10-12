---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Execute the implementation planning workflow using the plan template to generate design artifacts.
handoffs:
  - label: Create Tasks
    agent: devspark-tasks
    prompt: Break the plan into tasks
    send: true
  - label: Create Checklist
    agent: devspark-checklist
    prompt: Create a checklist for the following domain...
scripts:
  sh: .devspark/scripts/bash/setup-plan.sh --json
  ps: .devspark/scripts/powershell/setup-plan.ps1 -Json
agent_scripts:
  sh: .devspark/scripts/bash/update-agent-context.sh __AGENT__
  ps: .devspark/scripts/powershell/update-agent-context.ps1 -AgentType __AGENT__
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Workflow Position

**Step 3 of 4** in the authoring chain (`specify → clarify → plan → tasks`).

- **Owns**: technical context (stack, libraries, project structure), Constitution Check gate, question-driven research (only for a material unresolved question), the support artifacts a change justifies (data model, interface contracts, quickstart), per-agent context update, and **context resolution** — the bounded `.knowledge/` traversal for this delta (2 hops maximum from the lexical seeds), pinned down as `context_resolved` so `/devspark-implement` never has to traverse more than one additional hop itself.
- **Does NOT own**: re-litigating WHAT/WHY (spec is authoritative); resolving open functional ambiguities (→ `/devspark-clarify`); the executable task list (→ `/devspark-tasks`); adversarial review (→ `/devspark-critic`, `/devspark-analyze`).
- **Pre-flight**: if the loaded spec still contains `[NEEDS CLARIFICATION: …]` markers, halt and route to `/devspark-clarify`. Do not silently default open questions into planning decisions.

## Definition of Done

Done when: every material planning question is resolved (recorded in `research.md` only when research was needed, with zero `NEEDS CLARIFICATION` markers remaining), every requirement maps to a design element, the mechanism shape, expected change surface and verification strategy are known, the justified support artifacts exist, the agent-context script has run, `plan.md`'s `## Context Resolution` section is populated with the traversal's `context_resolved` entries, and the Constitution Check is re-evaluated post-design with no unresolved violations. This command stops after Phase 1 (step 4) — it does not generate tasks or write code. Chat output: report only the branch, IMPL_PLAN path, and generated artifact list — full design detail lives in the files.

## Constitution Authority

`/.knowledge/governance/constitution.md` is **non-negotiable** for planning. Violations may not be carried forward as `NEEDS CLARIFICATION`; they must be resolved before exiting the Constitution Check gate. Justified deviations require an explicit `## Constitution Waivers` block in `plan.md` citing the principle, deviation, reason, and compensating control.

## Spec Knowledge Consumption

This command consumes spec knowledge by purpose, as defined once in the consumption map in `spec-validation-contract.md` §8. Plan reads every element but writes none to the spec, and keeps no per-plan manifest of what it projected.

```yaml
spec_knowledge_consumption:
  command: plan
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

- **Smallest sufficient projection.** Load the smallest authoritative set of spec knowledge that completely defines the current change. It always includes the requirements, relevant clarification decisions, current boundaries and non-goals, and every applicable domain invariant, architectural constraint, accepted limitation and acceptable manual operation. Plan owns the concrete mapping of spec elements to purposes, decided at planning time from repository evidence.
- **Only `PROJECT` authorizes work.** `CONSTRAIN` shapes the design and never independently generates plan work; `CONTEXT` generates none, though the design may stay compatible with it. Every planned activity must be justified by `PROJECT` knowledge, not by a constraint, boundary, invariant or context that merely exists.
- **Optimization is free only inside declared behavior.** Plan may choose any mechanism that preserves the requirements, domain invariants, architectural constraints, accepted limitations and acceptable manual operations. When the chosen mechanism cannot preserve one of them, reconsider the mechanism (for example by making the same accepted operation safely repeatable) rather than introducing a different recovery operation, cleanup step or lifecycle, and never silently redefine the limitation or operation. A limitation that is no longer acceptable goes back to the developer as a spec or clarification decision.
- **Revise authority depends on whether Judge has run.** Before Judge (no `gates/judge.md`) the plan is a draft: Plan may reconcile it with the current spec using its initial-generation authority, bounded by the `PROJECT` / `CONSTRAIN` / `CONTEXT` roles, `expected_change_surface`, `expected_new_mechanisms`, Minimum Sufficient Change and every recorded invariant, constraint, accepted limitation, acceptable manual operation, Constitution Waiver and explicit human decision. After Judge, revise stays Judge-driven in full: Plan only restamps when no content change is required, applies Judge-owned `plan_revisions`, or revises under explicit human authority given during that run, and it preserves recorded Judge dispositions and human decisions. Plan adds no automatic repair; the edge state is deterministic evidence the developer acts on. A fix that only restores the plan to already-established meaning is a bounded consistency correction; one that changes meaning is a substantive revision.
- Missing spec knowledge is never an error: fall back to the spec itself.

## Outline

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1 — it covers multi-app scope resolution and the 2-tier override check for both `{SCRIPT}` and `{AGENT_SCRIPT}`. Preamble modules: `provenance`.

1. **Setup**: Run `{SCRIPT}` from repo root and parse JSON for FEATURE_SPEC, IMPL_PLAN, SPECS_DIR, BRANCH. For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot").

2. **Load context**: Read FEATURE_SPEC and `/.knowledge/governance/constitution.md`. Load IMPL_PLAN, which setup preserves when it already exists.
   - Read the YAML frontmatter in FEATURE_SPEC before planning.
   - Treat frontmatter as authoritative for `classification`, `risk_level`, `recommended_next_step`, and `required_gates`.
   - If the body text appears to conflict with the frontmatter, flag the inconsistency to the user instead of overriding the metadata.
   - **Preserve the stage split.** Where the spec declares a `stage: post-deploy` obligation (or a deployment-dependent shape), the plan keeps both halves: the pre-merge `verify:<mode>` proof and the post-deploy obligation with its owner, required `deployed` or `rollout` evidence, and the existing `captured_as` record. Do not plan a pre-merge mode that needs a deployed environment; that is a spec authoring defect to return to the spec (verification contract §2.3).
   - Compute the spec fingerprint once with `judge.py semantic-hash --path <FEATURE_SPEC>` (`.devspark/scripts/judge.py` in installed repos) at the moment you read the spec, and keep that value as the only one you may stamp: it identifies the spec content you actually read and validated, never a value recomputed afterward.

   **Existing Plan / focused revise**: `PLAN_EXISTS` reports prior existence; `PLAN_MODE: revise`
   means preserve the existing Plan, including unrelated decisions and sections. Do not infer
   permission to reset from a template-only or empty file. Use `--reset` / `-Reset` only after
   explicit destructive intent from the developer; a STOP never grants that intent.

   **Reconciliation against the current spec.** Select this branch when the plan's `spec_semantic_hash` differs from the fingerprint just computed, or is absent while Plan is legitimately invoked.

   1. If `gates/judge.md` exists, run `judge.py review-state --feature-dir <SPECS_DIR>` first. Stop and name `/devspark-planning-review` when `spec_changed` or `judge_stale` is true or `pending_decisions` is above zero; never restamp or change Plan-owned content after Judge on stale review evidence.
   2. Validate every Plan-owned artifact against the current spec only, with no memory of prior spec content. Report exactly one outcome in this run's output and nowhere else: NO CONTENT CHANGE (leave content alone, update only `spec_semantic_hash`), BOUNDED CORRECTION (change only what restores already-established spec intent), or SUBSTANTIVE REVISION (revise only what the current spec authorizes). These labels are report-only: never write them to the spec, plan, tasks, any support artifact or any gate artifact, and they never alter state, routing or authorization. Preserve unrelated decisions and sections.
   3. Write `spec_semantic_hash` with the fingerprint from the context step only after the result is coherent with the current spec; never stamp to silence freshness. If the fingerprint was unavailable, reconcile but do not stamp.
   4. Hand off to Tasks only when Plan content changed; a restamp alone changes nothing Tasks depends on.

   Stop without changing the stamp or any provenance when: (1) `[NEEDS CLARIFICATION]` remains in the spec; (2) the authoritative spec is ambiguous; (3) two recorded authoritative decisions conflict; (4) restoring fidelity would require changing the spec; (5) no mechanism can preserve a domain invariant, architectural constraint, accepted limitation, acceptable manual operation or constitution requirement; (6) the revision would invalidate already-completed work; (7) the revision would drop a recorded Constitution Waiver or explicit human decision; (8) the only safe route is destructive replacement requiring `--reset`; (9) a Judge artifact exists and the change exceeds Judge-owned revisions or explicit human authority, or review evidence is not current for the spec you read; (10) a pending human decision exists (after Judge, a recorded Judge pending decision; before Judge, a question you or Tasks raised this run that is still unanswered). Every stop names its resolution channel: `/devspark-clarify` for a spec-side answer; an explicit developer answer in the same run for a design-side decision; `/devspark-planning-review` (including `--judge-only`) for Judge-owned authority or a pending decision; `--reset` only when the developer states destructive intent. `--auto` or a standing autonomy instruction never substitutes for the developer's answer to a Plan stop.

   Plan never modifies `spec.md` or `tasks.md`, and never treats Analyze or Critic findings, legacy gate artifacts or handoff wording as authorization.

   For a Judge-owned correction, run `judge.py remediation --feature-dir <SPECS_DIR>` and consume
   only validated `plan_revisions`. Pending or ambiguous authority stops for the exact question.
   A `PLANNING GAP` identifies a missing decision but does not authorize redesign: without an
   approved Judge correction or explicit human authority, report it and stop. Raw findings alone
   do not authorize changes.

   Apply only the approved corrections. Refresh affected models, contracts, procedures and other
   support consumers when their claims change; preserve unaffected content. Explore only a material
   unresolved question necessary for the correction and stop once answered. Re-evaluate the affected
   Constitution checks. Then hand off to Tasks for re-projection and a focused Planning Review;
   changed Plan bytes do not prove a concern resolved. Do not run the initial-generation phases
   again for an otherwise complete existing Plan.

3. **Execute plan workflow** (initial generation): Follow the structure in IMPL_PLAN template to:
   - Fill Technical Context (mark unknowns as "NEEDS CLARIFICATION")
   - Fill Operating Context and Expected Mechanisms from the spec's `minimum_sufficient_change` and what is known (Minimum Sufficient Change, preamble §13); never invent scale, and explain every mechanism beyond the expected shape under Deviations (its shape, the forcing requirement, and the least-complex constraint that still suffices)
   - Fill `expected_change_surface` with the files or bounded areas this plan expects to change, each with `change` and `why`. It is orientation for Tasks, Analyze, Implement and owner lookup, not an allow-list: do not store counts or a list of Knowledge nodes to update (`explain-context.py --files` derives owners)
   - Fill Constitution Check section from constitution
   - Evaluate gates (ERROR if violations unjustified)
   - Phase 0: Resolve material planning questions; create `research.md` only if at least one needed research (see Phase 0)
   - Phase 1: Create `data-model.md`, `contracts/` or `quickstart.md` only when the change justifies them (see Phase 1)
   - Phase 1: Update agent context by running the agent script
   - Re-evaluate Constitution Check post-design
   - Once Phase 1 is coherent, record `spec_semantic_hash` in the plan frontmatter with the fingerprint taken in the context step

4. **Stop and report**: Command ends after Phase 2 planning. Report branch, IMPL_PLAN path, and generated artifacts.

## Phases

### Phase 0: Planning Questions (research only when needed)

1. **List the material unresolved questions**: each `NEEDS CLARIFICATION` in Technical Context, plus any decision a requirement needs before it can map to a design element. A dependency, integration or technology merely existing is not a question.
2. **If there is no question, do not research and do not create `research.md`.**
3. **For each question, explore only as far as needed to answer it**, then record it in `research.md`, which is the question ledger:
   - Question: [what must be decided]
   - Why it matters: [the requirement or design element that depends on it]
   - Evidence needed: [what would settle it]
   - Answer: [what was found]
   - Evidence pointer: [file, document or command that supports the answer]
   - Resolution: [the decision, its rationale, and alternatives only when real ones were weighed]
4. **Stop when planning is semantically finished**, not when a budget runs out: every material question is resolved, every requirement maps to a design element, the mechanism shape and expected change surface are known, and the verification strategy is sufficient. There is no token, search or time quota.

**Output**: `research.md` only when a question needed research, with every `NEEDS CLARIFICATION` resolved

### Phase 1: Design & Contracts

**Prerequisites:** every material planning question resolved

Create a support artifact only when a downstream consumer needs it, and never create a document just to say there is nothing to record. Downstream commands tolerate any of them being absent, and an existing useful artifact is not deleted merely because it is optional.

1. **`data-model.md`** only when persisted or domain shape changes materially, or when dedicated modeling is needed to reason about the design:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **`/contracts/`** only when an interface the project exposes to users or other systems changes materially:
   - Document the contract format appropriate for the project type
   - Examples: public APIs for libraries, command schemas for CLI tools, endpoints for web services, grammars for parsers, UI contracts for applications
   - Skip if no interface changes (internal refactors, build scripts, one-off tools, etc.)

   **`quickstart.md`** only when validation needs a procedure beyond the ordinary test and run commands.

   **`qa-acceptance.md`** only when both are true: the change needs acceptance validation beyond the developer's ordinary tests, and Product or QA would benefit from agreeing on that validation before implementation. Never create it empty or just to say it is not applicable; absence is valid everywhere. At most one per spec, ephemeral in the spec bundle, and never referenced by durable code, tests or Knowledge (it may name durable tests by path). Write one entry per scenario with these fields, compactly as ID | Trace | Surface | Scenario | Runner | Regression:
   - `id` and `trace` (an `FR-###` or `SC-###` of the spec; a scenario never creates a requirement)
   - `surface`: free text naming where the behavior is validated (for example API, Web, Mobile, CRM or cross-channel), never a DevSpark enumeration
   - `scenario`: Given/When/Then on observable behavior, where the Then is the expected result; add detailed steps only when genuinely necessary
   - `runner`: `automated`, `manual` or `hybrid`, naming no tool or vendor
   - `regression`: `add`, `update <ref>`, `retire <ref>` or `none (<reason>)`; replacement is `retire` plus `add`, consolidation is `update`
   - No execution-tracking fields (status, per-platform results, comments, test users, release tracking)
   - A durable destination may be an externally owned manual regression system, referenced by path or identifier and not required to appear in the Git diff
   - Acceptance scenarios validate the delta; regression tests validate the resulting system, so regression is curation, not accumulation

3. **Agent context update**:
   - Run `{AGENT_SCRIPT}`
   - These scripts detect which AI agent is in use
   - Update the appropriate agent-specific context file
   - Add only new technology from current plan
   - Preserve manual additions between markers

4. **Context Resolution** — traverse `.knowledge/` (flat documents, entities, entity layer docs, `governance/decisions/`) starting from the content this delta touches:
   - Seed the traversal from lexically discovered current-knowledge (the same matching `/devspark-explain` uses for `MATCHED_KNOWLEDGE`/`MATCHED_ENTITIES`) for the entities/paths this delta touches.
   - Run lexical matching and projection as one operation: `.devspark/scripts/bash/explain-context.sh --project --json <delta topic>` (PowerShell: `.devspark/scripts/powershell/explain-context.ps1 --project --json <delta topic>`). It matches the topic, then projects the matched ids over the same single verified index build and returns `MATCHED_KNOWLEDGE`, `MATCHED_ENTITIES` and `PROJECTION`. Exit code 3 with `RETRY_REQUIRED` means a Knowledge input changed during the run and the output was discarded: repeat it once, and ask the developer if it fails again. Do not run a second building command for the same delta.
   - To project from ids you chose yourself, Context Projection (`.devspark/scripts/bash/context-projection.sh --seed <id> [--seed <id> ...] --max-hops 2 --json` / `.devspark/scripts/powershell/context-projection.ps1 --seed <id> [--seed <id> ...] --max-hops 2 -Json`, one `--seed` per lexical hit) deterministically expands those seeds across accepted `.knowledge` relationships (entity `relations[]`, `constrains`/`constrained_by`, `links.references`) on its own build — never a second lexical/semantic search, never an unaccepted `/devspark-discover-knowledge` finding.
   - Evaluate every returned candidate for relevance to this delta; you are not required to include every candidate, but you MUST preserve the projection's provenance (seed, relation, hop distance) for whichever candidates you do keep, distinct from any purely-lexical (hop 0) seeds.
   - **Candidate context is not resolved context.** Projection is deliberately generous — it returns everything structurally reachable so nothing relevant is silently unreachable. Selecting from it is your job, and it is a narrowing one: `context_resolved` is the **smallest** set that lets the work be done safely, not the candidate list with provenance attached. Retaining a candidate because it is plausibly related, rather than because this delta needs it, is the failure mode this step exists to prevent. A resolved set materially smaller than the candidate set is the expected outcome; one that equals it means no selection happened.
   - Sibling layers of a matched entity, and governance documents reached through a shared hub rather than through a constraint that actually binds this delta, are the two candidate classes most often reachable but not needed. Drop them unless they inform the change.
   - Budget: **2 hops maximum** from the lexical seeds. Stop earlier when the traversal stops finding anything that can materially affect this delta — adjacency is not relevance. Design time is where retrieval complexity gets worked out so `/devspark-implement` never has to, but a wider set is not a better one.
   - Record each resolved item in `plan.md`'s `## Context Resolution` section as `context_resolved:` with its `id`, its `kind` (`knowledge` for a flat document, `entity`, `entity-layer`, or `decision`), the `via` relation or match that reached it, its `hop` (0, 1, or 2), and a short `reason` it was retained. Omit items that don't inform the delta — this is a pinned working set, not an index dump.
   - **Flat Knowledge documents are first-class here.** A resolved set consisting entirely of flat documents is valid and complete; never promote a flat document into an entity just so it can be recorded. Equally, resolve the specific entity layer when only one layer informs the delta rather than pulling the whole entity in.
   - This list is read, not re-derived, by `/devspark-analyze` (validity) and `/devspark-critic` (sufficiency), and consumed as already-resolved by `/devspark-implement`.

**Output**: the justified support artifacts (if any, including a conditional `qa-acceptance.md`), agent-specific file, `## Context Resolution` in plan.md

## Constraints

- Use absolute paths
- ERROR on gate failures or unresolved clarifications

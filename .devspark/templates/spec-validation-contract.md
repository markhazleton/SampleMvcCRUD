# Specification Validation Contract

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Use this contract whenever a workflow creates, updates, clarifies, or evaluates `spec.md`.

Installed repos should resolve this file from `/.devspark/templates/spec-validation-contract.md`.
Source repos should resolve it from `templates/spec-validation-contract.md`.

## 1. Frontmatter Contract

Every feature spec MUST begin with YAML frontmatter containing these keys:

```yaml
classification: quick-spec | full-spec
risk_level: low | medium | high
target_workflow: specify-light | specify-full
required_artifacts: intent, action-plan | spec, plan, tasks
recommended_next_step: plan | clarify | implement
required_gates: checklist | checklist, analyze, critic | checklist, analyze, critic, verify:<mode>
route_intent: optional one-off-fix | quick-spec | full-spec | refactor | prompt-change
depends_on: optional [] or ["repo:identifier", ...]
supersedes: optional [] or ["repo:identifier", ...]
participants:
  owner: human
  planner: ai
  implementer: ai
  reviewer: human
  critic: ai
  scribe: ai
```

Validation rules:

- `classification`, `target_workflow`, and `required_artifacts` MUST agree with each other.
- `quick-spec` maps to `specify-light` and `intent, action-plan`.
- `full-spec` maps to `specify-full` and `spec, plan, tasks`.
- `required_gates` MUST match the chosen route unless the spec explicitly documents why additional gates were added.
- Any `verify:<mode>` value MUST use one of the canonical modes from `templates/verification-contract.md`: `snapshot-neutral`, `golden`, `regression-pinned`, `shadow-agreement`, `end-to-end`.
- If `route_intent` is present, it is advisory classification context. It must not contradict `classification` and must follow: `refactor`/`prompt-change` => `classification: full-spec`; `quick-spec` => `classification: quick-spec`.
- If `depends_on`/`supersedes` are present, each entry MUST be a non-empty `repo:identifier` string and the body MUST include corresponding cross-repo context in `## Rationale Summary` (preferably `### External Contracts`).
- `participants` is optional advisory metadata. Its absence must not fail
  validation, block a workflow, alter prompt resolution, alter script
  resolution, or change command output.
- If frontmatter conflicts with body prose, treat frontmatter as authoritative and either repair the prose or flag the inconsistency.

Participant examples should use role-to-kind values such as `owner: human` or
`implementer: ai`. Stock examples must avoid personally identifying information.
Teams that add display labels own their own privacy handling.

## 2. Lifecycle State Contract

The spec body MUST include a status line using this format:

```markdown
**Status**: Draft
```

Allowed states: `Draft`, `In Progress`, `Complete`.

Validation rules:

- Newly created specs MUST start in `Draft`.
- Clarification updates MUST preserve the current lifecycle state unless the user explicitly changes it.

## 3. Required Sections By Route

### Quick Spec Required Structure

Quick specs MUST preserve this heading order:

1. `## Product Owner TLDR`
2. `## Intent`
3. `## Scope`
4. `## Constraints`
5. `## Action Plan`
6. `## Validation Notes`

### Full Spec Required Structure

Full specs MUST preserve this heading order:

1. `## Product Owner TLDR`
2. `## Rationale Summary`
3. `## User Scenarios & Testing`
4. `## Requirements`
5. `## Success Criteria`

Additional allowed headings:

- `## Clarifications`
- Route-specific subsections already present in the canonical templates
- New subsections only when they fit under an existing required heading and do not replace it

Validation rules:

- Required headings MUST exist exactly once.
- Headings may contain additional subsections, but required top-level sections must remain intact.
- Workflows may add `## Clarifications` when needed, but must not reorder unrelated required sections.

## 4. Required Content Quality

All specs MUST satisfy these content requirements:

- No unresolved placeholder text from the stock templates remains unless it is an explicit `[NEEDS CLARIFICATION: ...]` marker.
- No more than 3 `[NEEDS CLARIFICATION: ...]` markers may exist at one time.
- Requirements and validation notes must be testable and specific.
- Success criteria must be measurable and technology-agnostic.
- Scope boundaries must be explicit enough to distinguish in-scope vs out-of-scope behavior.
- Edge cases, assumptions, or constraints must be documented in the route-appropriate sections.

### Full Spec Required Content

Full specs MUST contain all of the following:

- At least one user story with acceptance scenarios
- At least one edge case bullet
- At least one functional requirement
- At least one measurable success criterion

### Quick Spec Required Content

Quick specs MUST contain all of the following:

- At least one in-scope bullet and one out-of-scope bullet
- At least one constraint bullet
- At least three action plan steps
- At least one validation note

## 5. Clarification Session Rules

If a workflow adds clarifications, it MUST use this structure:

```markdown
## Clarifications

### Session YYYY-MM-DD

- Q: <question> → A: <answer>
```

Validation rules:

- Add `## Clarifications` only once.
- Add exactly one bullet per accepted clarification answer.
- Integrate the clarified answer into the relevant required section; do not leave it only in the clarification log.
- Remove or replace now-contradictory text instead of duplicating it.

## 6. Repair Rules

When validation fails:

- Prefer minimal repairs that restore the canonical structure without rewriting unrelated content.
- If a missing or malformed item can be repaired directly from route metadata or obvious existing content, repair it.
- If classification or required section intent cannot be inferred safely, stop and ask the user to rerun `/devspark-specify` or repair the spec manually.
- Never silently drop user-authored requirements to make the document fit the contract.

## 7. Participant Metadata Non-Goals

Participant metadata is silent artifact-only context in this phase. It must not
create workflow dispatch behavior, a participant registry, reviewer lockout,
metadata-based inheritance, metadata-based customization precedence, or a
dedicated command-output summary.

## 8. Spec Knowledge Consumption Map

The spec is the source of truth for a feature's intent. The feature's `knowledge/` documents are a
non-authoritative work product derived from it; the `knowledge/feature.md` overview indexes the spec
sections below by reference and never copies their prose. This map defines once which commands
consume which spec element, for what purpose, and which may write it. It is a contract-level
definition only: no runtime engine, parser framework, graph, database or new artifact.

```yaml
spec_knowledge_map:
  purposes: [PROJECT, CONSTRAIN, CONTEXT, NOT_CONSUMED]
  elements:
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
  spec_sections:
    domain_invariants: [Key Entities, Requirements]
    architectural_constraints: [Requirements, Key Entities]
    accepted_limitations: [Requirements, Assumptions, Minimum Sufficient Change]
    clarifications: [Clarifications]
    success_criteria: [Success Criteria]
    assumptions: [Assumptions]
    boundaries: [Out of Scope, Minimum Sufficient Change]
    non_goals: [Minimum Sufficient Change]
  commands:
    clarify:
      declared: true
      reads: [requirements, domain_invariants, architectural_constraints, accepted_limitations, success_criteria, clarifications, assumptions, boundaries, non_goals, context]
      writes: [requirements, domain_invariants, architectural_constraints, accepted_limitations, success_criteria, clarifications, assumptions, boundaries, non_goals, context]
    plan:
      declared: true
      reads: [requirements, domain_invariants, architectural_constraints, accepted_limitations, success_criteria, clarifications, assumptions, boundaries, non_goals, context]
      writes: []
    analyze:
      declared: true
      reads: [requirements, domain_invariants, architectural_constraints, accepted_limitations, success_criteria, clarifications, assumptions, boundaries, non_goals, context]
      writes: []
  not_yet_declared: [specify, tasks, critic, checklist, implement]
```

Rules:

- **Purposes are semantic.** `PROJECT` is current intent: it may authorize plan work, bounded by
  constraints. `CONSTRAIN` limits or shapes the work and never independently generates it; it is not
  a missing capability, a backlog invitation, or authorization to invent a recovery workflow.
  `CONTEXT` informs understanding and generates no work; a design may stay compatible with it but
  must not implement it. `NOT_CONSUMED` is an explicit disposition, not an omission. An element may
  carry more than one applicable purpose, and an element needs no active consumer.
- **Awareness is not authorization.** Knowing an element never turns `CONTEXT` or `CONSTRAIN` into
  scope.
- **Accepted limitations are decisions.** Accepted limitations, acceptable manual operations,
  `explicitly_not_required` items and operational boundaries are `CONSTRAIN`. The current Plan may
  not override them; reconsidering one needs an explicit spec or clarification decision. Implementation
  mechanics adapt to accepted behavior; accepted behavior is not rewritten to suit a mechanism.
- **Domain invariants and architectural constraints** are declared in Requirements and Key Entities,
  the only places they bind implementation. Rationale, Assumptions and Out of Scope may reinforce
  one but never hold one alone. The spec defines what may vary and what must not; the Plan chooses how
  to vary what may vary.
- **Tight context is minimum sufficient authoritative context**, not minimum information. Plan
  consumes the smallest projection that completely defines the current change, always including every
  invariant, constraint, accepted limitation, boundary and non-goal. Analyze deliberately reads more
  than Plan so it can judge Plan against everything authoritative.
- **Declarations must agree.** Each command in `commands` carries a `## Spec Knowledge Consumption`
  section naming its per-element purposes and writes; a contract test fails when a declaration and
  this map disagree, naming the command and element.
- **Provisional.** Spec knowledge is a work product of the active change, not a durable Knowledge
  pillar; no archival or promotion machinery applies to it.

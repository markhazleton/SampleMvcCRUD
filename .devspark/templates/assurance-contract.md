# Assurance Contract

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Use this contract whenever a command evaluates work it did not author — `/devspark-analyze`, `/devspark-critic`, `/devspark-verify`, `/devspark-pr-review`.

Installed repos should resolve this file from `/.devspark/templates/assurance-contract.md`.
Source repos should resolve it from `templates/assurance-contract.md`.

## 1. Why This Exists

Four commands evaluate the same change. Without a shared boundary they converge on one behavior — "review the work" — and the cost shows up as duplicate findings, lane leakage, and a developer who reads the same concern four times in four voices and stops reading.

The four capabilities are **complementary, not redundant**. Each answers a question the others structurally cannot:

| Capability | Core question | Primary evidence | Owns | Explicitly does **not** own |
|---|---|---|---|---|
| **Analyze** | Are the planning artifacts internally sound? | `spec.md`, `plan.md`, `tasks.md`, declared contracts/schemas, resolved knowledge ids | Closed-world correctness of the authored artifacts (§2) | NFR achievability, unwritten operational controls, baseline-source contradictions, context sufficiency |
| **Critic** | Is this internally sound plan wrong about the real system it will run in? | The artifacts **plus** existing code, tests, and `.knowledge/` | Open-world challenge against repository and operating reality (§3) | Artifact consistency, traceability, task bookkeeping, documentation drift, implementation correctness, diff review |
| **Verify** | Did the implementation actually work? | Executed proof modes, committed tests, measured values | Empirical proof and the closeout decision | Planning-artifact quality, speculative design risk |
| **PR Review** | Should this resulting delta be accepted? | The durable diff, executed tests, `.knowledge/`, the constitution | Acceptance of the change as written | Re-deriving the route's findings from zero |

The whole model is one sentence per lane:

> **Analyze checks whether the plan makes sense on paper. Critic checks whether that sensible plan survives contact with the real system. Verify proves whether the implementation works. PR Review decides whether the resulting delta should be accepted.**

## 2. Analyze's Lane

Analyze owns the **closed-world correctness of the authored work artifacts**. Its test is mechanical:

> If something can be shown wrong solely because the planning artifacts contradict themselves or fail their declared structural contract, it belongs to Analyze.

Analyze owns, exclusively:

- spec ↔ plan consistency, plan ↔ tasks consistency, including a plan that contradicts a decision the spec records or changes behavior the spec declares (clarification answers, non-goals, domain invariants, architectural constraints, accepted limitations, acceptable manual operations)
- requirement coverage and requirement ↔ task traceability
- duplicate, contradictory, or unanchored requirements
- terminology drift and wording precision
- task dependency and ordering validity
- missing task anchors; tasks citing requirement ids that do not exist
- completed tasks with no supporting linkage
- identifier and field names contradicting a declared contract or schema
- Rationale Summary completeness and spec→plan Core-Problem drift
- `context_resolved` **validity** — every cited id and relation actually resolves
- artifact and schema conformance, and planning bookkeeping correctness
- whether affected current-knowledge updates and tests *exist as tasks* (presence of coverage, not its adequacy)
- when `qa-acceptance.md` exists: scenario trace validity, scenarios introducing requirements outside approved scope, and externally important success criteria left without a scenario
Analyze does **not** make open-world judgments. It cannot see the running system, and it must not guess at one.

## 3. Critic's Lane

> **Critic exists to find the places where a coherent plan is wrong about the system it will run in — unimplementable conditions, absent operational controls, and unexamined assumptions about code, dependencies, and failure — early enough that the plan can still change cheaply.**

Critic owns the **open-world challenge against repository and operating reality**:

- requirements resting on false assumptions about existing code
- conditions the baseline system can never actually produce
- behavior removed as "redundant" that is in fact a sole required path
- hidden coupling to existing consumers
- missing rollback, disable, or kill-switch posture
- missing observability needed to operate the proposed behavior
- unexamined external-dependency behavior and failure modes
- persistent-data limits, state-machine failure modes, compatibility assumptions
- concurrency and async hazards visible from the proposed design
- NFR achievability
- `context_resolved` **sufficiency**
- architectural assumptions unsupported by current source
- whether planned test and knowledge coverage is *proportionate to runtime risk* (adequacy, not presence — presence is Analyze's)
- when `qa-acceptance.md` exists: internals-based scenarios, unjustified duplication, append-only regression dispositions, disproportionate coverage, and evidence that does not prove its requirement

These acceptance-planning checks sit inside the existing lanes. Planning-review adjudication is unchanged: Judge receives them only as ordinary findings from the lane that owns them.

### 3.1 Evidence Requirement (non-negotiable)

> **Critic MUST inspect the relevant existing code, tests, and `.knowledge/` before asserting anything about current system behavior.**

A finding that claims the baseline does or does not do something, without having read it, is a guess wearing a severity label. Cite the file and symbol that establishes the claim. If the baseline cannot be inspected, the finding becomes an assumption to verify (§7), not a defect.

The strongest shape is a contradiction between a plan statement and a source fact:

```text
Plan states:      <X> will be absent at this point.
Baseline proves:  <path:symbol> always populates <X>.
Therefore:        the condition the requirement depends on cannot occur.
```

### 3.2 Baseline Reality Is Not Implementation Review

Critic may read existing source to understand **the baseline system the plan will modify**. It must not trace the implementation being written and generate a new risk catalogue from it.

| Capability | Input | Output |
|---|---|---|
| **Critic** | existing system + proposed plan | challenged assumptions, before implementation |
| **Verify** | implemented result | empirical proof |
| **PR Review** | the actual diff | acceptance decision |

If implementation of the active route has materially begun, Critic restricts itself to planning and baseline assumptions, and routes any observation about the new implementation to Verify or PR Review. See §8.

## 4. One Finding, One Owner

> **A finding has exactly one assurance owner.**

Every finding emitted by an assurance command carries `owner:` naming the capability whose lane it belongs to. The owner is a property of the finding, not of the command that happened to notice it.

### 4.1 Lane Prohibitions

Critic MUST NOT emit normal risk findings for anything in §2 — documentation drift, requirement numbering, missing task anchors, task bookkeeping or completion state, requirement ↔ task coverage, terminology mismatch, duplicate requirement text, artifact structural inconsistency, or a `context_resolved` entry that simply fails to resolve.

Analyze MUST NOT emit findings for anything in §3 — NFR achievability, unwritten operational controls, real-world system assumptions, failure modes absent from the artifacts, `context_resolved` sufficiency, or baseline source behavior contradicting an otherwise coherent plan.

### 4.2 Assurance Escape Routing

A capability may incidentally notice a defect in another capability's lane. Ignoring it loses real information; emitting it as a normal finding destroys the boundary. So it is **routed, not emitted**:

```yaml
routed_findings:
  - routed_id: <stable-id>
    owner: analyze | critic | verify | pr-review
    description: <what was noticed>
    rationale: <why this lane owns it>
```

Routed findings appear in a separate `routed_findings:` block, never in `findings:`. They do not count toward the emitting command's findings, never affect its verdict, and are addressed by re-running the owning capability. A routed entry naming the emitting command's own lane is a contract violation — emit it as a finding instead.

## 5. Deduplication

### 5.1 Across Capabilities

Before emitting a finding, confirm the substantive concern is not already captured by the spec, the plan, the task list, or another capability's open findings. If it is already explicitly recognized and appropriately handled, **do not create another finding**. Reference it in narrative if useful; do not mint a second id to restate it.

### 5.2 Within One Capability

One root concern normally produces **one** finding. Do not serialize a single root cause into a chain of consequences:

```text
WRONG   risk → telemetry consequence → fallback consequence → task consequence → doc consequence   (5 ids)
RIGHT   one finding: root cause, impact, evidence, required decision                               (1 id)
```

Split into separate findings only when the consequences require **independent decisions** by different owners. Record the shared root with `root_cause:` so related findings are demonstrably one concern.

### 5.3 The Metric

> **Material unique findings** — not the number of things a capability can plausibly mention. A run producing two material unique findings is better than one producing fifteen plausible observations.

## 6. Finding Quality

Every normal assurance finding must answer all five:

1. **What** is wrong or unexamined?
2. **Why** is it material to *this* change?
3. **What evidence** supports it?
4. **Why does this capability own it** rather than another?
5. **What decision, mitigation, or proof** would resolve it?

If those cannot be answered, prefer not emitting the finding. Generic advice — "consider scalability", "ensure observability", "think about security" — fails questions 2, 3, and 5 by construction and must not be emitted.

### 6.1 No Metadata Self-Policing

A capability MUST NOT manufacture a finding because its own preferred metadata is absent. Missing classification metadata (archetype, risk profile, change shape, and equivalents) is **resolved by inference and reported in the gate header**, not emitted as a developer-facing defect.

The single exception: if the missing metadata genuinely prevents meaningful review, say so in the summary and reduce the declared scope. That is a scope statement, not a finding.

## 7. Uncertainty Is Not a Defect

When something cannot be known until execution, say so and create the verification obligation. Do not escalate a speculative runtime hypothesis into a pseudo-defect.

```yaml
verification_obligations:
  - assumption: <what is being assumed>
    why_material: <what breaks if it is false>
    proposed_proof: <the verify mode or test that would settle it>
```

> **Critic identifies the risk in time to plan for proof. Verify performs the proof.**

## 8. Bounded Passes

Assurance is bounded. An unbounded gate is the convergence loop in `closeout-contract.md` §1 reappearing on the authoring side.

```text
DISCOVERY     one pass; findings surfaced freely
REMEDIATION   blocking / current-scope findings addressed
RECHECK       confirm disposition of the original findings
STOP
```

Rules:

- A **recheck** confirms the disposition of the findings from the discovery pass. It MUST NOT open a new open-ended discovery pass.
- A new observation seen *during a recheck* is classified under `closeout-contract.md` §4. It may expand the current route only if it hits one of the five expansion triggers in `closeout-contract.md` §2 — objective failure, unresolved requirement, violated invariant, correctness defect, or regression. Otherwise it is an Accepted Limitation, Deferred Work, or Learning, and the pass stops.
- A capability MUST NOT review the work performed to resolve its own earlier findings. Once implementation of those fixes begins, that delta belongs to Verify and PR Review.
- The prohibition above is per capability, so a mechanism added to resolve an *Analyze* finding can still be reviewed by Critic, and the reverse. That cross-lane path is how a remediation ratchet continues; Planning Review's Judge phase watches for it (`judge-contract.md` §13).
- **Assurance may discover work, but discovery alone does not expand current scope.** Only a Judge disposition (`judge-contract.md` §11), or a finding that hits a `closeout-contract.md` §2 expansion trigger, turns a finding into planning work.
- A non-blocking finding does not automatically create a task. Scope expands only on explicit developer intent (`closeout-contract.md` §8).

## 9. Verdicts Are Derived

There is one verdict vocabulary. A capability MUST NOT invent another, and MUST NOT emit a verdict that contradicts its own finding state.

| Verdict | Derivation |
|---|---|
| **STOP** | At least one open finding is a Blocking Defect — it hits an expansion trigger (`closeout-contract.md` §2) |
| **CONDITIONAL** | No Blocking Defect, but at least one open finding requires a consequential human decision or authorization before implementation |
| **PROCEED** | Neither of the above |

The verdict is computed from the findings, never authored independently. `GO`, `PASS`, `MOSTLY READY`, `PROCEED AFTER …`, and any other free-text verdict are contract violations. A verdict of `PROCEED` alongside an open Blocking Defect, or `blocking: false` alongside required pre-implementation actions, is a self-contradiction and must be corrected before the gate artifact is written.

Severity and convergence stay separate dimensions. A severe risk may be an Accepted Limitation or Deferred Work and not block; a modest issue that violates a required invariant blocks.

**Planning Review.** Child verdicts are evidence, not inputs to average. `/devspark-planning-review` derives one verdict from the reconciled concerns using this same vocabulary (`judge-contract.md` §14), so the developer never has to reconcile an Analyze `CONDITIONAL` with a Critic `STOP` by hand.

## 10. Interaction Policy

A capability running is not by itself a reason to interrupt the developer.

| State | Behavior |
|---|---|
| `PROCEED`, no open consequential findings | Report and continue |
| Non-blocking findings, already classified | Concise summary and continue |
| An unresolved consequential human judgment | Ask exactly that question |
| `STOP`, or a blocker needing authority or a scope decision | Stop |

The developer interacts with **decisions**, not with a gate merely because the gate ran.

**Authority is not a severity level.** Some findings are not hard to judge technically but are not an assurance capability's to settle — a product trade-off, a policy or compliance choice, an accepted risk, a cost commitment, a change to governance. Automate assurance; expose authority. Such a finding is surfaced as the explicit decision it is, naming who needs to make it, and a generated `PASS` / `FAIL` never stands in for that person having made it.

## 11. Critic Applicability

Critic applies on the basis of **observable change shape**, not authored risk metadata. Repository evidence shows authored `risk_level` predicts poorly in both directions: low-risk routes carrying critical design defects, and safety-critical routes with nothing for Critic to find.

> **`risk_level` MUST NOT be the primary run/skip switch. It may modulate review depth once applicability is established.**

### 11.1 Signals

Critic is applicable when the change exhibits at least one of these shapes:

| Signal | Shape |
|---|---|
| `persistent-data-shape` | Changes the shape, meaning, or controlled vocabulary of persisted data |
| `cross-boundary-interaction` | Adds or alters interaction with an external service or another subsystem |
| `state-lifecycle` | Changes state-machine transitions, lifecycle stages, or terminal-path behavior |
| `nondeterministic-behavior` | Depends on model, heuristic, or other nondeterministic output |
| `new-entry-point` | Introduces a new publicly reachable entry point or write path |

Signals describe **software shape**. They never name a product, vendor, or repository-specific system.

### 11.2 Negative Controls

A change exhibiting none of the signals is not automatically applicable. Typical negative controls: a pure symbol rename, a local mechanical refactor with no behavioral surface, a documentation-only change, formatting.

Not automatically applicable is **not** prohibited. `/devspark-critic` remains explicitly callable on any route — for an adversarial second opinion, after a significant plan revision, when the signals miss something, or when Analyze passes and something still looks wrong.

### 11.3 Inspectable Decision

The applicability decision is recorded in the gate header, in one of two shapes:

```yaml
applicability:
  applicable: true
  signals: [cross-boundary-interaction, state-lifecycle]
  invoked: automatic | explicit
```

```yaml
applicability:
  applicable: false
  signals: []
  reason: "pure symbol rename with no behavioral surface"
  invoked: explicit
```

The reason is one line. Applicability is a routing decision, not another risk essay.

## 12. Non-Negotiables

1. A finding has exactly one assurance owner; cross-lane observations are routed, never emitted.
2. Critic cites inspected evidence for any claim about existing system behavior.
3. Critic is planning-time assurance and never becomes post-implementation code review.
4. No capability emits a finding about its own missing metadata.
5. One root concern yields one finding unless the consequences need independent decisions.
6. Verdicts come from the controlled vocabulary and are derived from finding state.
7. Assurance passes are bounded: discovery, remediation, recheck, stop.
8. Findings classify under one convergence model — `closeout-contract.md`. There is no capability-specific taxonomy.
9. Analyze and Critic discover; Planning Review's Judge phase (`judge-contract.md`) adjudicates scope. The developer reads one planning decision, not two reports.

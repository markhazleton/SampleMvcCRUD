# Verification Contract

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Use this contract whenever a workflow declares, produces, or consumes a `verify` gate (`gates/verify.md`).

Installed repos should resolve this file from `/.devspark/templates/verification-contract.md`.
Source repos should resolve it from `templates/verification-contract.md`.

## 1. Why This Exists

The document gates (`checklist`, `analyze`, `critic`) validate that artifacts are internally consistent and that risks are named. None of them require demonstrating that the change actually works. This contract closes that gap: it converts empirical-proof practices (baseline-before-change, golden renders, regression-pinned tests, shadow agreement, end-to-end drive-through) from one-off spec prose into a declared, checkable gate.

Findings in `gates/verify.md` are **evidence**, not opinions: every claim MUST be backed by a command that was actually run and its actual output (or a reference to a committed artifact/log), not a description of what *should* happen if run.

## 2. Proof Modes

A spec declares which proof mode(s) it requires via `required_gates: verify:<mode>` in its frontmatter (see §3). `/devspark-specify`'s route classification assigns a default mode per route (see `specify.md`); the user or `/devspark-plan` may add or override it explicitly.

| Mode                | Use when…                                                                 | Minimum evidence required                                                                                                                                     |
| -------------------- | -------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `snapshot-neutral`   | Pure refactor — behavior must not change                                  | Baseline output captured **before** the change, the same probe re-run **after** the change, and a diff of the two shown to be empty (or containing only the intentionally-frozen deltas, each named and justified) |
| `golden`             | Prompt/template/rendering change where output shape is the contract       | Byte-identical (or intentionally-diffed-and-justified) render comparison for each golden fixture, before vs. after                                            |
| `regression-pinned`  | Bugfix — the defect must be provably fixed and provably absent before      | A test that fails on the pre-fix code (shown failing via `git stash`/checkout-old-code run) and passes on the fixed code                                      |
| `shadow-agreement`   | Routing/engine/model swap where old and new must agree on live/replay traffic | An agreement-rate measurement against a defined traffic sample or replay set, with the threshold declared and the actual measured rate reported              |
| `end-to-end`         | New feature — the real flow must be demonstrably exercised                 | The actual commands/steps used to drive the flow end-to-end, and their actual results (not a description of expected behavior)                                |

A spec MAY require more than one mode (e.g., a migration might need both `regression-pinned` and `shadow-agreement`). List all required modes; the gate is not satisfied until every declared mode has its own evidence block.

## 2.1 When Empirical Proof Is Warranted

Ordinary deterministic validation — the test suite, the linters, the type checker — answers *"does the code do what the code says?"* Empirical verification answers a different question:

> **Does this change depend on something the test suite cannot observe?**

That is a property of the **change shape**, not of the route label or the authored `risk_level`. Routes named `refactor` and `prompt-change` carry a default mode (see `specify.md`) because their shape is known in advance, but a change on any route can exhibit a verify-sensitive shape, and such a change should receive the matching mode regardless of its route.

The vocabulary is deliberately small and technology-neutral. Each shape names something the tests can be green about while reality disagrees:

| Change shape | Why tests can be green and reality wrong | Indicated mode |
|---|---|---|
| **runtime-configuration** | Defaults, flags, and settings resolve at runtime from state the tests supply themselves | `end-to-end` |
| **environment-dependent-behavior** | Behavior varies by deployed environment, platform, locale, clock, or identity | `end-to-end` |
| **external-contract** | An outside service or payload shape is asserted by a fixture the outside world never signed | `end-to-end` or `shadow-agreement` |
| **datastore-semantics** | Query, index, transaction, or collation behavior differs between the real engine and the test double | `end-to-end` or `regression-pinned` |
| **mock-substituted-behavior** | The component under test is replaced in every test that covers it | `end-to-end` |
| **deployment-state-assumption** | Correctness depends on the shape of data or configuration already deployed | `end-to-end` or `snapshot-neutral` |
| **behavior-preservation-claim** | The change asserts nothing observable changed | `snapshot-neutral` |
| **output-shape-contract** | The rendered or generated output *is* the contract | `golden` |
| **defect-recurrence** | A fix must be provably absent before and provably present after | `regression-pinned` |

These are indications, not an engine. A shape makes a mode **recommended**; declaring it in `required_gates` is what makes it required. Absence of every shape is a legitimate and common result — a contained local change with no environment-dependent behavior does not need empirical proof, and manufacturing a mode for it is waste.

Do not extend this table with repository-specific technologies. If a new shape is genuinely general, generalize it before adding it.

## 2.2 The Applicability Decision Is Reported, Not Hidden

Whichever command decides whether empirical verification is warranted — `/devspark-specify` at route classification, `/devspark-plan` when the design makes the shape visible, or `/devspark-verify` itself — states the decision and its reason in two lines:

```text
Empirical verification: REQUIRED (end-to-end)
Reason: runtime-configuration — behavior depends on the shape of already-deployed configuration
```

```text
Empirical verification: NOT REQUIRED
Reason: no verify-sensitive change shape — contained local refactor with no environment-dependent behavior
```

Keep it to those two lines. Classification may be automatic; the reasoning may not be invisible.

## 2.3 Stage and Evidence Class

**Implementation complete means ready to merge and deploy, not already deployed.** Deployment validation is a separate milestone with its own evidence obligations. Two optional attributes keep the milestones apart without adding a lifecycle state, a gate, or a script.

| Attribute | Values | Meaning |
|---|---|---|
| `stage` | `pre-merge` (default), `post-deploy` | When the obligation can be satisfied. A criterion or mode with no `stage` is `pre-merge`. |
| `evidence_class` | `automated`, `local-integration`, `deployed`, `rollout` | What kind of evidence a mode entry or a closeout criterion carries. |

| Evidence class | What it is | Typical source |
|---|---|---|
| `automated` | Unit, contract, golden, snapshot, or regression tests run in the build environment | The test suite and linters |
| `local-integration` | A real flow driven against a development or local dependency, not the deployed target | A local run against a dev service or fixture server |
| `deployed` | A check that ran against the actual deployed environment | A smoke or acceptance run against the published target |
| `rollout` | An operational rollout check: configuration applied, migration run, monitoring or alerting live | A rollout checklist result or an operational query |

Rules:

- A `post-deploy` criterion requires `deployed` or `rollout` evidence and cannot be satisfied by a pre-merge run. Local execution is never evidence of deployed-environment validation: `local-integration` evidence is recorded as `local-integration` and is never counted as `deployed` or `rollout`.
- Every `verify:<mode>` entry in `required_gates` is a pre-merge obligation. A `post-deploy` obligation is not listed in `required_gates`; it is carried as a deferred requirement with a durable record by the Closeout Contract (§4 there), so no pre-merge gate can pass or fail on evidence that does not exist yet.
- A required pre-merge mode that can only be proven in a deployed environment is a spec authoring defect: the mode is recorded `fail` with that reason, the local portion is run and recorded as `local-integration` evidence, and the work returns to the spec for a visible amendment (move the deployment-dependent half to a `post-deploy` obligation and keep the pre-merge half). The mode is never skipped and never marked `pass`.
- A mandatory pre-merge check that fails stays a failure. Nothing in this section weakens implementation verification, a pre-merge safety or compliance requirement, or the blocking `deployment_rollback` concern.
- A bundle that records neither `stage` nor `evidence_class` behaves exactly as before: every obligation is `pre-merge` and every mode keeps its existing meaning.

**Consequential proof stays a human decision.** A mode that needs resources or authority the command does not already hold — live traffic, a production-like environment, a replay corpus, credentials, or anything with a cost — is proposed, not assumed. State what the proof requires and ask. Automate the classification; never hide the commitment.

## 3. Frontmatter Contract

Specs that require verification MUST include `verify` in `required_gates`, qualified by mode:

```yaml
required_gates: checklist, analyze, critic, verify:snapshot-neutral
```

Multiple modes are comma-separated within the same `verify:` token set is not valid YAML scalar syntax — list each mode as its own `verify:<mode>` entry, e.g. `required_gates: checklist, analyze, critic, verify:regression-pinned, verify:shadow-agreement`.

Validation rules:

- If `required_gates` names one or more `verify:<mode>` entries, `/devspark-implement`'s gate pre-flight (see `implement.md`) MUST refuse to proceed until `gates/verify.md` exists, its `modes:` list is a superset of the ones required, and every listed mode's `status` is `pass`.
- A `verify` requirement is never satisfied by document review alone — `/devspark-critic` and `/devspark-analyze` MUST NOT mark it resolved.

## 4. Gate Artifact Shape

`/devspark-verify` (or a manual/CI-produced equivalent) writes `FEATURE_DIR/gates/verify.md`:

```yaml
gate: verify
devspark_version: "<installed version, or `unknown`>"
generated: "<ISO-8601 timestamp of this run>"
status: pass | warn | fail
blocking: true | false
summary: "<concise outcome>"
modes:
  - mode: snapshot-neutral | golden | regression-pinned | shadow-agreement | end-to-end
    evidence_class: automated | local-integration | deployed | rollout   # optional; default automated
    status: pass | fail
    evidence: |
      <actual commands run and actual output/results — not a plan to run them>
discovered:
  - "<observation surfaced while running the proof, carried to closeout for classification>"
```

The `discovered:` list is the overflow channel for everything the run surfaced that is not a proof result. It never affects any mode's `status`, and it is not a to-do list for this route — the closeout phase classifies each entry (see §6).

## 5. Non-Negotiables

- **Never fabricate evidence.** If a proof mode can't be executed in this environment (no test runner, no traffic sample, no baseline artifact), the gate status is `fail` with the reason stated — not a hypothetical pass. A mode that needs a deployed environment that does not exist yet is the authoring defect described in §2.3, not a pass.
- **Name the evidence class.** Record the class that was actually obtained. Evidence from a local run against a development dependency is `local-integration`, never `deployed`.
- **Re-run on drift.** Like `analyze`/`critic` (see the gate-staleness convention in those commands), `verify` evidence is tied to the code state it was captured against; if the implementation changes materially after verification, re-run before `/devspark-create-pr`.
- **Evidence over narration.** A finding that says "this should work because X" is not evidence. A finding that shows the command, its output, and the pass/fail conclusion is.

## 6. Closeout: The Convergence Phase of Verification

Gathering evidence answers **"what does the evidence say?"** It does not answer **"is this work finished?"** That second question is the **closeout phase**, defined by the Closeout Contract (`templates/closeout-contract.md`) and run by `/devspark-verify` itself, in the same invocation, immediately after the evidence phase. Two phases, two artifacts (`gates/verify.md` and `gates/closeout.md`), one developer interaction.

The phases stay distinct because their incentives differ: the evidence phase optimizes for finding things, the closeout phase optimizes for converging. Collapsing them would quietly bias verification toward noticing less, since every observation would arrive already priced as work.

A failing proof mode is a blocking defect and routes back to `/devspark-implement`. Every other observation that surfaces during the run is recorded under `discovered:` and classified in the closeout phase as an Accepted Limitation, Deferred Work, or a Learning / Changed Assumption. Turning such an observation into new work inside the current spec, on verification's own authority, is the convergence failure that contract exists to prevent.

`/devspark-closeout` re-runs the closeout phase alone — same contract, same artifact — and is an advanced reassessment entry point, never a required next step after this command.

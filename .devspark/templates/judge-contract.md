# Judge Contract

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Use this contract whenever Planning Review adjudicates the findings of Analyze and Critic into one planning decision.

Installed repos should resolve this file from `/.devspark/templates/judge-contract.md`.
Source repos should resolve it from `templates/judge-contract.md`.

This contract defines the **adjudication** half of Planning Review. The **mechanics** half (resolving the feature once, running the children, presenting one result) lives in `templates/commands/planning-review.md`. They are kept apart on purpose: only the adjudication rules are Judge-specific.

## 1. Why This Exists

Analyze and Critic are good at discovery, and each is bounded by `assurance-contract.md`. Their findings still flowed too directly into remediation and into the developer's head:

```text
finding → becomes work → remediation adds machinery → machinery creates findings → repeat
```

and the developer had to reconcile two verdicts, routed findings, and verification obligations by hand. Judge is the one place where discovery turns into a planning decision.

## 2. Principles (non-negotiable)

> **Analyze and Critic discover. Judge adjudicates scope.**
>
> **Judge does not discover.**
>
> **Judge does not remediate.**
>
> **A finding is evidence, not automatically work.**
>
> **Discovery alone does not expand current scope.**
>
> **A risk does not prescribe its remediation.**
>
> **Severity, convergence classification, and remediation choice are different concepts.**
>
> **Staleness is not blocking by itself.**
>
> **Assurance should converge based on unresolved risk, not artifact churn.**
>
> **Capture discovery without converting discovery into scope.**
>
> **DEFER is a disposition, not a task status.**
>
> **Complexity is a cost that requires justification.** (Minimum Sufficient Change, `command-preamble-contract.md` §13)

## 3. Canonical Question and Boundary

> **Given the objective, requirements, operating context, and the children's findings, what is the smallest justified change set that is ready to implement?**

| Capability | Question |
|---|---|
| Analyze | Are the planning artifacts internally sound? |
| Critic | Is this internally sound plan wrong about the real system it will run in? |
| **Judge** | **What is the smallest justified change set that is ready to implement?** |

Judge **owns**: collecting, normalizing, merging, classifying, challenging proportionality, bounding scope, surfacing consequential decisions, and deriving one verdict.

Judge **does not** (a violation of this contract, not a style issue):

- scan the repository, review the architecture, hunt for gaps, or speculate like Critic;
- emit a concern that no child finding supports;
- edit `spec.md`, `plan.md`, `tasks.md`, code, tests, Knowledge, or a child gate artifact.

Its only durable output is `gates/judge.md`. It may open a file or symbol **only** when a child finding cites it as `evidence:` and only to adjudicate that finding.

**Enforcement.** The source invariant (every concern cites at least one existing child finding) and the read-only boundary are checked by `scripts/judge.py` (`validate`, `seal`, `guard begin`, `guard end`), not left to prompt wording. The guard protects the planning bundle (`spec.md`, `plan.md`, `tasks.md`, child gates, everything under the feature directory except `gates/judge.md`). A change there is a violation; a change elsewhere in the repository is reported as `concurrent-change-detected` and is never attributed to Judge.

## 4. Planning Review Structure

```text
resolve the feature once → Analyze → Critic (when applicable) → Judge → one result
```

**Mechanics** (candidate seams for a later Plan → Tasks orchestrator — not extracted here): resolve the active feature once; decide child applicability; reuse or rerun a child artifact; validate the child completed; consume the durable child artifact; produce the parent artifact; present one result.

**Judge-specific**: classification, reclassification, proportionality, scope authority, ratchet detection, human authority.

Analyze always runs. Critic runs when `assurance-contract.md` §11 finds a change-shape signal, or when invoked explicitly. If Analyze reports a Blocking Defect that needs plan revision, Judge adjudicates Analyze alone — Critic reviews a coherent plan, and re-reviewing an incoherent one wastes the pass.

## 5. Context Boundary

> **Smallest sufficient trustworthy context for the job at hand.**

Judge reads: the objective, requirement ids and any operating context in `spec.md`, a one-line summary of `plan.md`, the two gate artifacts, and the convergence state. It does not reload the repository. Anything beyond that must be justified by a child finding's `evidence:` pointer.

## 6. Normalization

For each child finding Judge keeps, without rewriting meaning: source gate, source finding id, child severity (Analyze `severity`; Critic `base_severity` and `effective_severity` — the two scales are never merged), classification, owner, `root_cause`, requirement ids, plan/task anchor, evidence, `execution_mode`, and routing.

`classification_source` records where a classification came from:

- `child` — the child supplied it.
- `judge-inferred` — the child did not (a legacy artifact) or Judge changed it. Inference is conservative: a significant finding is a `blocking-defect`; anything else is `deferred-work` until adjudicated.

Metadata is never silently invented: inferred means labeled inferred.

## 7. Root Concerns and Merging

> **One decision by one owner resolves all of them.**

That is the merge test. Prefer deterministic signals — the same `root_cause`, requirement, task, plan anchor, mechanism, or remediation lineage — then bounded judgment. Every merged concern records `sources`, `merge_basis` (`deterministic` or `judgment`), and a one-line `merge_reason`. Independent risks are never merged to lower a count.

**Anti-laundering.** A finding is *significant* when it is blocking, has critical/high/showstopper severity, falls in a security, safety, data-integrity, or contract category, or cites a constitution principle. A merge never hides or weakens a significant source. Merging a blocking source into a non-blocking concern is a downgrade (§9), whatever the other sources say.

## 8. Classification, Resolution, Route, Future Work

| Dimension | Answers | Values |
|---|---|---|
| `classification` | What state is the finding in? | `blocking-defect`, `accepted-limitation`, `deferred-work`, `learning` — the closeout vocabulary, unchanged |
| `resolution` | What is the smallest valid remediation choice? | `require`, `simplify`, `accept`, `defer` |
| `change_shape` | What *kind* of change is proposed (not how hard)? | `documentation`, `planning`, `test`, `code`, `operational`, `machinery` |
| `route` | Which later phase owns this? | `verify`, `closeout`, `pr-review`, `implement` |

Valid example: `classification: blocking-defect` with `resolution: simplify` — the concern is real and blocking, and the smallest valid remediation removes machinery instead of adding it.

Where each kind of outcome lives:

| Outcome | Representation | Becomes a current task? |
|---|---|---|
| Current planning work | `require` or `simplify` | Yes |
| Routed obligation | `route:` / `routed_obligations:` | No — but it stays part of this spec's completion |
| Accepted limitation | `accept` + `accepted-limitation` | No, and no future-work promise |
| Future work | `defer` + `future_work:` | No — preserved outside the spec |
| Awaiting a human | `resolution: null` + `needs_authority: true` + `decisions_needed` | No — and no future work, until decided |

Rules:

- **A pending human decision is a decision state, not a resolution.** A concern that needs a human to choose carries `resolution: null`, `needs_authority: true`, and a `decisions_needed` entry (optionally `decision: {state: pending, question: ...}`). `classification` describes the concern; `resolution` records what was decided; until someone decides, `resolution` is honestly unset. `null` is valid only with `needs_authority: true`. A pending concern yields `CONDITIONAL`, appears in `decisions_needed`, and is **not** current planning work, **not** future work, **not** accepted, and never silently dropped. When the human answers, re-run Judge (`--judge-only`): it records the chosen `resolution`, clears `needs_authority`, and only then does the concern flow to a task (`require` / `simplify`) or to future work (`defer`). Never add a fifth resolution such as `pending`.
- `accept` requires `accepted-limitation` or `learning`, names `accepted_by`, and carries no `future_work`. `planning-review` may accept only a non-significant finding.
- `defer` requires `deferred-work` and a `future_work` block (`preserve: true`, `type`, `title`, `reason`). A deferral with nothing preserved is an acceptance in disguise.
- A routed obligation is **not** deferred and **not** a new planning task.
- A non-blocking concern enters current scope (`require` / `simplify`) only on explicit developer intent, recorded as `promoted_by` (`closeout-contract.md` §8).
- New machinery (`change_shape: machinery` with `require`) needs a `forcing_requirement`, or an authority decision.

Future work is real work removed from current scope. Judge only *identifies* it. A separate lifecycle step materializes it as a Future Work Plan (`future-work-plan-template.md`); creating one does not authorize investigation beyond preserving the discovery.

## 9. Reclassification Authority

Judge may change a classification, but never silently, and never downward without authority.

```yaml
classification: accepted-limitation
classification_source: judge-inferred
classification_change:
  from: blocking-defect
  source: critic-002
  reason: "FR-060 explicitly accepts this rollout behavior"
  authority: governing-requirement
```

- **Upgrade** (non-blocking → blocking): allowed when the child's evidence directly supports it; record `from`, `source`, `reason`.
- **Downgrade of a blocking source**: requires `authority` of exactly one kind — `human`, `governing-requirement` (an explicit requirement in an accepted artifact), or `deterministic-evidence` (accepted artifacts that contradict the finding). Otherwise the concern stays `blocking-defect`, and when the interpretation is genuinely unresolved it sets `needs_authority: true`. Child opinions are never averaged.
- Changes between non-blocking classifications are a resolution choice and need no record.

## 10. Proportionality

For every concern that would create or retain significant architecture, Judge asks, first:

> **What can be removed or simplified while still satisfying every forcing requirement and invariant?**

and then: what current requirement forces this; what demonstrated risk exists; what scale and frequency apply; is the operation autonomous or operator-driven; is human review acceptable; can an existing capability handle it; what simpler remediation exists; and what new failure surface would the mechanism create? Scale, frequency, execution, and review tolerance come from `plan.md`'s `operating_context` when present; never invent them.

When `plan.md` records `expected_new_mechanisms` and a remediation would introduce a mechanism beyond that shape (for example, a cache where `caches: 0`), ask: **what current requirement or demonstrated constraint forced this deviation?** The deviation may be justified. This is a proportionality signal, not a score, and never an automatic rejection.

Resolution order, lightest first:

1. remove behavior
2. simplify the design
3. reuse an existing capability
4. deterministic validation or a test
5. an operational procedure
6. accept a bounded limitation
7. preserve as future work
8. add new runtime machinery

Runtime machinery is allowed. It carries the greatest burden of justification. Critic's `least_complex_mitigation` and `mitigation_kind` are evidence for this order, not a decision: Judge may choose a lighter or heavier resolution with a recorded reason.

**Where proportionality is recorded.** Use the existing concern fields; there is no parallel field set. When a concern requires machinery, its `forcing_requirement` names the requirement, invariant or constraint that forces it, its `action` states the least-complex sufficient constraint (what the remediation may add and no more) and why a lighter step from the order above does not suffice. When the remediation would go beyond the shape `plan.md` expects (`expected_new_mechanisms`, `expected_change_surface`), the concern sets `needs_authority: true`: exceeding the approved Plan requires Judge-recorded or explicit human authority, never an implementer's or a Tasks run's own decision.

## 11. Scope Authority

Findings flow through Judge, and only through Judge, into current-spec changes:

```text
Analyze / Critic findings → Judge → dispositions → Gate Remediation Merge → current-spec changes
```

When a valid `judge.md` exists, Gate Remediation Merge consumes `scripts/judge.py remediation` and does **not** merge raw findings into scope. Only `require` and `simplify` become planning tasks. Accepted limitations, routed obligations, and preserved future work never do.

That authoring-time rule does not remove human authority during implementation. A human may
explicitly authorize a bounded correction needed to complete the current objective. Record the
authorization and its boundary in the existing feature artifacts; do not create a new spec or
branch solely because the correction adds a file or task. This does not waive Analyze/Judge
freshness or any verification gate: use the existing freshness result to determine whether a focused
recheck or rerun is required. A change to the objective or a material design, domain, safety,
security, or data-contract decision still requires the applicable authoring/review decision.

## 12. Freshness, Rerun, Recheck, Route

Raw hashes remain provenance and concurrency evidence. Compatible recorded `semantic_hash`
values distinguish recognized bookkeeping from decision changes; missing or incompatible
algorithms retain raw-hash behavior. `bookkeeping-changed` is non-blocking and needs no recheck.
Never add semantic metadata retroactively to older evidence. After PROCEED, supported child
execution annotations must still respect existing dispositions: new/reopened undispositioned
blockers, changed classification, unknown status and weakened proof remain consequential.

Approved source/test/non-governance Knowledge changes during execution are not unchanged code.
With compatible unchanged planning evidence and an approved change surface, they remain
`stale-nonblocking`, with execution evidence owned by Verify and PR Review. Unknown deviations,
missing inputs and governance changes require recheck. Raw seal/guard checks are unchanged.

Child states:

| State | Meaning |
|---|---|
| `fresh` | Every reviewed artifact still matches its recorded hash |
| `stale-nonblocking` | Changed, but traceably (bounded remediation, bookkeeping) — a focused recheck is possible, not required |
| `bookkeeping-changed` | Raw bytes changed but compatible recorded semantics did not; reuse without recheck |
| `stale-recheck-required` | Changed in a way that invalidates the evidence for a blocking decision |
| `superseded` | Reviewed a design or requirement set that no longer represents the current design; its open findings are not current blockers |
| `missing`, `not-applicable` | Not run, or Critic was not applicable |

> **Staleness is not blocking by itself.** Blocking requires an unresolved blocking finding, or a changed artifact that invalidates the evidence for a blocking decision.

Three different actions: **rerun** (the whole child gate), **recheck** (only the affected findings or remediation), **route** (a later mandatory phase owns the validation). A changed `spec.md` calls for a rerun; a mechanism-bearing remediation (new persistence, state, API, configuration) calls for a Critic recheck; bounded remediation that traces to a disposition calls for neither.

`scripts/judge.py children` computes these states. `superseded` is a Judge judgment, recorded in `judge.md` and honored only while the child artifact's hash still equals the one Judge recorded.

**Run provenance.** Planning Review generates one `run_id` (`judge.py resolve`) and hands it to every child, which stamps it as `planning_review.run_id` in its gate header. Before Judge runs, `judge.py seal` records for each child its hash and whether this run **generated** it (carries this run's id) or intentionally **reused** it (a fresh artifact from an earlier run, accepted by this run). A child that changes after it was sealed, or that a run claims to have generated but did not stamp, is invalidated and must be rerun; it is never consumed just because its reviewed input hashes still match. Reuse stays valid and a different run id alone never forces regeneration. This is attribution, not locking: there is no lock, lease, or run database.

**Bounded convergence.** Child gates → Judge → one remediation round → focused recheck where required → stop. A full Analyze → Critic cycle needs evidence that remediation materially changed the planning assumptions. A third adjudication round means stop and ask a human.

**Correction ownership.** Remediation adds `plan_revisions` and `task_work`; the existing
`current_planning_work` aggregate is retained for compatibility, not blanket task authority.
Optional `revision_target` must be `plan` or `tasks`. Planning and machinery corrections default
to Plan; execution decomposition needs an unambiguous Tasks owner. Unknown or invalid ownership
stops through `pending_decisions`. Accepted, deferred, pending and routed concerns never enter
either automatic-work partition. Raw findings do not authorize scope.

`review-state` derives `plan_revision_count`, `task_work_count`, `plan_changed`, `tasks_changed`
and `tasks_stale` from concerns, reviewed inputs and Tasks' `plan_semantic_hash`. Missing comparison
provenance is unknown, not evidence of correction. A changed Plan does not resolve its concern.
`plan_spec_state` and `tasks_plan_state` (`current`, `stale` or `unknown`) compare the Plan's
recorded `spec_semantic_hash` and Tasks' `plan_semantic_hash` with the current upstream
fingerprints. They are independent of the Judge-relative keys above and are reported with or
without a Judge artifact.
Next routes owned STOP to Plan revise, stale Tasks to re-projection and current corrected inputs
to focused review. Plan preserves unrelated content; Tasks protects completed work and records a
Plan fingerprint only after coherent projection. Pending authority and unrelated invalidation
take precedence. Failed correction stays STOP; successful review may PROCEED. No persisted
revision ledger or automatic redesign is introduced.

## 13. Ratchet Detection (advisory)

Remediation tasks already carry `(resolves: <finding-id>)`. A finding located in such a task is a finding **against a remediation**; one whose remediation resolved another such finding is **remediation-on-remediation**. `scripts/judge.py children` reports these as `ratchet_candidates`. They are advisory and never block. Before approving another layer, Judge asks:

> **Would removing or simplifying the earlier remediation resolve the original concern?**

## 14. Verdict

Child verdicts are evidence. Judge derives one verdict from the reconciled concerns and open decisions, using the existing vocabulary:

| Verdict | Derivation |
|---|---|
| **STOP** | At least one `blocking-defect` concern has a determined remediation and no `route` — the plan must change |
| **CONDITIONAL** | No such concern, but a concern or decision needs human authority (including a blocking defect awaiting a named choice, and any concern with `resolution: null`) |
| **PROCEED** | Neither |

A blocking concern carrying a `route` is owned by a later mandatory phase (typically `implement`, where the planned task already exists). It does not stop planning review, and it is not forgotten: `scripts/judge.py lifecycle` refuses a pull request until that phase has completed it.

The verdict is computed, never authored. `scripts/judge.py validate` rejects a mismatch.

## 15. Artifact Shape

Each `children.<name>` entry may additionally carry `semantic_hash: "sem1:<digest>"`.
When authoring Judge, obtain it from `scripts/judge.py semantic-hash --path <child-artifact>`
(installed: `.devspark/scripts/judge.py`), retaining the child's raw `hash`. Omit an unavailable
fingerprint, including when the helper/interpreter cannot run; do not restamp prior gates.
`bookkeeping-changed` is accepted wherever a child state is recorded or validated.

`FEATURE_DIR/gates/judge.md` holds one fenced `yaml` block. It does not copy the child reports.

```yaml
gate: judge
devspark_version: "<installed version, or `unknown`>"
generated: "<ISO-8601>"
feature: "<resolved feature directory name>"
planning_review:
  run_id: "<run id from judge.py resolve>"      # optional for legacy artifacts
round: 1
objective: "<one sentence>"
children:
  analyze: {artifact: gates/analyze.md, hash: "<sha>", state: fresh, ran: true,
            mode: generated, run_id: "<run id>"}
  critic:  {artifact: gates/critic.md, hash: "<sha>", state: fresh, ran: true,
            applicable: true, signals: [persistent-data-shape],
            mode: reused, artifact_run_id: "<earlier run id or empty>", accepted_by_run_id: "<run id>"}
concerns:
  - id: J-001
    sources: [critic-001]            # required, non-empty, each an existing child finding
    merge_basis: deterministic        # required when more than one source
    merge_reason: "Single root concern"
    classification: blocking-defect
    classification_source: child
    resolution: simplify
    change_shape: planning
    forcing_requirement: [FR-027]
    action: "Remove runtime policy machinery; use operator-reviewed dry-run"
    route: null                       # or {owner: verify, reason: "..."}
    future_work: null                 # required, with preserve: true, when resolution is defer
    needs_authority: true
    classification_change: null
decisions_needed:
  - {id: D-001, concerns: [J-001], question: "Remove OBSERVE mode?"}
  - {id: D-002, concerns: [J-004], question: "Is the documented rollback procedure sufficient, or must this spec add a rollback mechanism?"}
routed_obligations:
  - {source: critic-vo-002, owner: verify, reason: "Requires empirical real-Cosmos validation"}
verdict: CONDITIONAL
```

Every open child finding and every verification obligation appears in a concern's `sources` or in `routed_obligations`. A routed finding of another lane is information, not a finding to dispose.

A pending decision (`J-004` above) is shaped like this:

```yaml
- id: J-004
  sources: [critic-002]
  classification: deferred-work
  resolution: null
  needs_authority: true
  decision: {state: pending, question: "..."}
```

## 16. What the Developer Sees

One response. Counts, then only what needs attention:

```text
PLANNING REVIEW — <feature>

Analyze    <n> findings
Critic     <n> findings (or: not applicable — <signals>)
Judge      <n> root concerns

Current planning work   <n>
Simplifications         <n>
Accepted                <n>
Future work             <n>
Routed to Verify        <n>
Human decisions         <n>

Disposition             PROCEED | CONDITIONAL | STOP
```

Then show only: the human decisions, blocking current work, small required planning corrections, and important routed obligations. These counts are orientation, not quality targets — never optimize them.

## 17. Compatibility

Older Analyze and Critic artifacts without `classification`, `root_cause`, or freshness metadata remain valid input. Judge adapts them (`classification_source: judge-inferred`) and never rewrites a historical artifact. A route with no `judge.md` behaves exactly as before.

## 18. Non-Negotiables

1. Judge cites existing child findings only.
2. Judge writes only `gates/judge.md`.
3. Classification, resolution, change shape, and route stay separate fields.
4. A significant finding is never downgraded without `human`, `governing-requirement`, or `deterministic-evidence` authority.
5. Only `require` and `simplify` become current planning work.
6. Deferred work is preserved outside the spec; accepted limitations promise nothing.
7. Routed obligations stay part of this spec's completion.
8. The verdict is derived from concerns, never authored or averaged.
9. Staleness blocks only when it invalidates the evidence for a blocking decision.
10. Judge never lowers the bar for the developer to see a real blocker.
11. A pending human decision is an unset resolution, never a made-up one.

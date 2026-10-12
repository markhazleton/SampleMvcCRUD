---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Re-decide whether the active route is finished by classifying every open finding against the convergence rule, without re-running verification.
handoffs:
  - label: Create Pull Request
    agent: devspark-create-pr
    prompt: Open a PR now that the route has converged
  - label: Fix Blocking Defects
    agent: devspark-implement
    prompt: Address only the blocking defects recorded by closeout, then re-run closeout
    send: true
scripts:
  sh: .devspark/scripts/bash/check-prerequisites.sh --json --include-tasks
  ps: .devspark/scripts/powershell/check-prerequisites.ps1 -Json -IncludeTasks
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Optional / Advanced — Not a Step on the Normal Path

**`/devspark-verify` already does this.** Closeout is a lifecycle *phase*, not a lifecycle command, and verify runs it automatically as its second half (`closeout-contract.md` §6). On the ordinary path — specify → plan → implement → verify → create-pr → pr-review — a developer never needs to type this command, and no DevSpark command may recommend it as a routine next step.

This is the **reassessment entry point**: the same phase, re-run by hand, when the decision has gone stale or the inputs have changed without the evidence needing to be re-gathered. Reach for it when:

- work continued after verify ran, and the completion decision no longer reflects the route,
- a human wants to reassess completion without paying for an expensive re-verification,
- a previously blocking limitation has since been explicitly accepted, or
- an experimental result needs a fresh closeout decision.

If none of those apply and verification has not run at all, run `/devspark-verify` instead — it produces both the evidence and the decision in one interaction.

## Workflow Position

Replaces only the closeout phase of `/devspark-verify`, leaving the evidence phase untouched. The conceptual split is one question each:

- **Evidence** (verify steps 1–5) — "What does the evidence say?"
- **Closeout** (here, or verify step 6) — "Given that evidence, is this work finished?"

- **Owns**: resolving the stated objective, evaluating each acceptance criterion by its declared kind, classifying every open finding into exactly one of the four classifications, deciding `complete` / `not-complete`, and writing `FEATURE_DIR/gates/closeout.md`.
- **Does NOT own**: producing evidence (`/devspark-verify`), document consistency (`/devspark-analyze`), adversarial risk hypotheses (`/devspark-critic`), writing code (`/devspark-implement`), the PR (`/devspark-create-pr`), or archiving (`/devspark-release`).

## Constitution Authority

The project constitution (`/.knowledge/governance/constitution.md`) is **non-negotiable** within this command's scope. A closeout decision may never retire a constitution violation by classifying it — a violation is a Blocking Defect by definition, at the severity the constitution and `.knowledge/governance/severity-registry.md` assign it. If a principle itself needs to change, that happens through `/devspark-evolve-constitution`, visibly and on its own, never as a side effect of deciding that a piece of work is finished.

## Definition of Done

Done when `FEATURE_DIR/gates/closeout.md` exists with an objective outcome, every acceptance criterion evaluated, every open finding carrying exactly one classification with its classification-specific record populated, and an explicit `decision`. `not-complete` is a successful run of this command — it is a correct answer, not a failure to finish.

## Non-Negotiable: One Contract, Not Two Implementations

Everything this command does is defined by `/.devspark/templates/closeout-contract.md` (installed repos) or `templates/closeout-contract.md` (source repos). It is the identical phase `/devspark-verify` step 6 runs, against the identical rules, producing the identical artifact. Do not apply a looser standard here because verification is not being re-run, and do not invent a variant of the artifact — the only intentional difference is `produced_by: closeout`, which records that this decision was made without a fresh evidence phase beneath it.

## Non-Negotiable: Classify, Don't Discover

Closeout is a classification pass over findings that **already exist**. Read the gate artifacts, the verification evidence, the route record, and any review threads — do not open a new investigation, run a fresh audit, re-read the codebase looking for problems, or widen the evidence search. That is the loop this phase exists to stop (`closeout-contract.md` §1).

If a genuinely new blocking defect is noticed while reading, record it as a Blocking Defect and stop looking. Do not continue discovering.

## Non-Negotiable: Classification Is Not a Dodge

A failed `requirement` or a violated `invariant` is a Blocking Defect by definition. Choosing Deferred Work, Accepted Limitation, or Learning because Blocking Defect is inconvenient is the same class of dishonesty as fabricating verification evidence, and is judged the same way. If it is genuinely acceptable to ship without the criterion, the criterion itself must be amended through the authoring route — visibly and with its reason — never re-labeled here.

## Outline

Multi-app scope and the 2-tier override check for scripts are defined by the shared preamble contract core (`/.devspark/templates/command-preamble-contract.md`, or `templates/command-preamble-contract.md` in source repos), which you MUST load and obey before step 1 — in particular §10 (Convergence Discipline). Preamble modules: `convergence`, `orchestration`, `provenance`.

1. Run `{SCRIPT}` from repo root and parse FEATURE_DIR and AVAILABLE_DOCS. All paths must be absolute.

2. **Load the closeout contract** and execute its phase exactly as `/devspark-verify` step 6 does — same sub-steps, same authority (§3 criterion kinds, §4 classifications, §7 artifact shape and stop condition, §8 scope expansion):

   a. State the objective in one sentence, in the route's own words — read `FEATURE_DIR/spec.md`, or on the quickfix route the record at the path `FEATURE_SPEC` resolves to (`.devspark.work/quickfixes/<branch>.md`). Do not invent a more flattering objective, and do not substitute "everything the work touched" for the one the route declared.

   b. Collect the existing finding set by reading, not regenerating: `gates/verify.md` (mode statuses, evidence, and its `discovered:` list), `gates/analyze.md`, `gates/critic.md`, `gates/checklist.md`, the route record's unchecked items and `[NEEDS CLARIFICATION]` markers, open threads in `.devspark.work/pr-review/pr-<n>.md`, and anything the user named in `$ARGUMENTS`. A missing declared gate artifact is noted as missing.

   c. Evaluate each acceptance criterion by its declared kind, citing the evidence that settled it. An undeclared kind reads as `requirement`; a criterion with no evidence either way is `not-satisfied`.

   d. Classify every open finding, exactly one classification each, with its classification-specific record populated (`accepted_by`, `captured_as` pointing at a record that already exists, or `original_expectation` + `revised_understanding`). A `stage: post-deploy` requirement is `outcome: deferred` with its `evidence_class`, and follows closeout contract §4.1: `captured_as` must name an existing durable record that states owner, required deployed evidence, and open status (never an external record created to satisfy closeout); a high-risk deferral also needs `accepted_by`. A failed mandatory pre-merge requirement is never deferred.

   e. Apply the stop condition, then name the regression gate that keeps each claimed outcome true.

   f. Write `FEATURE_DIR/gates/closeout.md` in the contract §7 shape, with `produced_by: closeout`. Replace any prior `closeout.md` rather than appending.

3. **Account for the evidence this command deliberately did not refresh.** This run reused `gates/verify.md` instead of re-executing it. If the implementation changed materially since that evidence was captured, say so in the `summary` and treat the affected criteria as unsettled — a closeout resting on evidence that no longer describes the code is a decision about a previous version of the work. When the evidence is genuinely stale, the honest recommendation is `/devspark-verify`, not a `complete` decision here.

4. **Report.** Lead with the decision, then the counts per classification, then the blocking defects (if any) by name. Do not restate every classified finding in chat — the gate artifact carries the detail.

   - `decision: complete` → note readiness for `/devspark-create-pr` (ready to deploy, not deployed) and name any open `post-deploy` items as carried forward; they do not block merge.
   - `decision: not-complete` → route to `/devspark-implement` **for the blocking defects only**. Say explicitly that the accepted limitations, deferred work, and learnings are not part of that return trip; re-opening them is the expansion loop.

   A non-blocking finding MUST NOT become implementation work followed by another verification cycle in this route unless the developer explicitly asks for it (contract §8). If they do, record it as `promoted_by` — "the agent discovered this" and "the developer decided to include this" are different facts and must stay distinguishable.

**Autonomy override**: `--auto` (or a standing autonomy instruction) does not relax any classification requirement, and never converts a Blocking Defect into something softer. It only removes the confirmation prompts: `accepted_by` records the standing instruction, and a `deferred-work` finding with no obvious capture location is recorded as a Blocking Defect rather than being deferred to nowhere. `--auto` never promotes a finding into current scope — that needs a human.

## Context

$ARGUMENTS

---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Run Analyze, Critic when applicable, and Judge as one bounded planning review that returns a single planning decision before implementation.
handoffs:
  - label: Implement
    agent: devspark-implement
    prompt: Start the implementation now that planning review has proceeded
  - label: Apply Planning Dispositions
    agent: devspark-tasks
    prompt: Consume the Judge dispositions and append only the required planning work
scripts:
  sh: .devspark/scripts/bash/judge.sh resolve
  ps: .devspark/scripts/powershell/judge.ps1 resolve
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1. Preamble modules: `convergence`, `orchestration`, `provenance`.

Load and obey the Judge contract at `/.devspark/templates/judge-contract.md` (installed repos) or `templates/judge-contract.md` (source repos). It defines the adjudication rules; this command defines only the mechanics around them.

> **Analyze and Critic discover. Judge adjudicates scope.**
>
> **Judge does not discover.** **Judge does not remediate.**
>
> **A finding is evidence, not automatically work.**

Planning Review is the normal developer-facing assurance step between planning and implementation. Internally it is Analyze, Critic when applicable, and Judge. The developer reads **one** result. `/devspark-analyze` and `/devspark-critic` stay available for expert use, drill-down, and manual rerun.

## Workflow Position

- **Owns**: resolving the feature once, deciding which children must run, running them, reading their gate artifacts, adjudicating (Judge), and presenting one result.
- **Does NOT own**: discovering problems itself (Analyze and Critic), editing `spec.md`, `plan.md`, `tasks.md`, code, tests, or Knowledge (`/devspark-tasks` applies dispositions), or implementing.

## Definition of Done

Done when `FEATURE_DIR/gates/judge.md` exists, `judge.sh validate` passes, `guard end` reported no violation of a protected artifact, and the developer has seen exactly one planning-review result. `STOP` and `CONDITIONAL` are successful runs: they are correct answers.

## Outline

### 1. Resolve the Feature Once

`{SCRIPT}` has resolved the active feature (`FEATURE_DIR`, `feature`, and `handoff`). Do not resolve it again. If it reports an error (ambiguous or unknown), stop and ask the developer which feature — never guess.

Every child step below carries this **Planning Review hand-off**: `FEATURE_DIR` is the resolved directory, any script a child runs executes with `DEVSPARK_FEATURE` set to the resolved `feature` name, and `PLANNING_REVIEW_RUN_ID` is the run id `{SCRIPT}` generated once for this run (`run_id`). Every child that runs stamps it into its gate header as `planning_review.run_id`. A child never re-derives the feature from the branch name, which can differ from the spec directory.

### 2. Read Child State

Run `{judge helper} children --feature-dir <FEATURE_DIR>` (`.devspark/scripts/bash/judge.sh` or `.devspark/scripts/powershell/judge.ps1`; `scripts/…` in a source repo). Reuse what is fresh; do not repeat assurance work:

| Child state | Action |
|---|---|
| `fresh` | Reuse the artifact. No rerun. |
| `missing` | Run the child. |
| `stale-nonblocking`, `superseded` | Reuse. A focused recheck is optional. |
| `bookkeeping-changed` | Reuse without recheck. Compatible recorded semantics are unchanged. |
| `stale-recheck-required` | Follow the reported `action`: `rerun` runs the whole child; `recheck` re-evaluates only the affected findings. |

`--judge-only` skips every child run and adjudicates from the artifacts already present — for after the developer answered a decision, or after remediation and a focused recheck.

### 3. Run the Children

Analyze **always** runs when missing or in need of a rerun. Execute `templates/commands/analyze.md` as a phase of this run with the hand-off. It writes `gates/analyze.md`.

Critic runs only when `assurance-contract.md` §11 finds a change-shape signal in the spec/plan/tasks, or when the developer asked for it. Record the applicability decision. If Analyze reported a Blocking Defect that needs plan revision, skip Critic (`state: missing`, `ran: false`, with a `skipped_reason`) and adjudicate Analyze alone.

Pass **artifacts, not reasoning**, from each child to the next step. After each child, rerun `children` to confirm its gate artifact exists and is fresh.

**Seal what this run consumes.** After each child step (and once at the start of `--judge-only`), run `{judge helper} seal --feature-dir <FEATURE_DIR> --run-id <run_id> --generated <children this run just executed>`. The seal records, per child, the artifact hash and whether it was **generated** by this run (stamped with its run id) or **reused** (a fresh artifact from an earlier run, intentionally accepted by this run). If it reports `invalidated`, the artifact is not the one this run produced or accepted (a concurrent writer replaced it, or the child did not stamp the run id): rerun that child; never consume it. Reuse of a fresh artifact stays valid; a different run id alone never forces a rerun.

### 4. Judge

1. Arm the read-only guard: `{judge helper} guard begin --feature-dir <FEATURE_DIR> --run-id <run_id>`. It first re-verifies the seal; `invalidated` means a child changed after it was sealed, so rerun it.
2. Adjudicate per `judge-contract.md`: normalize, merge, classify, challenge proportionality (what can be removed or simplified while still satisfying every forcing requirement and invariant), derive dispositions. Work from the two gate artifacts plus the objective and requirement ids. Open another file only to check a cited `evidence:` pointer. A concern that needs a human choice is recorded with `resolution: null`, `needs_authority: true`, and a `decisions_needed` entry, never a made-up resolution.
3. Write `FEATURE_DIR/gates/judge.md` (replace, never append) in the contract §15 shape, including `planning_review.run_id` and each child's `mode` (`generated` or `reused`) with its run ids. Every concern cites existing child finding ids.
4. Disarm the guard: `{judge helper} guard end --feature-dir <FEATURE_DIR> --run-id <run_id>`. `violated` means a protected planning artifact (spec, plan, tasks, a child gate) changed during the Judge window: report the files, do not silently revert, and stop. `concurrent-change-detected` means only unprotected files moved (other work in the repository); it cannot be attributed to Judge, so note it and continue.
5. Run `{judge helper} validate --feature-dir <FEATURE_DIR>`. Fix **only** `judge.md` until it passes. A declared verdict that disagrees with the derived one is an error in `judge.md`.

If a concern seems to need a source that does not exist, that is a *new finding*, not a Judge concern. Route it back to Analyze or Critic by name and move on.

### 5. Present One Result

Use the §16 output shape of the Judge contract. Take the counts from `{judge helper} remediation --feature-dir <FEATURE_DIR>`. Show only the human decisions, blocking current work, small required planning corrections, and important routed obligations. Do not ask the developer to read the child reports.

| Verdict | Next |
|---|---|
| `PROCEED` | `/devspark-implement` |
| `CONDITIONAL` | Ask exactly the questions in `decisions_needed` (pending decisions create no task and no future work), then re-run with `--judge-only` to record the decision as a resolution |
| `STOP` | Read validated remediation owners: `plan_revisions` goes to `/devspark-plan` revise; `task_work` goes to `/devspark-tasks`; pending or ambiguous authority stops for the developer |

### 6. Bounded Convergence

One remediation round, then a focused recheck where `children` says one is required, then stop. A traceable remediation does not force a full rerun, and an artifact hash that changed is not by itself a reason to loop. A third adjudication round means ask the human.

Plan revise preserves unrelated decisions and refreshes affected support consumers. Tasks then
re-projects unchecked work, protects completed tasks and records the coherent Plan fingerprint.
Changed inputs permit focused review, not automatic resolution: an ineffective correction remains
STOP, while a satisfactory recheck may produce PROCEED. Reuse genuinely fresh children; rerun a
child only for changed assumptions requiring it. Neither an appended remediation heading nor new
task IDs are required to recognize a correction. No new lifecycle stage or state file is involved.

## Constraints

- Never edit a planning artifact, source, test, Knowledge file, or a child gate artifact.
- Never emit a concern, finding, or recommendation that no child finding supports.
- Never create a Future Work Plan here: Judge only identifies future work; `/devspark-tasks` materializes it.
- `--auto`: run through, but stop at `CONDITIONAL` and at `STOP`.

Follow the next-step footer contract in the preamble. Context: $ARGUMENTS

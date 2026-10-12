---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Detect where you are in the delivery lifecycle and run the single next DevSpark command after one confirmation.
handoffs:
  - label: Start New Work
    agent: devspark-specify
    prompt: Classify and route a new piece of work
  - label: Quick Fix
    agent: devspark-quickfix
    prompt: Start a lightweight fix for a small, contained change
scripts:
  sh: .devspark/scripts/bash/next-context.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/next-context.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Purpose

`/devspark-next` is the **single entry point for "what do I do now?"** It removes the need to remember which DevSpark command matches the current situation. It inspects the repository state, tells you the one next step in plain English, and — after a single confirmation — runs it for you.

A developer should be able to run `/devspark-next` at any point in a change and be guided correctly, without knowing the names of the underlying commands.

**Addressing the guide by name:** the developer may talk to this guide by its configured name (`ASSISTANT_NAME`, default `Spark`). Treat natural-language address as an invocation of this command — e.g. “hey Spark, where am I”, “Spark, what’s next?”, “Spark where are we”. When someone asks “where am I / where are we”, lead with the one-line orientation (repo, branch, platform) before the next step. This is especially useful with several repositories open at once.

## Workflow Position

This command sits *above* the lifecycle, not inside it. It does not author specs, write code, or open PRs itself — it detects state and dispatches to the right command (`/devspark-specify`, `/devspark-quickfix`, `/devspark-create-pr`, `/devspark-pr-review`, `/devspark-address-pr-review`, etc.).

- **Owns**: state detection, a plain-language recommendation, a single confirmation, and dispatch to the recommended command.
- **Does NOT own**: any of the underlying work (that stays with the specialized commands). It never merges code or bypasses a command's own gates.

## Definition of Done

Done when: the recommended next step has been presented with its reason, and either (a) the user confirmed and the recommended command was invoked / the manual action was clearly instructed, or (b) the user declined and no action was taken. Never run the recommended command before the user confirms (except under `--auto`, see below).

## Outline

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1 — it covers multi-app scope resolution and the 2-tier override check for scripts. Preamble modules: `orchestration`, `configuration`.

### 1. Detect State

Run `{SCRIPT}` from the repository root and parse the JSON. The script does the detection so you don't have to re-derive it — trust its output. Key fields:

- `ASSISTANT_NAME` — the developer-chosen name for this guide (from `.devspark.work/devspark.json` → `assistant.name`, env `DEVSPARK_ASSISTANT_NAME`, or default `Spark`). Speak as this name.
- `REPO_NAME`, `PLATFORM` — orientation facts for “where am I”: the repository name and its host (`github` | `azdo` | `gitlab`).
- `CURRENT_BRANCH`, `DEFAULT_BRANCH`, `IS_DEFAULT_BRANCH`
- `CONSTITUTION_EXISTS`, `DIRTY`, `HAS_REMOTE`, `BRANCH_PUSHED`, `BEHIND_TARGET`
- `SPEC_EXISTS`, `PLAN_EXISTS`, `TASKS_EXISTS`, `TASKS_DONE`/`TASKS_TOTAL`, `TASKS_COMPLETE`
- `SPEC_FEATURE`, `SPEC_AMBIGUOUS` — the active spec is resolved by identity, not by exact branch name: an exact spec directory wins, a quickfix record for the branch means a quickfix, otherwise the one spec directory sharing the branch's numeric prefix is the spec (so `132-fix-enhance-endpoint` resolves to `132-spec-complaint-catalog`). `DEVSPARK_FEATURE` overrides the branch. More than one match is never guessed: `STATE: ambiguous-spec`.
- `VERIFY_REQUIRED`, `VERIFY_PASSED` — passing evidence must cover every declared `verify:<mode>`; a top-level pass or a different passing mode is insufficient.
- `CLOSEOUT_DECISION` — `complete`, `not-complete`, or empty when the route has no closeout gate yet. `/devspark-verify` writes this gate itself as its second phase, so this is normally settled by the same command that produced the evidence. A `not-complete` decision means blocking defects are open: the next step is `/devspark-implement` **for those defects only**, never a re-run of everything, and never a separate closeout command.
- `QUICKFIX_EXISTS`, `QUICKFIX_COMPLETE`
- `PR_EXISTS`, `PR_NUMBER`, `PR_STATE`, `REVIEW_FILE_EXISTS`, `REVIEW_OPEN`
- `PENDING_GATES` — required-but-unmet advisory gates (checklist / analyze / critic / verify) surfaced for awareness; these never block forward progress. Note the gate order this implies: `plan` (produces `context_resolved`) → `tasks` (adds `code_ref`/`knowledge_ref` placeholders) → **planning review** (`/devspark-planning-review`: `analyze` validates that `context_resolved` resolves, `critic` judges whether it's sufficient, and `judge` adjudicates both into one planning decision) → `implement` (consumes it, never re-traverses more than one hop). The developer sees one step, not three. A `tasks-need-review` state with an empty `## Context Resolution` in `plan.md` is worth a plain-language nudge back to `/devspark-plan`, not just forward to planning review.
- `JUDGE_VERDICT` — the Planning Review verdict from `gates/judge.md` (`PROCEED`, `CONDITIONAL`, `STOP`), empty when no Judge artifact exists.
- `JUDGE_NEEDS_RECHECK` — `true` when a child gate needs a rerun or recheck, or the Judge artifact is stale (computed by `judge.py review-state`). **An unresolved Planning Review (missing where required, `STOP`, `CONDITIONAL`/pending decision, or needing a recheck) outranks task progress**: a finished task list never routes to implementation finalization until it is resolved.
- `PLAN_SPEC_STATE`, `TASKS_PLAN_STATE` — whether the plan still matches the spec it recorded (`spec_semantic_hash`) and whether the task list still matches the plan it recorded (`plan_semantic_hash`): `current`, `stale` (meaning changed since the recorded fingerprint) or `unknown` (no usable stamp or fingerprint). Both are present with or without a Judge artifact. Only `stale` routes: `plan-stale-against-spec` recommends `/devspark-plan` and `tasks-stale-against-plan` recommends `/devspark-tasks`. `unknown` never routes and never changes the recommendation. An unresolved Planning Review keeps precedence, so a spec change after Planning Review still goes to `review-needs-recheck` first, and a pending decision or `STOP` is resolved before either edge route.
- `RECOMMENDED_COMMAND` — the single next step the script computed
- `STATE` — a short state label (e.g. `no-active-work`, `spec-needs-plan`, `plan-stale-against-spec`, `plan-needs-tasks`, `tasks-stale-against-plan`, `tasks-need-review`, `review-blocked`, `review-needs-recheck`, `review-needs-decision`, `tasks-incomplete`, `implementation-needs-verification`, `verification-needs-closeout`, `closeout-not-complete`, `quickspec-needs-implement`, `quickfix-open`, `ready-for-pr`, `pr-needs-review`, `review-has-open-findings`, `behind-target`, `ready-to-merge`)
- `REASON` — a plain-language explanation to show the user (already includes any `PENDING_GATES` note)

The script's `RECOMMENDED_COMMAND` is authoritative for the common path. Only override it if the user's input in `$ARGUMENTS` explicitly asks for something different (e.g. "I want a full spec, not a quickfix") — in that case, explain the divergence.

Compatible `bookkeeping-changed` evidence does not request another planning review. Pending human
authority precedes automatic remediation; required Judge evidence cannot be replaced by child files.
After completed tasks, missing proof routes to Verify including its Closeout phase; missing legacy
Closeout routes to that same command for recovery. Blocking defects return to approved defect work,
with semantic planning changes stopped for authority. Satisfied proof and Closeout move forward to
the existing commit/PR/review/merge states. Retaining a completed bundle never triggers deletion or
another implementation pass.

For owned STOP recovery, `review-needs-plan` means a validated Plan correction is pending;
`plan-needs-reprojection` means its Plan changed but Tasks does not yet match it.
After authorized task correction or coherent re-projection, `review-needs-recheck` requests the
focused review. Changed content never clears STOP on its own. Pending authority, changed scope
and invalidated child adjudication precede this loop; unknown provenance must be reviewed rather
than guessed. Plan revise preserves existing content and never implies a destructive reset.

### 2. Present the Recommendation

Show the user a short, friendly summary — not a wall of state — in the voice of `{ASSISTANT_NAME}` (the developer's chosen guide name). Format:

```markdown
## {ASSISTANT_NAME} → next: {RECOMMENDED_COMMAND}

You're in **{REPO_NAME}** on `{CURRENT_BRANCH}` ({PLATFORM}).

{REASON}

_Where you are_: {STATE}.
```

Keep the persona light — a named signpost, not a character performance. The developer can address the guide by this name; honor it in your replies. (The persona names the guidance layer only; it does not change what any command does, and it is distinct from the underlying model identity.)

Interpret `RECOMMENDED_COMMAND` values:

| Value | What it means | Action on confirm |
| ----- | ------------- | ----------------- |
| `/devspark-*` | Run that DevSpark skill | Hand off to that skill |
| `commit` | Uncommitted work exists | Instruct the user to commit (or, mid-spec, hand off to `/devspark-implement`); do not auto-commit their code |
| `resolve` | More than one spec directory matches the branch's numeric prefix | Tell the user to set `DEVSPARK_FEATURE` to the intended directory name or remove the duplicate; never pick one |
| `sync` | Branch is behind target | Instruct the user to merge `origin/{DEFAULT_BRANCH}` in (per repo preference, merge — do not rebase — unless they ask) |
| `merge` | Ready to merge | Tell the user to approve and merge the PR in their Git portal (GitHub / Azure DevOps) — DevSpark does not merge for them |

### 3. Confirm, Then Run

Ask a single yes/no question: **"Run {RECOMMENDED_COMMAND} now?"**

- If **yes** and the recommendation is a `/devspark-*` skill → invoke that skill (pass through any relevant arguments the user supplied, e.g. a bug description for `/devspark-quickfix`).
- If **yes** and the recommendation is `commit` / `sync` / `merge` → these are actions DevSpark should not silently perform on the user's behalf. Give the exact command(s) to run and stop; do not fabricate a commit or a merge.
- If **no** → take no action. Optionally show the runner-up so the user can choose.

**Autonomy override**: if `--auto` (or a standing autonomy instruction) is in effect, skip the confirmation and dispatch the recommended `/devspark-*` skill directly. For `commit` / `sync` / `merge`, still do not perform the destructive/shared action automatically — report the exact command and stop, since those touch shared history or require human approval.

### 3a. `--auto` Chaining (carry me forward)

When `--auto` is in effect, `/devspark-next` does not stop after one step — it **loops**: run the recommended command (with `--auto`), then re-derive state by running `{SCRIPT}` again, then run the next recommended command, and so on. This lets a developer type `/devspark-next --auto` once and be carried through consecutive steps without re-prompting.

The loop MUST halt and hand back to the human at the first of these **stop conditions**:

- The recommendation becomes `commit`, `sync`, or `merge` (shared history / portal approval — never automated).
- The next command would **create or switch a branch** (e.g. dispatching `/devspark-specify` or `/devspark-quickfix` from the default branch). Branch changes are never automated — stop, name the branch that would be created, and let the human confirm (Branch Safety, non-negotiable).
- A dispatched command surfaces a blocking gate FAIL or an unresolved constitution violation.
- A command legitimately needs human input the autonomy instruction can't answer (e.g. an ambiguous route decision, a `one-off-fix` vs full-spec choice, missing credentials).
- The `STATE` stops advancing (the same recommendation repeats), to avoid an infinite loop.
- `ready-to-merge` is reached — the terminal success state for automation.

At every stop, report what was completed, the current `STATE`, and the single action the human needs to take. Never chain past a stop condition just because `--auto` is set.

### 4. Report

After dispatching (or instructing), state clearly what was done or what the user should do next, in one or two lines. Do not narrate the full state dump.

## Guidelines

- **One step at a time (default).** Without `--auto`, `/devspark-next` recommends exactly one action and stops. It is meant to be run repeatedly — after each step, run it again to get the next one. With `--auto`, it chains forward until a stop condition (§3a).
- **Trust the script.** Detection lives in `next-context.*` to save tokens and stay deterministic. Don't re-scan git or the filesystem yourself unless the script failed to run.
- **Surface, don't block, on advisory gates.** `PENDING_GATES` lists required-but-unmet gates (checklist / analyze / critic / verify). Mention them, but never let them stop forward progress toward the PR.
- **Never recommend `/devspark-closeout`.** Closeout is a phase of `/devspark-verify`, not a step the developer orchestrates. Name it only if the user explicitly asks to reassess completion without re-running verification.
- **A classified finding is not outstanding work.** Accepted limitations, deferred work, and learnings recorded in `gates/closeout.md` are converged, not pending — do not present them as reasons the route is unfinished or as candidates for the next step.
- **Never merge or force.** The router stops at the portal boundary for merges and never bypasses a command's own gates or confirmations.
- **Respect explicit user intent.** If the user says what they want in `$ARGUMENTS`, honor it over the default recommendation and explain the difference.

## Context

$ARGUMENTS

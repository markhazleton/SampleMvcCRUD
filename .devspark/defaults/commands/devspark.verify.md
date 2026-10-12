---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Produce empirical proof-of-change evidence (not document review) for the declared verification proof mode(s), classify what it surfaced, and decide whether the work is finished.
handoffs:
  - label: Create Pull Request
    agent: devspark-create-pr
    prompt: Open a PR now that the route has converged
  - label: Fix Blocking Defects
    agent: devspark-implement
    prompt: Address only the blocking defects recorded by closeout, then re-run verification
    send: true
  - label: Fix Failing Proof
    agent: devspark-implement
    prompt: Address the verify gate failure before re-running verification
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

## Workflow Position

Sits between `/devspark-implement` (code exists) and `/devspark-create-pr` (ship it). Produces empirical proof that the change works — distinct from `/devspark-analyze`/`/devspark-critic`, which review documents, not running code.

This command runs **two phases in one invocation**, and they answer different questions:

| Phase | Question | Artifact |
|---|---|---|
| **Evidence** (steps 1–5) | What does the evidence say? | `gates/verify.md` |
| **Closeout** (steps 6–7) | Given that evidence, is this work finished? | `gates/closeout.md` |

The phases are separate on purpose (`closeout-contract.md` §6) — evidence optimizes for finding things, closeout optimizes for converging — but the developer pays for one interaction, not two. Do **not** end a run by telling the user to go run `/devspark-closeout`; that command is an advanced reassessment entry point, not a step on this path.

- **Owns**: executing the proof mode(s) declared in `spec.md`'s `required_gates` (see `/.devspark/templates/verification-contract.md` or `templates/verification-contract.md` in source repos), recording actual commands + actual output as evidence, writing `FEATURE_DIR/gates/verify.md`, then classifying every open finding and writing the completion decision to `FEATURE_DIR/gates/closeout.md`.
- **Does NOT own**: re-litigating spec/plan/tasks consistency (`/devspark-analyze`), adversarial risk hypotheses (`/devspark-critic`), writing new tests or code (`/devspark-implement`), the PR itself (`/devspark-create-pr`).

## Definition of Done

Done when every `verify:<mode>` entry declared in `spec.md`'s `required_gates` has a corresponding `modes:` entry in `gates/verify.md` with real evidence and a `pass`/`fail` status, and `FEATURE_DIR/gates/closeout.md` carries an objective outcome, a classification for every open finding, and an explicit `decision`. If a mode cannot be executed in this environment, the gate records `fail` with the concrete blocker — never a fabricated pass. A `decision: not-complete` is a successful run of this command — it is a correct answer, not a failure to finish.

## Non-Negotiable: Evidence, Not Narration

Every evidence block MUST be the actual command(s) run in this session and their actual output (or a reference to a committed artifact, e.g. a CI log or fixture diff). A description of what *should* happen if the command were run is not evidence and MUST NOT be recorded as a `pass`.

## Non-Negotiable: Discovery Is Not Expansion

Running the real thing surfaces observations the document gates never could — an adjacent rough edge, a missing test, a better approach. Report every one of them. What this command MUST NOT do is convert them into new work inside the current spec on its own authority: that is the loop where verification silently expands the specification it was meant to close.

> Verification may discover work, but discovery alone does not expand the current specification.

An observation becomes mandatory work in this route only if it demonstrates one of the five expansion triggers (preamble §10): failure of the stated objective, an unresolved requirement, a violated invariant, a correctness defect, or a regression caused by this change. A failing proof mode is, by definition, one of those — record it `fail` and route back to `/devspark-implement`.

Everything else is carried into the closeout phase (step 6) and classified there. Record such observations under a `discovered:` list in the evidence artifact (step 5), and never let one change a mode's `status`.

The evidence phase keeps its own incentives clean by not being the phase that decides what the findings cost. **Find freely in steps 1–5; classify deliberately in step 6.** Do not soften, merge, or omit an observation because it looks like it would create work — step 6 exists precisely so that it usually doesn't.

## Genuine Fix Guard (command-preamble-contract.md §9)

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) — in particular §9 (Genuine Fix Discipline). Preamble modules: `finding-fix`, `convergence`, `orchestration`, `provenance`.

When the change under verification resolved a lint rule, complexity/length metric, or code-smell finding, a green linter run is **not** sufficient evidence of a genuine fix. The recorded evidence MUST also show the behavioral intent changed — e.g. the extracted unit is independently exercised by a probe, the previously swallowed error is now surfaced in output, the flattened path is covered by the same probe. A proof that only shows "the tool's number went down" while the probe's behavior is unchanged is a gamed fix: record that mode `status: fail` with the note "resolves the check, not the intent" and route back to `/devspark-implement`.

## Outline

Multi-app scope and the 2-tier override check for scripts are defined by the shared preamble contract already loaded above.

1. Run `{SCRIPT}` from repo root and parse FEATURE_DIR and AVAILABLE_DOCS. All paths must be absolute.

2. **Determine required proof modes**: Read `required_gates` from `spec.md`'s YAML frontmatter — or, if this branch has no `spec.md` (quickfix route), from the quickfix record's frontmatter at the path `FEATURE_SPEC` resolves to (`.devspark.work/quickfixes/<branch>.md`). Collect every `verify:<mode>` entry. If none are declared:
   - Check the change against the verify-sensitive shapes in verification contract §2.1. If the delta exhibits one and no mode was declared, say so and offer the indicated mode — an undeclared shape is the gap this check exists to catch, not a reason to proceed silently.
   - If the user explicitly asked for verification anyway, ask which mode applies (offer the table from the verification contract §2) and proceed with that mode for this run only — do not silently skip.
   - Otherwise, skip steps 3–5 and go straight to the closeout phase (step 6), noting in the report that no proof mode was declared for this route. A route with no required evidence still has an objective and findings to converge, and that is cheap to settle from the artifacts that already exist — it is not a reason to send the developer to another command.

   Report the outcome in the two-line form from verification contract §2.2, whichever way it lands.

3. **Load the verification contract**: Resolve `/.devspark/templates/verification-contract.md` (installed repos) or `templates/verification-contract.md` (source repos) and load it for the exact evidence requirements of each declared mode (§2 of that contract).

4. **Execute each declared mode.** For each `verify:<mode>` entry:

   - **`snapshot-neutral`**: Identify (or ask the user for) a deterministic probe that exercises the refactored code path (a test, a script, a CLI invocation, a rendered output). Capture its output at the pre-change commit (checkout/stash if needed), capture it again at the current commit, and diff the two. Record the exact commands and the diff (empty diff = pass; any non-empty diff must be individually justified as an intentional, named change or the mode fails).
   - **`golden`**: For each golden fixture affected by the change, render it before and after and compare byte-for-byte (or via the project's existing golden-test tooling if present). Record the fixture names, the comparison command, and the result.
   - **`regression-pinned`**: Identify the new/updated test(s) that pin the bugfix. Run them against the pre-fix code (via `git stash`, checkout of the parent commit, or a temporary revert) to confirm they fail, then run them against the current code to confirm they pass. Record both runs' actual output.
   - **`shadow-agreement`**: Identify the traffic/replay sample and the agreement threshold (from spec.md or ask the user if undeclared). Run old vs. new side-by-side against that sample, compute the agreement rate, and record the actual measured rate against the threshold.
   - **`end-to-end`**: Drive the real flow the feature enables (the actual CLI commands, API calls, or UI steps — not a description of them) and record the actual results at each step.

   If any mode cannot be executed (missing tooling, no traffic sample, no prior commit to diff against, etc.), record `status: fail` for that mode with the specific blocker — do not mark it `pass` and do not skip it silently.

   **Record the `evidence_class` actually obtained** on each mode entry (`automated`, `local-integration`, `deployed`, `rollout`; default `automated`), per verification contract §2.3. A run against a development or local dependency is `local-integration`, never `deployed`. A required pre-merge mode that can only be proven in a deployed environment is a spec **authoring defect**: record it `fail` with that reason, run and record the local portion as `local-integration`, and return to the spec (`/devspark-specify`) to move the deployment-dependent half to a `post-deploy` obligation. Do not skip the mode and do not mark it `pass`. `post-deploy` obligations are never `verify:<mode>` entries; they are carried as deferred requirements in step 6.

5. **Persist Gate Artifact**: Write `FEATURE_DIR/gates/verify.md` following the shape in verification-contract.md §4:

   ```yaml
   gate: verify
   devspark_version: "<installed version, or `unknown`>"
   generated: "<ISO-8601 timestamp of this run>"
   status: pass | warn | fail
   blocking: true | false
   summary: "<concise outcome>"
   modes:
     - mode: snapshot-neutral
       status: pass | fail
       evidence: |
         <actual commands + actual output>
   ```

   `devspark_version` is the version that produced **this** artifact (preamble contract §12). It is resolved through the shared version helper, and it does not restamp `spec.md` — a route whose spec was authored under an older version and verified under a newer one records exactly that.

   For every mode that passed by running a named, committed test (not an ad hoc probe), also record
   `test_ref: <path>::<test name>` alongside that mode's evidence. This is the citable pointer
   `/devspark-implement`'s evidence-preference bullet consumes when it records `verified_by:
   execution` on a `.knowledge/` node for this change — record it here, where the actual proof just
   ran, rather than leaving implement to reconstruct which test verified what.

   - `status: pass` only if every declared mode's entry is `pass`.
   - `status: fail` if any declared mode is `fail`; `blocking: true` in that case.
   - Ensure `FEATURE_DIR/gates/` exists; replace the prior `verify.md` artifact if present, don't append.

   Observations that surfaced during the run without hitting an expansion trigger go in a sibling `discovered:` list, each with a one-line summary — step 6 classifies them. They never affect any mode's `status`:

   ```yaml
   discovered:
     - "<what was observed while running the proof, for the closeout phase to classify>"
   ```

### Closeout Phase

<!-- markdownlint-disable MD029 -->
<!-- Step numbering deliberately continues across the phase heading: 6 and 7 are the
     second half of one invocation, not a new list. -->

6. **Classify and decide.** Load `/.devspark/templates/closeout-contract.md` (installed repos) or `templates/closeout-contract.md` (source repos) and run its phase here, in this same invocation. Its §3 (criterion kinds), §4 (classifications), §7 (artifact shape and stop condition), and §8 (scope expansion) are the authority for everything in this step.

   a. **State the objective** in one sentence, in the route's own words (the full spec's Rationale Summary → Core Problem, the quick spec's `## Intent`, the quickfix record's problem statement). If the route never stated a checkable objective, say so plainly — do not invent a more flattering one, and do not substitute "everything the work touched" for the objective it declared.

   b. **Collect the existing finding set — read, do not regenerate.** This step's `discovered:` list and mode results; open findings in `gates/analyze.md`, `gates/critic.md`, `gates/checklist.md`; unchecked items, open questions, and `[NEEDS CLARIFICATION]` markers in the route record; open threads in `.devspark.work/pr-review/pr-<n>.md` if a review exists. A missing declared gate artifact is noted as missing — that is a fact about the route's state, not an invitation to produce it now.

   c. **Evaluate each acceptance criterion by its declared kind.** For every `FR-###` and `SC-###` (or the quick/quickfix equivalent), record its `kind` (`requirement` | `invariant` | `hypothesis` | `target`) and `outcome`, citing the evidence that settled it. An undeclared kind reads as `requirement`. A criterion with no evidence either way is `not-satisfied`, not `satisfied`.

   d. **Classify every open finding** — exactly one of `blocking-defect`, `accepted-limitation`, `deferred-work`, or `learning` each, with the classification-specific record populated (`accepted_by`, `captured_as` pointing at a record that *already exists*, or `original_expectation` + `revised_understanding`). In each `rationale`, name which of the five expansion triggers the finding does or does not hit.

   e. **Decide** per contract §7: `complete` requires the objective `achieved` or `resolved-through-learning`, zero `blocking-defect` findings, and an empty `unclassified` list. Then **name the regression gates** — for each outcome being claimed, the test or check that keeps it true (prefer a `test_ref` recorded in step 5). An outcome with no regression gate is worth saying out loud.

   f. **Write `FEATURE_DIR/gates/closeout.md`** in the contract §7 shape with `produced_by: verify`. Replace any prior `closeout.md`; do not append.

   **Post-deploy requirements.** A criterion declared `stage: post-deploy` (default is `pre-merge`) is recorded `outcome: deferred` with its `evidence_class` and a Deferred Work finding whose `captured_as` is an existing durable record (closeout contract §4.1) that verify never creates. A missing or hypothetical record leaves the finding unclassified and the decision `not-complete`; a high-risk deferral also needs `accepted_by`. Never record it `satisfied` on `automated` or `local-integration` evidence.

   **Classification is not a dodge.** A failed `requirement` or violated `invariant` is a Blocking Defect by definition, and choosing a softer classification because Blocking Defect is inconvenient is the same class of dishonesty as fabricating evidence in step 4. If it is genuinely acceptable to ship without the criterion, the criterion is amended through the authoring route — visibly, with its reason — never relabeled here.

   **Classify; do not re-discover.** This step reads findings that already exist. It MUST NOT open a new investigation, re-read the codebase hunting for problems, or widen the evidence search — that re-enters the loop this phase exists to stop. If a genuinely new blocking defect is noticed while reading, record it and stop looking.

7. **Report once, for both phases.** Lead with the per-mode outcome, then the closeout summary in the contract §7 shape (objective, counts per classification, unclassified count, regression gates, decision). Do not restate every classified finding in chat — the artifacts carry the detail.

   - `decision: complete` → recommend `/devspark-create-pr`. Name the accepted limitations, deferred work, and learnings as **carried forward, not outstanding** — they are not reasons this route is unfinished. State that the change is ready to deploy, not deployed, and list any open `post-deploy` items with their `captured_as` records.
   - `decision: not-complete` → route to `/devspark-implement` **for the blocking defects only**, and say so explicitly. Re-opening the classified findings on that return trip is the expansion loop.
   - Never end by telling the user to run `/devspark-closeout`. The decision was just made here. Mention that command only if this run genuinely could not produce a decision, and say why.

   A non-blocking finding MUST NOT be turned into implementation work followed by another verification cycle in this route unless the developer explicitly asks for it (contract §8). If they do, record it as `promoted_by` — the distinction between "the agent discovered this" and "the developer decided to include this" is the whole point.

**Autonomy override**: `--auto` (or a standing autonomy instruction) does not change the evidence requirement — it never authorizes a fabricated pass — and does not relax any classification requirement or soften a Blocking Defect. It only removes confirmation prompts: in step 2, defaulting to the most conservative applicable mode from the verification contract's table; in step 6, recording the standing instruction as `accepted_by`, and recording a deferrable finding with no obvious capture location as a Blocking Defect rather than deferring it to nowhere. `--auto` never promotes a finding into current scope (§8) — that needs a human.

<!-- markdownlint-enable MD029 -->

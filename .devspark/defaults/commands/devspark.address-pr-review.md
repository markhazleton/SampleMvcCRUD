---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Address open PR review findings with enforced commit isolation between code fixes and review-file updates
handoffs:
  - label: Re-Review Updated PR
    agent: devspark-pr-review
    prompt: Run /devspark-pr-review UPDATE for this PR after fixes are committed
scripts:
  sh: .devspark/scripts/bash/address-pr-review.sh --pr-id $ARGUMENTS --json
  ps: .devspark/scripts/powershell/address-pr-review.ps1 -PrId $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before Phase 0 — in particular §9 (Genuine Fix Discipline) during the Phase 2 fix loop. Preamble modules: `finding-fix`, `convergence`, `configuration`.

This command is the **author-side companion** to `/devspark-pr-review`. It helps you address open findings in `/.devspark.work/pr-review/pr-{PR_ID}.md` while enforcing commit isolation:

1. Commit code fixes first.
2. Commit review-file updates second.

**IMPORTANT**: The staging gates are mechanical and mandatory. Do not bypass them.

## Prerequisites

- Existing PR review file at `/.devspark.work/pr-review/pr-{PR_ID}.md`
- Git repository with the PR source branch checked out
- PowerShell 7+ (`pwsh`) on Windows, or Bash on macOS/Linux, for the gate helper script

## Definition of Done

Done when: every selected finding is fixed and committed (Phase 4), the review file is updated and committed separately (Phase 6), the two commits are verified disjoint, and the handoff message is printed (Phase 7). If validation (Phase 3) can't be made to pass, stop and report which finding is blocking — don't keep retrying silently.

## Outline

### Phase 0 — Load context

> **Script Resolution**: Before running `{SCRIPT}`, apply the 2-tier override check — if `.devspark.work/scripts/powershell/address-pr-review.ps1` (PowerShell) or `.devspark.work/scripts/bash/address-pr-review.sh` (Bash) exists on disk, run that file instead, preserving all arguments. Team overrides in `.devspark.work/scripts/` always take priority over `.devspark/scripts/`.

1. Run `{SCRIPT}` with the PR id and JSON output enabled (`-PrId {PR_ID} -Json` on PowerShell, `--pr-id {PR_ID} --json` on Bash).
2. Fail fast if `/.devspark.work/pr-review/pr-{PR_ID}.md` is missing.
3. Parse open findings from checklist lines for `C-##`, `H-##`, `M-##`, `L-##`, `CON-##` and `PRD{N}-##` ids (`- [ ] **ID**`). The helper reports each finding's `status`, `blocking`, `route`, `auto_eligible` and `reason`; use those values instead of inferring eligibility from an ID prefix.
4. Confirm current branch equals the PR source branch. Refuse if mismatched.
5. Capture `git status --short`.
6. **Refuse to proceed** if any staged path matches `.devspark.work/pr-review/pr-*.md`.

If no open findings remain, print: `Nothing to address.` and stop.

### Phase 1 — Plan

1. Render open findings as a checklist with severity badges.
2. Ask which findings to address this iteration (`all` allowed). **Autonomy override**: if `--auto` (or a standing autonomy instruction) is in effect, skip the ask and select only findings with `auto_eligible: true` (open critical/high or blocking PR-delta findings with no accepted, deferred, resolved or withdrawn disposition). Report every other open finding with its `reason` and leave it untouched: ordinary Medium/Low and non-blocking `PRD` findings need a human decision, and `CON` findings route to `/devspark-evolve-constitution` rather than product code. A selected `C-NN` finding whose fix is ambiguous enough to need a human judgment call is flagged and skipped.
3. Build an internal todo list with one item per selected finding.

### Phase 2 — Fix loop (per finding)

For each selected finding:

1. Read the cited file/lines and confirm the issue.
2. If the finding cites a lint rule, complexity metric, or code smell, state in one sentence the behavioral intent behind it (command-preamble-contract.md §9) before writing the fix — do not react to the bare metric alone.
3. A finding's suggested fix is evidence, not an architecture mandate. Choose the lightest sufficient remediation, in this order: clarify, remove, reuse something that exists, change an operational detail, make a small local change, and only then add machinery. If the fix needs something beyond the approved Plan's shape (a new state, endpoint, persisted record, cache, dependency, runtime setting, pattern or rollback design), stop and ask for explicit authority; `--auto` never grants it. Show the diff for any alternative to the suggested fix, and confirm the fix addresses that intent (e.g., genuinely lowers branching/responsibility, genuinely names and surfaces an error) rather than merely satisfying the check mechanically.
4. **Rule 1 check**: the fix stays subject to the same one-way reference gate `/devspark-pr-review` §6 enforces — do not introduce a spec ID, task ID, plan reference, or PR-thread ID into code or `.knowledge/` to "explain" what review comment this addresses. A commit message or the review file itself is the right place for that context, never a durable comment.
5. Stage **only** code paths touched by that fix.
6. Never run `git add .`.
7. Never stage `/.devspark.work/pr-review/pr-{PR_ID}.md` during this phase.

### Phase 3 — Validate

1. Re-run the **locked pytest scope** from the review file `Stats` table (reuse the same command; do not pick a new scope).
2. Re-run project-specific validators explicitly recorded in the review file.
3. If any validation fails, return to Phase 2.
4. Do not continue until validations pass.

### Phase 4 — Commit code fixes (isolation gate #1)

1. Run gate script with code-only mode before commit (`-Gate code-only` on PowerShell, `--gate code-only` on Bash).
2. If the gate fails, **abort** and print offending staged paths.
3. Review staged diff and commit with:

```text
fix(pr-{PR_ID}): address {M-02,M-04,M-05}
```

1. Capture the resulting short hash as `{FIX_SHA}`.

### Phase 5 — Update the review file

For each fixed finding:

1. Flip `- [ ]` to `- [x]`.
2. Append `— *Fixed in {FIX_SHA}: {one-line how}*` to the finding heading line.
3. Do **not** change finding IDs, descriptions, or broken/fix code blocks.

Then update metadata:

1. Bump revision in the header table (`Rev N -> Rev N+1`).
2. Update `Stats` with current churn/test counts/commit snapshot.
3. Append a new row to `Revision Log` for this iteration.
4. Stage only `/.devspark.work/pr-review/pr-{PR_ID}.md`.

### Phase 6 — Commit review file (isolation gate #2)

1. Run gate script with review-only mode before commit (`-Gate review-only` on PowerShell, `--gate review-only` on Bash).
2. If the gate fails, **abort** and print offending staged paths.
3. Commit with:

```text
review(pr-{PR_ID}): rev {N} — {X} fixed, {Y} remaining
```

1. Verify commit disjointness:

```bash
git log HEAD~2..HEAD --name-only
```

Parse the two commits and assert they share zero file paths.

### Phase 7 — Handoff

1. Print both new commit hashes (`fix` and `review`).
2. Suggest focused re-review:

```text
Run `/devspark-pr-review UPDATE {PR_URL}` to trigger a focused re-review.
```

## Guidelines

### Commit Discipline (MUST)

- A commit touching `/.devspark.work/pr-review/pr-{PR_ID}.md` MUST NOT include any other path.
- Code fixes and review-file updates MUST be separate commits.
- Do not amend/squash these two commits together.

### Gate Execution (MUST)

- Use the helper script as the source of truth for staging gates.
- If a gate exits non-zero, stop and resolve staging before retrying.

### Edit Scope (MUST)

In review files, limit edits to:

- finding checkbox state
- heading-line fixed-in suffix
- revision metadata (`Revision`, `Stats`, `Revision Log`)

Everything else is immutable during addressing.

## Context

$ARGUMENTS

## Shared Review Resolution Contract Output

When emitting findings (review observations, issues, recommendations), structure each entry to include the shared resolution contract fields so downstream tools (/devspark-address-pr-review, telemetry, release) can act on them deterministically:

```yaml
findings:
  - finding_id: <stable-id-unique-within-this-command-output>   # e.g., analyze-001, clarify-002
    severity: critical | high | medium | low
    description: <1-3 sentence problem statement>
    recommended_action: <machine-actionable next step>
    execution_mode: auto | selective | manual
    status: open                                                  # set to `resolved` after remediation
    outcome: ""                                                  # populated post-resolution by address-pr-review
```

inding_id MUST be stable across re-runs when the underlying issue is unchanged. xecution_mode MUST be one of: `auto` (safe to apply automatically), `selective` (apply with reviewer approval), `manual` (requires human implementation). The `status` and `outcome` fields are written by `/devspark-address-pr-review` (FR-028).

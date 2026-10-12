<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

````markdown
---
description: Merge the latest remote main into the current branch with a script; the agent only resolves merge conflicts
scripts:
  sh: .devspark/scripts/bash/merge-from-main.sh $ARGUMENTS
  ps: .devspark/scripts/powershell/merge-from-main.ps1 $ARGUMENTS
---

## User Input

```text
$ARGUMENTS
```

Supported flags (bash / PowerShell): `--base <branch>` / `-Base` (default `main`), `--remote <name>` /
`-Remote` (default `origin`), `--push` / `-Push`. Pass only flags the user gave.

## Token budget

This command is deliberately script-first. Do NOT load the preamble contract, constitution,
`.knowledge/`, specs, or any file not named below. Do not run extra git commands to "check" what
the script already reports. Reply in at most a few lines.

## Steps

1. Run `{SCRIPT}` (a `.devspark.work/scripts/{bash,powershell}/merge-from-main.*` override wins if
   present). Read only its `STATUS:` block.
2. By `STATUS`:
   - `up-to-date`, `merged`, `fast-forwarded`, `aborted`, `resolved` — report the one-line status
     (branch, behind count, files changed, pushed) and stop.
   - `blocked` or `error` — relay `MESSAGE` (and any `DIRTY`/`MARKERS_REMAIN` lines) and stop.
     Never stash, reset, commit unrelated changes, or force anything to get past it.
   - `conflicts` — go to step 3.
3. Resolve conflicts. Each `CONFLICTS` line is `<state> <path> L<start>-<end>,...`.
   - Read ONLY the listed line ranges (plus a few lines of context if needed), not whole files.
   - Edit each hunk to the correct combined result, keeping the intent of both sides; remove every
     `<<<<<<<`, `=======`, `>>>>>>>` marker. Do not touch code outside the hunks.
   - States without line ranges (`DU`, `UD`, `AU`, `UA`, `DD`, binary) are modify/delete or binary
     conflicts: keep or delete the file per the intent of both sides; if the right choice is not
     clear, stop and ask the developer.
   - If a hunk's correct resolution is genuinely ambiguous, ask the developer instead of guessing.
     The developer may choose to abort: rerun `{SCRIPT}` with `--abort` / `-Abort`.
4. Rerun `{SCRIPT}` with `--continue` / `-Continue` (keep `--push` / `-Push` if originally given).
   It verifies no markers remain, stages the resolved paths, and commits the merge with git's
   default message. If it returns `blocked` with `MARKERS_REMAIN`, fix only those lines and repeat
   this step.
5. Report: conflicted files resolved, one short note per non-trivial resolution, final status.

Push only when the user passed `--push` / `-Push`.
````

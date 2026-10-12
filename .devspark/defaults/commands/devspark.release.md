---
source: "BSW.DevSpark - (c) 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Seal a release from the verified repository delta, sweep .devspark.work retention and archive-eligible work, and keep current knowledge accurate.
handoffs:
  - label: Run Final Audit
    agent: devspark-site-audit
    prompt: Run a final delta and documentation audit before release
scripts:
  sh: .devspark/scripts/bash/release-context.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/release-context.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You MUST consider non-empty user input. Load and obey the shared command preamble contract before
step 1.

## Purpose

Release seals the verified delta between the previous release and HEAD, and is the **sole** command
that ever moves anything from `.devspark.work/` into `.archive/{YYYY-MM-DD}/`. `{YYYY-MM-DD}` is
always *this release's own cut date* (today, the date this run tags) — never an individual swept
artifact's own internal date, timestamp, or filename stamp. Every artifact swept in steps 4–5 goes
into the one archive folder for the release being cut right now, regardless of what date it happens
to be stamped with internally. Its sources are code, current `.knowledge/`, tests, verification
results, Git history, merged PR metadata, and existing user-facing documentation.

`/devspark-implement` deliberately leaves every completed spec and quickfix record live in
`.devspark.work/` — its populated `code_ref`/`knowledge_ref` linkage is what lets anyone validate the
delta against code, tests, and knowledge for as long as it stays there, which can span more than one
sprint. Release is what eventually archives it, and also runs the maintenance sweep: work-product
retention, orphaned in-flight state, current-knowledge backstop checks, and leaked-planning-identifier
scans all run here.

## Options

| Option              | Behavior                                                                                     |
| ------------------- | -------------------------------------------------------------------------------------------- |
| `{version}`         | Use the explicit semantic version                                                            |
| `--from <ref>`      | Override the release-window base ref                                                         |
| `--scope=work`      | Evaluate `.devspark.work/` retention and archiving only; skip the version/CHANGELOG workflow |
| `--scope=knowledge` | Evaluate current-knowledge currentness and code/knowledge consistency only                   |
| `--scope=scan`      | Report candidates only; make no changes                                                      |
| `--dry-run`         | Print the complete plan without writing files                                                |

## Workflow

1. Resolve repository or app scope and run the release-context script. Its JSON output is
   diagnostic input for this run only — never commit it to Git under any name (e.g.
   `release-context-*.json`). Check the script's `STRAY_RELEASE_ARTIFACTS` field: any match is a
   file matching that pattern that was already committed by a prior, interrupted release run that
   never reached step 10's version bump/tag. Treat it as stray, remove it, and do not treat its
   presence as evidence that prior release work exists.
2. Establish the release window from the previous version tag to HEAD. Use commits and merged PR
   metadata to classify the semantic version bump.
3. Verify the delta:
   - required tests, lint, builds, and declared release gates pass;
   - changed behavior and current `.knowledge/` are complete and mutually consistent;
   - `.knowledge/index.json` is current;
   - `.knowledge/` structural completeness and knowledge/code mapping accuracy are clean — require a
     current `/devspark-site-audit` result (full scope, or at least `--scope=knowledge`) covering
     this release window with no open `KNOW1`–`KNOW6` findings. Re-run site-audit's knowledge scope
     now if the most recent result predates the window; release does not duplicate that check
     itself, only confirms it has run and passed;
   - shipped code, tests, and `.knowledge/` contain no planning identifiers.
4. **Sweep `.devspark.work/specs/` and `.devspark.work/quickfixes/` for archive-eligible work** — this
   is the only place in DevSpark this happens. The release context computes this classification for
   you: `ARCHIVE_ELIGIBLE` lists the bundles that qualify, `ARCHIVE_BLOCKED` lists each remaining
   bundle with its `blockers`, and `ORPHAN_CANDIDATES` lists bundles whose branch no longer exists.
   Treat those fields as the decision, and spot-check rather than re-derive them:
   - A bundle is **archive-eligible** when its own completion marker reads `Complete` (`spec.md`
     `**Status**`, or the quickfix record's completion marker) and every task/action-plan/checklist
     item's `code_ref`/`knowledge_ref` is populated (real path(s), or an explained `n/a`) — the same
     linkage `/devspark-implement` populated and left in place. Confirm the linkage is still populated
     now; do not re-derive it.
   - Move every archive-eligible bundle into `.archive/{YYYY-MM-DD}/` (preserving its relative path
     minus the `.devspark.work/` prefix, e.g. `.devspark.work/specs/042-foo/` →
     `.archive/{YYYY-MM-DD}/specs/042-foo/`), whole and intact — plan, tasks, checklists, gates,
     `context_resolved`. Never leave a copy behind under `.devspark.work/`, and never move it to
     `.knowledge/`.
   - A bundle that is **not yet** `Complete`, or is `Complete` but still has an unpopulated
     `code_ref`/`knowledge_ref` marker, stays exactly where it is — spanning more than one release
     window is normal, not an error. Report it as still in flight; never archive it.
   - Separately, flag a bundle whose branch is stale, merged, or deleted but that never reached
     `Complete` — this is orphaned, abandoned in-flight state, not a normal rollover. Surface it for
     human disposition; never archive it automatically.
5. Move every other categorized `.devspark.work/` artifact — `pr-review/`, `audit/`, `metrics/`,
   `telemetry/` per `.knowledge/taxonomy-registry.json` — into `.archive/{YYYY-MM-DD}/` the same way.
   Every release sweeps all of them unconditionally: there is no age, count, or eligibility check for
   these categories, unlike specs/quickfixes in step 4. Never delete outright, so nothing committed
   only moments ago is lost if it wasn't yet captured elsewhere in Git history. (`.devspark.work/runs/`
   no longer exists — it was tied to the now-removed CLI; ignore any leftover copy as stray, not a
   category to sweep.)
6. Check `.knowledge/` for obsolete, duplicate, historical, decision-log, rationale-archive,
   lifecycle, or supersession content as a lighter, non-blocking backstop — `/devspark-pr-review` and
   `/devspark-site-audit` are the authoritative checks for this; release only catches what they missed
   between runs. This includes `.knowledge/governance/decisions/` docs: flag any that have drifted
   into a numbered/ADR-style filename, a "superseded by" chain, or a `constrains`/`constrained_by`
   pointer that no longer matches an existing entity — correct in place or delete, never archive.
7. Scan shipped code and `.knowledge/` for leaked spec, task, feature, quickfix, or requirement
   identifiers. Remove the reference; do not rewrite it into another planning reference.
8. Generate user-facing CHANGELOG and release notes from the release delta. Do not mention archived
   spec, task, quickfix, feature, or requirement identifiers.
9. Write operational release output — including a summary of what was archived and what retention
   swept — directly to `.archive/{YYYY-MM-DD}/release/`. Release output is never staged under
   `.devspark.work/`, even temporarily: `.archive/` is the only place any of this release's output
   lives once the run completes.
10. Bump source version files and the `.devspark/BSW.DevSpark.version` stamp where applicable.
11. Show the exact file, archive, and tag plan. Require explicit confirmation before commits,
    archiving, tags, pushes, or publication; `--dry-run` never writes.
12. Re-run release gates, create the release commit/tag only after success, and report publication
    status from the host.

Git is the sole history for `.knowledge/`: release never archives it, only corrects it in place or
deletes it. `.devspark.work/` retained work products, archive-eligible specs/quickfixes, and orphaned
planning bundles are the one exception — those move to `.archive/{YYYY-MM-DD}/` per steps 4–5 above,
never deleted outright. No DevSpark command, release included, reads, lists, or globs `.archive/`
afterward; a human decides when an entry there is safe to remove, outside any DevSpark command.

Where you are: release sealed, `.devspark.work/` swept
Next: monitor for the next release window

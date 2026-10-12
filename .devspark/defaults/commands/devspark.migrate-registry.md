---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Move the multi-app registry out of the legacy combined devspark.json into a new, dedicated devspark.registry.json (optional, human-confirmed).
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Purpose

`devspark.json` historically did double duty as both repo facts/team policy (`platform`/`organization`/`policies`) and the multi-app registry (`version`/`mode`/`profiles`/`apps`) in one file. `/devspark-migrate-registry` moves the registry keys into a new, dedicated `devspark.registry.json`, leaving `devspark.json` for repo facts only.

**This is entirely optional.** The legacy combined format keeps working forever via a permanent fallback — nothing requires migration, and no other command ever runs this automatically. Run it only if you want the clean split.

## Outline

Run `devspark migrate-registry` (or `devspark migrate-registry --yes` to skip the confirmation prompt).

1. **Preflight checks** — reports and stops (no changes) if:
   - `devspark.registry.json` already exists (nothing to migrate)
   - `devspark.json` doesn't exist (nothing to migrate)
   - `devspark.json` has no registry keys (`version`/`mode`/`profiles`/`apps`) to move

2. **Show the migration plan** — how many apps/profiles will move, and which repo-facts keys remain in `devspark.json`.

3. **Confirm** — unless `--yes` is passed, ask before writing anything.

4. **Write both files**:
   - `.devspark.work/devspark.registry.json` — the new file, containing `version`/`mode`/`profiles`/`apps`
   - `.devspark.work/devspark.json` — rewritten with only the remaining repo-facts keys (never deleted, even if empty)

## Constraints

- Never runs automatically — always a separate, explicit, human-confirmed command
- Never deletes `devspark.json`, even if no repo-facts keys remain
- `.devspark/` (framework-installed) is never touched

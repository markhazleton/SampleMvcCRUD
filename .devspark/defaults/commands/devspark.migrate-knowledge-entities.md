---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Move existing flat .knowledge/ docs (architecture, patterns, operations) into the opt-in entity-ontology layout under .knowledge/entities/<id>/, then rebuild the knowledge index (optional, human-confirmed).
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Purpose

`.knowledge/` supports two coexisting content models: flat type-folder docs (`architecture/`,
`patterns/`, `operations/`, classified by `type:` frontmatter — the default, no setup required) and
the entity-ontology layout (`entities/<id>/_entity.yaml` plus per-layer docs such as
`architecture.md`, `business.md`, `operations.md`). `/devspark-migrate-knowledge-entities` moves
existing flat docs into entity folders and regenerates the knowledge index.

**This is entirely optional.** Flat type-folder docs keep working forever — nothing requires
entities, no other command runs this automatically, and per constitution Principle I this migration
is always explicit, human-run, and reversible via `git`. Adopt entities per-domain, incrementally;
you don't have to migrate every doc in one pass.

## Outline

1. **Fetch the script.** Retrieve `scripts/migrate-knowledge-to-entities.py` from the same
   cache-busted BSW distribution source used by the install/upgrade quickstart (never a
   webpage-fetch tool — see that guide's Step 0).

2. **Dry-run first.**

   ```bash
   python scripts/migrate-knowledge-to-entities.py --repo-root . --dry-run --json
   ```

   Show every proposed move: source path, destination `entities/<id>/<layer>.md`, and the derived
   entity id (from frontmatter `id`, else a slug of the filename). The script only ever reads
   `.knowledge/architecture/`, `.knowledge/patterns/`, `.knowledge/operations/` — governance docs
   under `.knowledge/governance/**` are never touched or considered domain entities.

3. **Confirm.** Ask for explicit confirmation before writing anything. If the user wants to migrate
   only some docs, filter the plan and note the rest remain as flat docs (a fully supported,
   permanent state).

4. **Apply with `git mv`.**

   ```bash
   python scripts/migrate-knowledge-to-entities.py --repo-root . --yes
   ```

   The script uses `git mv` so history follows each file. It never overwrites an existing
   `entities/<id>/` doc — stop and reconcile manually if a destination already exists.

5. **Fill in `_entity.yaml`.** For each newly created entity folder, review or create
   `.knowledge/entities/<id>/_entity.yaml` with `id`, `type`, `owner`, and any `relations` — the
   script does not infer these; flag them for manual review rather than guessing.

6. **Rebuild the index.**

   ```bash
   python scripts/build_knowledge_index.py --repo-root .
   ```

   Regenerates `index.json` and reports any coverage gaps (missing layer docs, unresolved
   `relations[].object` references, missing `constrains`/`constrained_by` reciprocity) introduced
   by the move. Resolve gaps before considering the migration complete.

## Constraints

- Never runs automatically — always a separate, explicit, human-confirmed command
- Never touches `.knowledge/governance/**` — governance docs are not domain entities
- Never overwrites an existing `entities/<id>/` destination; stop and reconcile conflicts manually
- Never creates `.old/`, or history folders — `git mv` preserves history in place
- Flat type-folder docs remain fully supported; this command is additive, not a forced conversion

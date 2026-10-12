# CLAUDE.md — Agent Instructions

## BSW.DevSpark Commands

This project uses BSW.DevSpark for spec-driven development. Available slash commands:

- `/devspark-specify` — Define requirements and user stories
- `/devspark-plan` — Create implementation plan
- `/devspark-tasks` — Break plan into actionable tasks
- `/devspark-implement` — Execute tasks
- `/devspark-create-pr` — Draft or update a pull request
- `/devspark-pr-review` — Constitution-based PR review
- `/devspark-quickfix` — Lightweight bug fix workflow

See `.devspark/defaults/commands/` for the full list.

## Constitution

Read `.knowledge/governance/constitution.md` before making changes — it defines the project's non-negotiable principles.

## History

Migrated from DevSpark v2.6.0 to Bold (2026-07-12), then converted to BSW.DevSpark (2026-10-11). `.archive/index.md` holds the earlier migration record and DevSpark-era documentation. Reference docs, ADRs and prior feature records are in `.knowledge/`.

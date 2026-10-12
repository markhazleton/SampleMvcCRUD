# Future Work Plan Template

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

A Future Work Plan preserves a real discovery that was intentionally removed from the current spec's scope. It is written by the remediation step (`scripts/judge.py future-work --write`), never by Judge itself, and lives under `.devspark.work/intake/` beside other pre-spec material.

> **Capture discovery without converting discovery into scope.**
>
> **Creating a Future Work Plan does not authorize additional investigation beyond what is necessary to preserve the discovery.**

It is deliberately not a spec. It has no task list, no required gates, no implementation status, and no acceptance criteria. When someone activates it, its contents become the `VERIFIED FINDINGS` of an intake for `/devspark-specify`.

## Shape

```markdown
# Future Work Plan: <title>

**Type**: bug | enhancement | architecture | operational | research

## Problem

## Originating Spec

## Why Not Current Scope

## Evidence Already Discovered

## Candidate Direction

## Constraints

## Open Questions

## Activation Trigger

## Provenance
```

Fields not captured stay as the line `Not captured. A Future Work Plan does not authorize further investigation.` — never fill them by investigating.

## Where It Can Live

`.devspark.work/intake/future-<slug>-<concern-id>.md` is the default and is idempotent: an existing file is never overwritten. When work tracking is configured, a tracker item that **already exists** may be referenced in the concern's `future_work.captured_as`; no command creates one for this purpose.

A preserved deferral that was never materialized is a lifecycle failure: `scripts/judge.py lifecycle` reports it before a pull request is created.

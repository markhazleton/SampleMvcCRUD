# Evidence-Backed Intake Template

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Use this when starting `/devspark-specify` for non-trivial work. The goal is to provide grounded evidence up front so the spec maps facts instead of re-deriving them.

## Paste-Ready Format

```text
CONTEXT/TIMING
- Why now? What changed?
- Scope urgency (hotfix / planned / migration window)

VERIFIED FINDINGS
- Fact 1 with concrete reference (file path, log, metric, failing test)
- Fact 2 with concrete reference
- Fact 3 with concrete reference

GOAL
- Single sentence: what outcome must be true when done

DESIGN
- Proposed approach at a high level (WHAT/WHY, not implementation minutiae)
- Key constraints and tradeoffs

NON-GOALS
- Explicitly out of scope items

ACCEPTANCE CRITERIA
- Measurable outcomes that define done

ROLLOUT
- How this will be introduced safely (phased, flag, migration, fallback)
```

## Mapping Rules (used by /devspark-specify)

- `VERIFIED FINDINGS` are preserved verbatim in spec evidence/rationale sections.
- `GOAL` and `ACCEPTANCE CRITERIA` map directly into requirements and success criteria.
- `NON-GOALS` map directly to bounded scope.
- `ROLLOUT` informs planning and verification gates.
- Missing sections may be inferred, but only after preserving provided evidence unchanged.

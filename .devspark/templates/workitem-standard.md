# Work-Item Review Standard

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

**Calibrated against**: `tests/fixtures/workitem-review/calibration-corpus.json`, `corpus_version: 1`

This is the rubric `/devspark-workitem-review` applies to a work item. It is deliberately a separate
document from the command that uses it: the rubric is the thing being calibrated, and a calibration
run is only meaningful if the artifact under test can change independently of the prompt that loads
it.

## How to apply it

Assess the item against all five dimensions below and return a verdict for **every** dimension, even
after one fails. Reporting only the first failure hides the rest of the work a developer has to do,
so a review that stops early is wrong even when its one reported verdict is right.

The verdict vocabulary is exactly two values: `pass` and `fail`. There is no partial credit, no
score, and no third "needs discussion" value — a dimension either gives a reader what it owes them
or it does not, and a middle value in practice becomes a way to avoid saying `fail`.

An item passes review only when all five dimensions are `pass`. A single `fail` fails the item.

Judge only what the item says. Do not credit a dimension because the intent is obvious from the
title, because the team already knows the context, or because a linked item might cover it — an
absent statement is a `fail`, since the reader who needs it most is the one without that context.

## Dimensions

### `problem_statement`

**Passes when** the item states what goes wrong today, independently of any proposed fix. A reader
who disagrees with the solution must still be able to confirm the problem is real.

**Fails when** the item opens with a chosen solution and never describes the current defect, or
describes only the absence of the proposed feature ("we don't have X") rather than the harm that
absence causes.

### `user_value`

**Passes when** the item names who benefits and what they can do afterwards that they cannot do now.
The beneficiary is a specific role, system, or audience.

**Fails when** the benefit is asserted in the abstract — "improves usability", "increases
efficiency" — with no named beneficiary, leaving nobody able to judge whether the work is worth
scheduling against anything else.

### `acceptance_criteria`

**Passes when** the criteria are observable and would be graded identically by two independent
reviewers. Each criterion names a condition and the expected outcome.

**Fails when** a criterion depends on the reviewer's judgment — "works well", "is intuitive", "is
fast enough" — so that agreement on "done" is not reachable from the text.

### `scope`

**Passes when** the item's boundary is stated: what is included, and where it stops. An explicit
exclusion list is the strongest form but is not required if the inclusion is bounded on its own.

**Fails when** the item is open-ended, so that unrelated work can be absorbed into it and no state
of the world clearly ends it.

### `dependencies`

**Passes when** every blocking prerequisite is named, or the item states that there are none. An
explicit "no dependencies" is a pass; silence is not.

**Fails when** the text implies or mentions a prerequisite without identifying it — "once the new
service is available", "after the migration" — leaving the item unsequenceable.

## Calibration

This standard is graded against the seven cases in the calibration corpus. One case passes every
dimension; five fail exactly one named dimension each; one fails three at once. A grading run must
reproduce each case's full five-dimension verdict vector exactly.

Matching only the overall pass/fail verdict is explicitly not sufficient. Five of the seven cases
fail exactly one dimension, so an overall-verdict comparison would accept a reviewer that failed the
wrong dimension for the right item — precisely the error the per-dimension corpus exists to detect.

The corpus and this document are a matched pair. Changing either without the other invalidates the
calibration, which is why a drift guard in the test suite fails when one moves without the other.

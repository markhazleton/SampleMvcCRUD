---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Explain why temporary DevSpark tasks cannot be exported as durable issue-tracker records.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Deprecated Behavior

DevSpark treats `spec.md`, `plan.md`, and `tasks.md` as temporary implementation aids. Exporting
their task breakdown into an issue tracker would turn disposable planning detail into a second
durable history and conflict with the rule that Git is the sole implementation history.

Do not read a temporary task list, call an issue API, or create issues from planning identifiers.
Explain that this command no longer performs writes. For durable follow-up work discovered during
implementation or review, create a separately worded issue from current repository behavior and the
remaining user outcome, with no spec, requirement, task, or phase references.

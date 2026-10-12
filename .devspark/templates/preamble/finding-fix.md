# Preamble Module: Finding Fix Discipline

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Part of the Command Preamble Contract (`command-preamble-contract.md`). Section numbers are stable identities that other documents cite. Commands load this module when the contract's module table lists them as consumers.

## 9. Genuine Fix Discipline (anti-gaming)

When a task, gate finding, or automated check (lint rule, static-analysis metric, code-smell warning, test failure) names a specific violation, do not react to the bare metric alone — e.g. "function too complex (11 > 5)" or "bare except". A published pre-registered eval (prompt-vs-metric-eval, comparing bare-metric vs. behavioral-intent phrasing across two Claude models) measured genuine-fix rates as low as 5.6% when the model reacted only to a raw linter number, versus 83% when it was first asked to reason about the underlying intent — and the stronger model gamed the bare metric *more*, not less.

Before fixing, state in one sentence the behavioral intent the check is a proxy for (e.g. "this function should do one thing — what is it, in one sentence?"; "this handler should name and surface the specific error, not swallow it"), then implement a fix that satisfies that intent — not just the number. Before marking the finding resolved, confirm it isn't a gamed fix:

- **Complexity/length findings**: a genuine fix reduces actual branching or splits real responsibilities apart; splitting the same branches into helper functions purely to lower the tool's count is gamed.
- **Exception-handling findings**: a genuine fix names a specific exception type and does something with it (log, re-raise, handle); narrowing the `except` clause while still silently discarding the error is gamed.
- **Any finding**: if a human reviewer would say "this only satisfies the tool without changing behavior," it is gamed, not done.

Applies wherever `/devspark-implement` resolves an `analyze`/`critic` finding, `/devspark-quickfix` implements its root-cause fix, or any code-quality remediation task is performed.

### 9.1 Intent Cues for Common Structural Smells

Stating the behavioral intent from scratch each time invites inconsistent phrasing. For the smells a linter or static-analysis tool most commonly flags, use the intent cue below as the starting sentence, then apply the genuine-fix test in the row before marking the finding resolved:

| Smell (tool-agnostic)                          | Intent cue (say this first)                                                        | Genuine fix looks like                                                         | Gamed fix looks like                                                                  |
| ----------------------------------------------- | ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Oversized function / high complexity            | "This function should do one thing — what is it, in one sentence?"                  | Extracting a real sub-responsibility into a named, independently testable unit | Splitting branches into helpers with no real boundary, purely to lower the count       |
| Too many parameters                             | "These parameters travel together — is there a missing object/abstraction?"        | Introducing the missing type and passing it instead of the loose bag           | Wrapping the same list in an untyped `{ ...everything }` bag that renames the coupling |
| Deep nesting                                    | "What is the guard clause or early return that removes this level?"                | Inverting conditions / early-returning so the happy path is flat               | Wrapping the same nested logic in an extra function to hide it from the counter        |
| Duplicated code                                 | "What is the one behavior these copies implement, and where should it live once?"   | Extracting a shared implementation both call sites use                        | Renaming variables/reformatting one copy so a text-diff tool no longer matches it      |
| Swallowed / bare exception                      | "What should the caller learn when this fails, and what should happen next?"       | Naming the specific exception type and logging, re-raising, or handling it     | Narrowing the `except`/`catch` clause while still silently discarding the error        |
| Unused variable / import / dead code            | "Does this code path still get exercised — by whom?"                                | Removing it, or wiring it in if it was meant to be used                       | Prefixing with `_`/adding a suppression comment purely to silence the tool             |
| Oversized file / module                         | "What are the distinct responsibilities living in this file?"                      | Splitting along real seams (one responsibility per file)                       | Moving code to a second file with no coherent boundary just to shrink line counts      |

This table is a starting point, not a substitute for judgment — a smell not listed still requires stating intent before fixing, per the rule above.

**Cross-language note**: the intent cues and the genuine-fix test are language-agnostic. The same gamed patterns appear across ecosystems — an empty `catch (e) {}` / `catch {}` (JS/TS, C#, Java) or a `catch (Exception)` that swallows, and suppressions such as a `_`-prefixed name, `// eslint-disable-next-line`, `# type: ignore`, `#pragma warning disable`, or `[SuppressMessage]` added purely to silence the tool. Apply the same intent test regardless of language: would a reviewer say the behavior changed, or only the tool's verdict?

### 9.2 Citing a Constitution Principle

If the project constitution (`/.knowledge/governance/constitution.md`) declares a genuine-fix / intent-over-metrics principle, a gate command (`/devspark-critic`, `/devspark-analyze`, `/devspark-pr-review`, `/devspark-site-audit`) that flags a gamed fix SHOULD cite that principle ID in the finding, escalating it from process guidance to a constitution violation (which those gates already treat as their highest severity). If no such principle exists, §9 still stands on its own as a non-negotiable process rule — the citation is an escalation hook, not a precondition.

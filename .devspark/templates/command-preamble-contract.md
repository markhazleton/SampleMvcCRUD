# Command Preamble Contract

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

This contract centralizes shared command preamble behavior to reduce prompt bloat and improve consistency.

Commands that reference this contract MUST load and obey this core before step 1. It holds the rules that bind every command; rules that bind only some commands live in modules (see Modules and Consumers).

## 0. Plan Temporarily, Review the Delta

Planning is an implementation aid, not a permanent compliance record.

- Authoring routes write temporary specs, plans, tasks, checklists, and gates under
  `.devspark.work/`. Route-specific traceability stays inside that active bundle.
- Implementation updates behavior, affected tests, and the current `.knowledge/` nodes that
  describe that behavior in the same change. Durable code, tests, and knowledge MUST NOT contain
  spec, task, quickfix, feature, or requirement identifiers.
- Traceability is strictly one-directional: the ephemeral spec/plan/tasks/quickfix record links to
  the durable code, test, and knowledge files it produced via its linkage fields (`code_ref` may
  name both production and test files; `knowledge_ref` names current `.knowledge/` files). It is
  never the reverse — durable code, tests, and `.knowledge/` MUST NOT carry a comment, docstring,
  identifier, or link back to any spec, plan, task, quickfix, or feature document, live or archived.
- Every implementation task or checklist item that changes behavior MUST record all affected
  durable production-code, test, and current-knowledge paths in the temporary spec/plan/tasks or
  quickfix record before that item is marked complete. Use `n/a` only when a category was genuinely
  not changed, with a brief reason. This is required traceability in the temporary record, not
  permission for any durable file to reference the temporary record.
- A durable `.knowledge/` node's `source_of_truth` or evidence may cite only durable code or test
  files. It MUST NOT cite a spec, plan, task, gate, quickfix record, `.devspark.work/`, or
  `.archive/` path. Planning artifacts may cite that node, but the node must remain self-contained
  current truth after the planning bundle is deleted or archived.
- After all required verification passes, implementation populates `code_ref`/`knowledge_ref`
  linkage on every item in the planning bundle and leaves it live under `.devspark.work/` — it
  never archives the bundle itself. `/devspark-release` is the sole command that later confirms
  every item's linkage and moves the completed bundle to `.archive/{YYYY-MM-DD}/` (preserving its
  relative path minus the `.devspark.work/` prefix), which can happen across more than one release
  window. It is not released, mined later, or reconstructed by downstream commands from
  `.devspark.work/`.
- **No DevSpark command reads, lists, enumerates, or globs `.archive/`, under any circumstance.**
  It is a write-only destination from every command's perspective — commands move content into it
  and never look back. Purging `.archive/` is a human action only; no command deletes from it on
  any schedule.
- PR creation, review, release, audit, and maintenance evaluate the resulting delta: code,
  current `.knowledge/`, tests, verification evidence, Git history, and host metadata. Missing
  planning records under `.devspark.work/` are normal and MUST NOT be reported as a defect or
  recovered from there.
- `.knowledge/` contains current authoritative truth only. Update or delete obsolete content;
  never retain decision logs, rationale archives, supersession chains, lifecycle states, or
  historical copies there. Git is the sole history for `.knowledge/` — the `.archive/{YYYY-MM-DD}/`
  convention above applies only to retired `.devspark.work/` planning artifacts, never to `.knowledge/`.

### Four-Root Placement

| Root | Owner | Purpose |
|---|---|---|
| `.devspark/` | Framework | Installed stock prompts, scripts, templates, and schemas |
| `.devspark.work/` | Repository | Temporary planning plus operational work products and overrides |
| `.knowledge/` | Repository | Current authoritative architecture, governance, reference, patterns, and operations, plus published user-facing guidance under `.knowledge/guides/` |
| `.archive/` | Repository | Landing place for all old work products, grouped by release; write-only, sole writers are `/devspark-release`, `/devspark-constitution`, and `/devspark-evolve-constitution` |

Install and upgrade may refresh `.devspark/` only. Migration between major layouts is an explicit,
human-confirmed operation and is never performed by install or upgrade.

## 1. User Input Rule

If `$ARGUMENTS` (or equivalent user input) is non-empty, you MUST consider it before proceeding.

## 2. Multi-App Scope Resolution

If `.devspark.work/devspark.registry.json` exists with `mode: "multi-app"`:

- Check for `--app <id>` in user input.
- Resolve work products from `{app.path}/.devspark.work/`, current knowledge from
  `{app.path}/.knowledge/`, and published guidance from `{app.path}/.knowledge/guides/` when app
  context is provided.
- Print resolved scope (app name, doc root) at the start of output.

If no app context is provided, fall back to repository-root `.knowledge/guides/` as before, but print a visible deprecation warning: "multi-app repos should pass `--app <id>`; this will require confirmation in a future release." (Phase 1 of a two-phase rollout — the fallback becomes a hard STOP asking which app in a later, separately-dated release, never silently in a patch.)

## 3. Script Resolution (2-tier override)

Before running any stock script declared in command frontmatter:

- If `.devspark.work/scripts/powershell/<filename>` (PowerShell) or `.devspark.work/scripts/bash/<filename>` (Bash) exists, run that override instead.
- Preserve all original arguments.
- Team overrides in `.devspark.work/scripts/` always take priority over `.devspark/scripts/`.

## 4. Shell Quoting Reminder

For single quotes in arguments like `I'm Groot`, use `I'\''m Groot` or double quotes when possible.

## 5. Metrics Emission Contract (Disabled by Default)

Command metrics are temporarily disabled unless the developer explicitly sets
`DEVSPARK_METRICS_ENABLED=true`. When enabled, append one JSON line after command completion
(success or failure) to:

- `.devspark.work/metrics/devspark-metrics.jsonl`

Each line SHOULD include, when available:

- `ts_utc` (ISO timestamp)
- `command` (e.g., `specify`, `analyze`)
- `status` (`success` | `fail`)
- `duration_ms`
- `branch`
- `feature_dir`
- `classification`
- `route_intent`
- `risk_level`
- `required_gates`
- `findings` object (`showstopper`, `critical`, `high`, `medium`, `low` counts)
- `author` (if available)

Use best-effort writes; metrics logging failures must never block the primary workflow.

## 6. Next-Step Footer Contract

To reduce the cognitive load of remembering which command comes next, every command that references this contract SHOULD end its output with a consistent two-line footer:

```text
Where you are: <short state label>
Next: <the single recommended command, or "run /devspark-next">
```

Guidance:

- Keep it to those two lines — it is a signpost, not a summary.
- When the command cannot confidently name the next step, fall back to `Next: run /devspark-next` so the developer always has one safe move.
- The footer is advisory; it never replaces a command's own gate results or required confirmations.
- Omit the footer only for pure query/list commands where "next" is meaningless.

## 8. Branch Safety (non-negotiable)

DevSpark commands MUST NOT create, switch (`git checkout` / `git switch`), rename, `reset`, or otherwise move `HEAD` to a different branch without the developer's explicit, in-the-moment confirmation.

- Before any branch create or switch, show the **current branch** and the **proposed branch** and ask. Wait for an explicit yes before running the git command.
- This confirmation is **required even under `--auto`** and even when `auto_push` is set. Changing the working branch is a context change only a human may approve — it is never auto-bypassed.
- If the working tree has **uncommitted changes**, warn that switching would carry them onto the other branch, and let the developer commit, stash, or decide first.
- Never `git reset --hard`, force-delete a branch, or discard commits/branches without explicit confirmation.
- `policies.protect_main` ("branch first") does **not** authorize an unattended switch: offer to create the branch and wait for the yes.

## Modules and Consumers

The sections that live in modules keep their numbers wherever they are; other documents cite them as `§N`. The
core (this file) carries §0-§6, §8, §13 and §14, which bind every command. The remaining sections live
in modules under `preamble/` (installed: `/.devspark/templates/preamble/`; source:
`templates/preamble/`). A command loads this core, then only the modules the table names it as a
consumer of. A command that cites a section MUST load the module that owns it, and every consumer
listed here MUST load its module even without an explicit citation, because the obligation binds it
whether or not its prompt says so. `tests/test_preamble_modules_contract.py` enforces both rules.

| Module | Sections | Owns |
|---|---|---|
| `finding-fix` | 9, 9.1, 9.2 | Genuine Fix Discipline, intent cues, constitution citation |
| `convergence` | 10 | Finding classification and when a route is finished |
| `orchestration` | 11 | How many commands a developer must know |
| `provenance` | 12 | Artifact version provenance |
| `configuration` | 7 | Repository and user configuration, `auto_push` safety |

```yaml
preamble_modules:
  finding-fix: [address-pr-review, analyze, critic, implement, pr-review, quickfix, site-audit, verify]
  convergence: [address-pr-review, analyze, checklist, closeout, create-pr, critic, explain, implement, planning-review, pr-review, site-audit, tasks, verify]
  orchestration: [closeout, next, planning-review, verify]
  provenance: [analyze, clarify, closeout, critic, implement, plan, planning-review, pr-review, quickfix, site-audit, specify, tasks, verify]
  configuration: [address-pr-review, create-pr, next, pr-review, quickfix, site-audit, specify, update-pr, workitem-create, workitem-review]
```

## 13. Minimum Sufficient Change

> **Minimum Sufficient Change.** Build the smallest complete solution that satisfies the current objective, explicit requirements, invariants, and demonstrated operating constraints. Prefer simpler and more local solutions over speculative flexibility. Future usefulness alone does not justify present complexity.

- **MVP** — build the smallest valuable outcome. Specify names it; everything else is explicitly out of scope.
- **KISS** — prefer the simplest safe design that satisfies the current requirements, and the least powerful mechanism that satisfies them safely.
- **YAGNI** — do not add behavior, abstraction, state, configuration, infrastructure, or extensibility for hypothetical future needs. Every new mechanism (state, configuration, feature flag, persistence, cache, background process, endpoint, telemetry subsystem, extension point, generic abstraction) is justified by a current requirement, invariant, demonstrated constraint, or accepted operational need.

> **Complexity is a cost that requires justification.** Completeness means completely satisfying today's justified needs, not anticipating every future need.
>
> **Correctness, security, safety, data integrity, and explicit contracts are not optional simplifications.**

Least-powerful-mechanism order (guidance, not an algorithm): existing behavior already satisfies it; clarify the requirement or documentation; remove unnecessary behavior; simplify the existing design; reuse an existing capability; deterministic validation or a test; operational procedure or human review; accept a bounded limitation; local code; new runtime machinery last.

This is a rule of reasoning for every command, not a gate: Specify captures the MVP boundary, Plan captures operating context and the expected mechanism shape, Critic proposes the least-complex mitigation first, and Judge asks what can be removed. Nothing here fails a plan on a count alone.

Minimum Sufficient Change applies to governance as well as implementation. When an authorized
bounded correction crosses the original scope boundary, follow convergence §10: reconcile the
existing feature records and perform only the applicable review and verification steps. Human
authorization never waives a required gate or its actual result.

## 14. Reusing Already-Loaded Artifacts

When the current context already holds the **full, unchanged** content of an artifact this command needs (the constitution, `spec.md`, `plan.md`, `tasks.md`, a contract), use it from context instead of loading it again.

- Reuse only full content that nothing has changed since it was read, in this same context. Never reuse a summary, an excerpt, or a copy from another session or a cache; reload when in doubt.
- Reuse saves a read, not a responsibility. A command whose job is an independent check (constitution compliance, analysis, critique, verification) still evaluates the artifact itself and never adopts another command's conclusion about it.
- A command that edits an artifact reloads it before relying on it again.

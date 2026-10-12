# BSW.DevSpark Prompt Templates

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

This directory contains the **core deliverable** of BSW.DevSpark — prompt templates that give AI coding assistants structured commands for specification-driven development.

## Commands (`commands/`)

Each file in `commands/` is a slash-command prompt (e.g., `/devspark-specify`, `/devspark-plan`). The quickstart install (`quickstart/devspark_quickstart_*.md`) deploys stock prompts to `.devspark/defaults/commands/`. In 9.0 GitHub Copilot receives each command as a native Agent Skill (`.github/skills/devspark-<command>/SKILL.md`, generated into `consumer-skills/`), and other agents receive hyphenated command entry points. Each entry point resolves prompts via the 3-tier order: personal override, team override, then stock default.

Terminology used by these templates:

- **Prompt** files are BSW.DevSpark workflow command surfaces.
- **Agents** are AI runtime or client integrations that execute prompts.
- **Skills** are portable capability packages prompts may delegate to.
- **Participants** are human or AI-filled team members responsible for work,
  review, critique, approval, or decision capture.
- **Roles** are responsibility labels for participants, such as owner, planner,
  implementer, reviewer, critic, or scribe.

BSW.DevSpark ownership follows four roots:

- `.devspark/` is framework-managed stock content
- `.devspark.work/` contains temporary plans, operations, configuration, and overrides
- `.knowledge/` contains current authoritative truth plus published user-facing guidance (`.knowledge/guides/`)
- `.archive/` is the landing place for all old work products, grouped by release

The collection includes 38 active commands.

| File                       | Command                           | Purpose                                                                                          |
| -------------------------- | --------------------------------- | ------------------------------------------------------------------------------------------------ |
| `specify.md`               | `/devspark-specify`               | Define requirements and user stories                                                             |
| `plan.md`                  | `/devspark-plan`                  | Create technical implementation plan                                                             |
| `tasks.md`                 | `/devspark-tasks`                 | Generate actionable task list                                                                    |
| `implement.md`             | `/devspark-implement`             | Execute tasks to build the feature                                                               |
| `create-pr.md`             | `/devspark-create-pr`             | Draft or update a pull request with workflow context                                             |
| `update-pr.md`             | `/devspark-update-pr`             | Refresh an existing pull request description from the current branch delta                       |
| `constitution.md`          | `/devspark-constitution`          | Establish project principles                                                                     |
| `pr-review.md`             | `/devspark-pr-review`             | Review PRs against constitution                                                                  |
| `address-pr-review.md`     | `/devspark-address-pr-review`     | Address PR review findings with enforced commit isolation                                        |
| `site-audit.md`            | `/devspark-site-audit`            | Comprehensive codebase audit                                                                     |
| `explain.md`               | `/devspark-explain`               | Trace "how is X done?" against code and knowledge, verifying sync and fixing drift as it's found |
| `commit-audit.md`          | `/devspark-commit-audit`          | Analyze commit history for workflow, hygiene, and delivery signals                               |
| `quickfix.md`              | `/devspark-quickfix`              | Lightweight bug fix workflow                                                                     |
| `release.md`               | `/devspark-release`               | Verify the durable Git delta, sweep `.devspark.work/` retention, and archive completed work        |
| `evolve-constitution.md`   | `/devspark-evolve-constitution`   | Propose constitution amendments                                                                  |
| `repo-story.md`            | `/devspark-repo-story`            | Narrative from commit history                                                                    |
| `critic.md`                | `/devspark-critic`                | Adversarial risk analysis                                                                        |
| `planning-review.md`       | `/devspark-planning-review`       | Analyze, Critic when applicable, and Judge as one bounded planning decision                      |
| `verify.md`                | `/devspark-verify`                | Empirical proof gate for declared verification modes                                             |
| `merge-from-main.md`       | `/devspark-merge-from-main`       | Script-first merge of remote `main`; the agent only resolves conflicts                           |
| `clarify.md`               | `/devspark-clarify`               | Clarify underspecified areas                                                                     |
| `analyze.md`               | `/devspark-analyze`               | Cross-artifact consistency check                                                                 |
| `checklist.md`             | `/devspark-checklist`             | Quality validation checklists                                                                    |
| `personalize.md`           | `/devspark-personalize`           | Create per-user prompt overrides                                                                 |
| `upgrade.md`               | `/devspark-upgrade`               | Upgrade project to latest templates                                                              |
| `discover-constitution.md` | `/devspark-discover-constitution` | Reverse-engineer principles from code                                                            |
| `taskstoissues.md`         | `/devspark-taskstoissues`         | Deprecated task-export explanation; performs no issue writes                                     |
| `workitem-review.md`       | `/devspark-workitem-review`       | Grade a named Azure DevOps work item against the shared standard (opt-in)                        |
| `workitem-create.md`       | `/devspark-workitem-create`       | Draft and, after confirmation, create a new Azure DevOps work item (opt-in)                       |
| `add-application.md`       | `/devspark-add-application`       | Register a new application in the multi-app registry (optional)                                  |
| `list-applications.md`     | `/devspark-list-applications`     | Display all registered applications (optional)                                                   |
| `validate-registry.md`     | `/devspark-validate-registry`     | Validate registry schema, references, and consistency (optional)                                 |

> **Note**: The three multi-app commands (`add-application`, `list-applications`, `validate-registry`) are only needed for repositories with multiple applications. Single-app repositories can ignore them entirely.

## Helper Templates

| File                          | Purpose                                                            |
| ----------------------------- | ------------------------------------------------------------------ |
| `spec-template.md`            | Template structure for feature specifications                      |
| `quick-spec-template.md`      | Template structure for lightweight quick specifications            |
| `plan-template.md`            | Template structure for implementation plans                        |
| `tasks-template.md`           | Template structure for task breakdowns                             |
| `checklist-template.md`       | Template structure for quality checklists                          |
| `spec-validation-contract.md` | Shared validation contract for spec structure and required content |
| `agent-file-template.md`      | Template for agent configuration files                             |
| `vscode-settings.json`        | Recommended VS Code settings                                       |

The stock spec, quick-spec, plan, and tasks templates include optional
`participants` YAML frontmatter examples. This metadata is advisory
responsibility context only. It is not required for existing artifacts, does
not affect prompt or script resolution, and does not change command output.
Customization layers and precedence are unchanged.

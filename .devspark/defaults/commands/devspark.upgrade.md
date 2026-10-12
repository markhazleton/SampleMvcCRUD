---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Check the installed BSW.DevSpark version, identify stale framework files, and guide a safe upgrade to the latest release
---

<!-- markdownlint-disable MD040 -->

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty). Supported options:

| Option | Description |
|--------|-------------|
| `--dry-run` | Show what would change without modifying files |
| `--force` | Skip confirmations |

---

## Overview

This command checks whether the consumer project's installed BSW.DevSpark matches the
latest available version and guides you through a safe upgrade. It:

1. Reads `.devspark/BSW.DevSpark.version` to find the installed version (fallback: legacy `.documentation/DEVSPARK_VERSION`)
2. Detects the latest version from `CHANGELOG.md` or `pyproject.toml`
3. Classifies files across the four roots by ownership
4. Identifies stale or missing framework files
5. Runs the canonical quickstart guide's Update Mode to apply updates
6. Retires 8.x entry points (stateless, one confirmation, edited files quarantined)
7. Verifies the stamp file was updated after the upgrade

### Execution

Run the canonical BSW.DevSpark install/upgrade/migration guide directly from your AI agent —
the agent-specific guide under `quickstart/`. This guide is the manifest-driven source of
truth for install and upgrade. It refreshes `.devspark/`, preserves `.devspark.work/`,
`.knowledge/`, and `.archive/`, and explicitly migrates legacy layouts to the four-root
model when required. This is the only supported install/upgrade path.

---

## Outline

### 1. Read Installed Version

Check for `.devspark/BSW.DevSpark.version` first:

```text
.devspark/BSW.DevSpark.version
```

Expected format (preferred key-value):

```
version: <X.Y.Z>
installed: <YYYY-MM-DD>
method: <install-method>
migrated-from: <source>
```

Legacy fallback format (still supported):

```
<version>
installed: <YYYY-MM-DD>
agent: <agent-key>
```

**If the file is missing:**

- Check legacy `.documentation/DEVSPARK_VERSION`
- If both are missing, report: `VERSION stamp not found — version unknown`
- Proceed to Step 2 to determine what version is actually present

**If the file exists**, extract:

- `INSTALLED_VERSION` — e.g., `1.1.0` (must be semver `X.Y.Z`; otherwise treat as unknown)
- `INSTALL_DATE` — e.g., `2026-02-08`
- `INSTALL_METHOD` — e.g., `copilot-quickstart` or `source-dogfood`

### 2. Detect Latest Available Version

Read `CHANGELOG.md` at the repo root (or `.documentation/CHANGELOG.md`):

- Find the most recent `## [X.Y.Z]` heading
- That is `LATEST_VERSION`

Fallback: read `pyproject.toml` `version = "..."` if CHANGELOG is absent.

### 3. Compare Versions

| Condition | Status |
|-----------|--------|
| `INSTALLED_VERSION == LATEST_VERSION` | Up to date |
| `INSTALLED_VERSION < LATEST_VERSION` | Upgrade available |
| VERSION stamp absent or non-semver | Unknown — treat as upgrade needed |

Display the comparison result clearly:

```
Installed : 1.1.0  (2026-02-08, method: copilot-quickstart)
Latest    : 1.2.4
Status    : UPGRADE AVAILABLE
```

If up to date, skip to Step 6 (Verify Files).

### 4. Classify Files Across the Four Roots

Separate framework-owned files (overwritten on upgrade) from user-owned files (never
touched). Use this classification:

#### Command Resolution Order

BSW.DevSpark uses a **3-tier override system** for command prompts. When an AI agent
runs a `/devspark-*` command, it resolves the prompt in this order (first match wins):

```text
1. .devspark.work/{git-user}/commands/   ← Per-user overrides (via /devspark-personalize)
2. .devspark.work/commands/              ← Team customizations (yours to edit freely)
3. .devspark/defaults/commands/     ← Stock BSW.DevSpark prompts (upgrade overwrites ONLY this)
```

**Key principle: upgrades NEVER touch `.devspark.work/commands/`.** They only write
to `.devspark/defaults/commands/`. Your team's customizations in
`.devspark.work/commands/` always take priority and are never lost.

After an upgrade, the team can compare `defaults/commands/` vs `commands/` to see
what changed and selectively merge improvements they want.

#### Framework-owned (safe to overwrite on upgrade)

These are written to `.devspark/` and should match the latest version:

- `.devspark/defaults/commands/devspark.*.md` — stock prompt templates
- `.devspark/templates/` — stock helper templates
- `.devspark/templates/skills/` — portable Agent Skill packages (SKILL.md + scripts + references) consumed by commands such as `/devspark-specify`
- `.devspark/scripts/bash/*.sh` and `.devspark/scripts/powershell/*.ps1` — both sets always present
- `.devspark/BSW.DevSpark.version`
- Command entry points, one per stock command:
  - `.github/skills/devspark-<cmd>/SKILL.md` (GitHub Copilot: the manifest category `skill`)
  - `.claude/commands/devspark-*.md`
  - `.cursor/commands/devspark-*.md`
  - `.windsurf/workflows/devspark-*.md`
  - *(and equivalents for other supported agents)*

#### Repository-owned (NEVER overwritten by framework refresh)

These are written by the project team and must be preserved:

- `.devspark.work/commands/` — team-customized command prompts (the working copies)
- `.devspark.work/{git-user}/commands/` — per-user personalized prompts
- `.devspark.work/scripts/` — team-customized script overrides (see Script Resolution below)
- `.devspark.work/specs/` — active temporary specifications, plans, tasks, and gates
- `.knowledge/` — all current authoritative project knowledge, plus `.knowledge/guides/` for
  published user-facing documentation
- `.devspark.work/devspark.json` — platform configuration (github/azdo/gitlab)
- `.devspark.work/audit/` — current operational audit work products
- `.devspark.work/quickfixes/` — active temporary quickfix records
- `CHANGELOG.md` (repo root)
- Any file not listed in the framework-owned category

#### Script Resolution Order

Scripts use a **2-tier override system**. When a command runs a script, it resolves
the script in this order (first match wins):

```text
1. .devspark.work/scripts/{bash|powershell}/   ← Team overrides (yours to edit freely)
2. .devspark/scripts/{bash|powershell}/        ← Stock scripts (upgrade overwrites ONLY this)
```

**Key principle: upgrades NEVER touch `.devspark.work/scripts/`.** They only write
to `.devspark/scripts/`. Your team's script customizations in
`.devspark.work/scripts/` always take priority and are never lost.

Common reasons to override a script:

- Platform adaptation (Azure DevOps instead of GitHub)
- Custom CI/CD integration
- Organization-specific authentication or tooling

### 5. Identify Stale Files

Check for signs that the install needs updating:

| Check | Issue | Severity |
|-------|-------|----------|
| `.devspark/BSW.DevSpark.version` absent (and legacy stamp absent) | No version stamp | HIGH |
| VERSION stamp present but older than `LATEST_VERSION` | Out of date | MEDIUM |
| Old `devspark.*-old.md` command files in agent folder | Leftover duplicates | LOW |

Report findings before proceeding.

Also check for `.devspark.work/commands/` overrides that shadow commands with structural contract changes, especially `/devspark-specify`, `/devspark-plan`, `/devspark-tasks`, `/devspark-implement`, and `/devspark-create-pr`. Warn the user explicitly when an override may mask a stock routing, frontmatter, or lifecycle change and recommend a manual diff/merge.

#### Orphaned overrides (stale overrides of removed/renamed stock files)

After refreshing `.devspark/`, check every override for a stock default that no longer exists. An override is **orphaned** when the stock file it was meant to shadow was renamed or removed upstream — the override now shadows nothing (or a stale contract) and is a silent staleness trap.

Check both override surfaces, across **both** script types:

| Override | Stock default it must map to |
|----------|------------------------------|
| `.devspark.work/commands/devspark.<name>.md` | `.devspark/defaults/commands/devspark.<name>.md` |
| `.devspark.work/{git-user}/commands/devspark.<name>.md` | `.devspark/defaults/commands/devspark.<name>.md` |
| `.devspark.work/scripts/bash/<name>.sh` | `.devspark/scripts/bash/<name>.sh` |
| `.devspark.work/scripts/powershell/<name>.ps1` | `.devspark/scripts/powershell/<name>.ps1` |

Only `devspark.*.md` command override files mirror a stock command — a team's genuinely custom command is not orphaned. For scripts, list any override whose stock counterpart is absent so the team can confirm whether it is an intentional custom script or a stale fork.

Report each orphan and recommend one of: delete it if obsolete, or rename/re-point it to the current stock file. **Never auto-delete anything under `.knowledge/` or `.archive/`** — this is advisory only.

#### Stale-fork rot markers (overrides copied from a pre-DevSpark / legacy layout)

A raw line-diff cannot tell an intentionally customized override from one that was forked long ago and never rebased. Scan the **content** of every command and script override (both `.sh` and `.ps1`) for markers that only exist in old stock files:

| Marker found in an override | What it means |
|-----------------------------|---------------|
| `SPECIFY_FEATURE` | Pre-DevSpark Spec Kit env var; stock standardized on `DEVSPARK_FEATURE` |
| `.specify/` path reference | Legacy layout; stock uses the four-root model |
| `.documentation/defaults/commands` path | Pre-separation stock path; stock now uses `.devspark/defaults/commands/` |

Any hit means the override was copied from an old stock file. Recommend diffing it against the current `.devspark/` stock file and rebuilding the override on top of current stock. This is the class of rot that caused a stale `create-new-feature` override to crash in the field.

#### Legacy repo-root `build_knowledge_index.py` (framework-owned split-brain)

Before the `.devspark/` framework root existed, `build_knowledge_index.py` lived directly at the repository's own `scripts/build_knowledge_index.py`. That path is no longer part of any current install target — the manifest installs it only to `.devspark/scripts/build_knowledge_index.py`, which is the sole engine every command (`/devspark-explain`, `site-audit`, `release`, CI checks) resolves to. If both a repo-root and a `.devspark/scripts/` copy exist, they can drift apart silently — a real defect fixed in 7.5.1.

Check for `scripts/build_knowledge_index.py` at the repository root. If absent, nothing to do. If present, verify the two copies with
`scripts/knowledge-integrity.py --verify-sync scripts/build_knowledge_index.py .devspark/scripts/build_knowledge_index.py`
(installed-repo path: `.devspark/scripts/knowledge-integrity.py`) rather than eyeballing a diff — this is the same
hash-equality check `check_engine_divergence` uses internally, so the upgrade flow and the ongoing `site-audit --scope=knowledge`
check (`KNOW7`) agree on what "identical" means:

- **Identical**: it is a redundant, framework-owned leftover. Delete it and report the removal.
- **Different**: treat it as possibly user-customized — never delete it silently. Report the diff and ask the repository owner whether to retire it in favor of `.devspark/scripts/build_knowledge_index.py` or keep it deliberately as a project override.

Either way, confirm no CI workflow or project script in the repository still invokes the root copy — every caller must point at `.devspark/scripts/build_knowledge_index.py`. After any deletion or reconciliation in this step, **re-run `--verify-sync`** (or, if the root copy was deleted, confirm it is now absent) before reporting success — do not claim the split-brain is resolved on the strength of the edit alone.

#### New stock commands without an entry point

After refreshing `.devspark/`, cross-check every stock command in `.devspark/defaults/commands/devspark.*.md` against the agent's entry points (for Copilot `.github/skills/devspark-<cmd>/SKILL.md`; for other agents `devspark-<cmd>.<ext>` in the agent's commands directory). Report:

- **Missing entry points** — a stock command was added upstream but has no entry point yet, so it is not invokable. Re-run entry point generation (Step 7 of the quickstart) to create it.
- **Orphan entry points** — a generated entry point exists for a command that no longer ships in stock (removed/renamed). The retirement procedure below handles it.

#### Missing `devspark.user.json` gitignore exclusion

`.devspark.work/devspark.user.json` holds per-developer preferences and must never be committed. Check
the repository `.gitignore` for `/.devspark.work/devspark.user.json`.

- **If missing**: report it and print the exact line to add:

  ```gitignore
  # Per-developer DevSpark preferences (assistant name, shell, auto_push) - never commit
  /.devspark.work/devspark.user.json
  ```

  Do not write this automatically. Surface it for the repository owner to add.
- **If present**: no action, no report.

### 6. Verify Framework Files (even if up to date)

Even when the version matches, check that all expected framework files are present.
List any that are **missing** from the expected locations.

Missing framework files should be reported as:

```
MISSING: .devspark/scripts/powershell/setup-plan.ps1
MISSING: .github/skills/devspark-specify/SKILL.md
MISSING: .devspark/templates/skills/write-spec/SKILL.md
```

For every command prompt that delegates to a skill (currently `/devspark-specify` → `write-spec`), verify the skill's `SKILL.md`, `scripts/`, and `references/` are present under `.devspark/templates/skills/`. A missing skill must be reported at the same severity as a missing script — commands that delegate to a missing skill silently degrade to fallback behaviour.

### 7. Perform the Upgrade

**If `--dry-run`**: Display the full plan and stop. Do not modify files.

**Otherwise:**

#### 7a. Update stock defaults

Write the latest BSW.DevSpark prompt templates to `.devspark/defaults/commands/`
and **both** script sets to `.devspark/scripts/bash/` and `.devspark/scripts/powershell/`.
Always sync both sets regardless of the current OS — a repo is shared across macOS,
Linux, and Windows, so both sets must be present at all times.

Also write the latest **Agent Skill packages** from upstream `templates/skills/` to
`.devspark/templates/skills/`. This directory is framework-owned and safe to overwrite
completely. At minimum the following must land:

- `.devspark/templates/skills/README.md`
- `.devspark/templates/skills/ADAPTER-contract.md`
- `.devspark/templates/skills/SKILL-validation-contract.md`
- `.devspark/templates/skills/references/devspark-skills-guide.md`
- `.devspark/templates/skills/write-spec/SKILL.md`
- `.devspark/templates/skills/write-spec/references/spec-template.md`
- `.devspark/templates/skills/write-spec/scripts/gather-context.ps1`
- `.devspark/templates/skills/write-spec/scripts/gather-context.sh`

These directories are framework-owned and safe to overwrite completely.

Replay the remaining manifest entries from the quickstart's Step 4 as well. Entries of category `skill` (the command skills, destined for `.github/skills/devspark-<cmd>/SKILL.md`) are installed only for GitHub Copilot; every other agent skips them and receives `devspark-<cmd>.<ext>` entry points instead.

**Important**: Do NOT write to `.devspark.work/commands/` or `.devspark.work/scripts/`.
Those directories belong to the team. Only `.devspark/defaults/commands/` and
`.devspark/scripts/` are updated.

#### 7b. Show what changed

After updating `defaults/commands/` and `.devspark/scripts/`, compare against the
team's working copies:

```
Changed prompts (defaults/ vs commands/):
  devspark.specify.md   — 12 lines differ
  devspark.release.md   — new prompt (not in commands/ yet)
  devspark.critic.md    — identical (no action needed)

Changed scripts (.devspark/scripts/ vs .devspark.work/scripts/):
  get-pr-context.ps1    — 8 lines differ (team has custom override)
  platform.ps1          — new script (not in team overrides)
  common.ps1            — no team override (stock version used)
```

Offer to show diffs for any changed files so the team can decide what to merge.

**Script merge guidance:**

- If the team has overridden a script that changed upstream, show the diff
  and let the team decide whether to merge the upstream improvements
- If a new stock script was added, inform the team — no action needed unless
  they want to customize it
- Never silently overwrite `.devspark.work/scripts/`

#### 7c. Retire 8.x entry points

Runs after 7a installs the 9.0 entry points, and never blocks that install: a retirement problem is reported and the upgrade continues. The 8.x legacy classes below apply only when the installed major version read in Step 1 is below 9; the orphan class applies on every upgrade. The procedure is stateless: there is no marker and no flag, so the candidate rule alone decides. A second and third run list nothing and change nothing, and an interrupted run recomputes the list from what remains. The agent's quickstart holds the exact recognition rule; this section is its summary.

DevSpark 9.0 exposes every command as `/devspark-<cmd>`. The 8.x dotted entry points would list each command twice, so they are retired.

##### Candidates

For each stock command `<cmd>`, examine only the agent's 8.x file names:

- GitHub Copilot: `.github/agents/devspark.<cmd>.agent.md` and its companion `.github/prompts/devspark.<cmd>.prompt.md`
- Other agents: `devspark.<cmd>.<ext>` in the agent's commands directory (for example `.claude/commands/devspark.<cmd>.md`)

| Class | Test | Action |
|---|---|---|
| Unmodified managed entry point | Frontmatter absent or only `description`, `handoffs`, `agent` and a `name` equal to `devspark.<cmd>`; body is exactly one of the generated package or quickstart forms (placeholder `{{input}}`, `$ARGUMENTS` or `{{args}}`), or the Copilot prompt companion pointer (frontmatter only `agent: devspark.<cmd>`, empty body) | Delete |
| Edited managed-name file | Anything else | Move to the quarantine; never delete |
| Foreign file | No stock command name | Left untouched |

Also report:

- **Conflicts** — a path the 9.0 install needs (such as `.github/skills/devspark-<cmd>/SKILL.md`) that holds foreign content: an existing foreign skill folder or entry point file is never overwritten; report it for a human to rename or remove.
- **Orphans** — a generated 9.0 entry point (a `devspark-<name>` skill folder marked as generated, or a `devspark-<name>.<ext>` file) whose `<name>` has no stock command; list it for deletion.

For GitHub Copilot, also prune `chat.promptFilesRecommendations` in `.vscode/settings.json`: remove only the managed entries (the keys `devspark.<cmd>` for stock commands) and leave user-added entries, other settings and comments as they are.

##### Confirm once, then act

Show the lists and ask for **one confirmation** covering every deletion, quarantine move, orphan removal and settings cleanup. If every list is empty, report "No 8.x entry points to retire." Without a yes, change nothing.

- Delete unmodified managed entry points and orphans.
- Quarantine edited files byte-identical to `.devspark.work/migration/8.x-entry-points/<file name>`; add a numeric suffix on a name collision and never overwrite an existing file. Report this remedy: recreate the edit as a customization in `.devspark.work/commands/devspark.<cmd>.md` (team) or `.devspark.work/{git-user}/commands/devspark.<cmd>.md` (developer).
- Report conflicts.

The quarantine only adds new files. This procedure never touches, modifies, deletes or overwrites existing files in `.devspark.work/`, `.knowledge/` or `.archive/`, any customization, or any lifecycle artifact. Re-list afterwards: only the reported items may remain.

### 8. Post-Upgrade Verification

After the upgrade completes:

1. **Read `.devspark/BSW.DevSpark.version` again** — confirm version updated
2. **Verify `.devspark/defaults/commands/` has latest prompts**
3. **Confirm `.devspark.work/commands/` is untouched** — team customizations preserved
4. **Confirm `constitution.md` is intact** (never touched by upgrades)

Report a post-upgrade summary:

```
Post-Upgrade Verification
  VERSION stamp      : 1.2.4  (was 1.1.0)
  defaults/commands/ : updated (27 prompts)
  commands/          : unchanged (team customizations preserved)
  stock scripts/bash : updated (15 scripts)
  stock scripts/ps   : updated (16 scripts)
  stock skills/      : updated (write-spec + contracts)
  team scripts/      : unchanged (overrides preserved)
  constitution.md    : untouched (never modified by upgrades)
```

### 9. Output Final Summary

#### Upgrade performed

```
BSW.DevSpark Upgrade Summary
  Previous Version : <INSTALLED_VERSION>
  New Version      : <LATEST_VERSION>
  Install Method   : <INSTALL_METHOD>
  Date             : <TODAY>

Stock prompts updated in .devspark/defaults/commands/.
Stock scripts updated in .devspark/scripts/bash/ and .devspark/scripts/powershell/ (both sets).
Team customizations in .devspark.work/commands/ and .devspark.work/scripts/ are untouched.

To merge specific improvements into your team prompts:
  Compare .devspark/defaults/commands/ vs .devspark.work/commands/
To merge script improvements:
  Compare .devspark/scripts/ vs .devspark.work/scripts/

Next steps:
  1. Review changes: git diff
  2. Test: run a slash command in your AI assistant (e.g., /devspark-specify)
  3. Commit: git add -A && git commit -m "chore: upgrade devspark to vX.Y.Z"
```

#### Already up to date

```
BSW.DevSpark is up to date.
  Version : <INSTALLED_VERSION>
  Method  : <INSTALL_METHOD>
  Date    : <INSTALL_DATE>
```

#### Dry run

```
Dry Run — No changes made.

Would upgrade: <INSTALLED_VERSION> -> <LATEST_VERSION>
Framework files to update: <N>
Repository content preserved: .devspark.work/, .knowledge/, .archive/

To apply:
  Re-run this command without --dry-run
```

---

## Guidelines

### User Data is Sacred

Never modify or delete:

- `.devspark.work/commands/` — team-customized prompts
- `.devspark.work/{git-user}/commands/` — per-user personalized prompts
- `.devspark.work/scripts/` — team-customized script overrides
- `.devspark.work/specs/` and all contents
- `.devspark.work/devspark.json` — platform configuration
- `.knowledge/` — all current authoritative knowledge, including the constitution, plus
  `.knowledge/guides/` for published documentation
- `.devspark.work/audit/`
- Any file the user created that is not in `.devspark/`

### Non-Destructive by Default

Always check before replacing framework files. If `--dry-run` is specified,
produce only the plan — never modify files.

### Version Stamp is Authoritative

`.devspark/BSW.DevSpark.version` is the primary source of truth for the installed version in a
consumer project. Legacy `.documentation/DEVSPARK_VERSION` may appear in older
installs and should be read only as a fallback. After any successful upgrade,
verify `.devspark/BSW.DevSpark.version` was updated. If the stamp is absent after an upgrade,
warn the user and suggest re-running this command.

### Constitution is Never Touched

The constitution (`constitution.md`) is user-owned and NEVER modified by upgrades.
No backup is needed because the upgrade process does not touch it.

## Context

$ARGUMENTS

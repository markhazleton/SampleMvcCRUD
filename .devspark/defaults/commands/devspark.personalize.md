---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Create a personalized copy of any BSW.DevSpark command prompt for the current git user.
scripts:
  sh: .devspark/scripts/bash/check-prerequisites.sh --json
  ps: .devspark/scripts/powershell/check-prerequisites.ps1 -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Command Resolution Order

BSW.DevSpark uses a **3-tier override system**. When a `/devspark-*` skill runs,
the prompt is resolved in this order (first match wins):

```text
1. .devspark.work/{git-user}/commands/   ← Per-user overrides (this command creates these)
2. .devspark.work/commands/              ← Team customizations (shared, editable)
3. .devspark/defaults/commands/     ← Stock BSW.DevSpark prompts (read-only, upgrade-safe)
```

Upgrades only write to `defaults/commands/`. Team and user customizations are never touched.

This command only personalizes repository-owned overrides under `.devspark.work/`. It never edits stock prompts under `.devspark/defaults/commands/`.

## Outline

This command creates a per-user personalized copy of a BSW.DevSpark command prompt.
Personalized prompts live in `.devspark.work/{git-user}/commands/` and take priority
over both team customizations and stock defaults.

### Steps

1. **Determine the git user identity**:

   ```bash
   git config user.name
   ```

   Normalize to a folder-safe slug:
   - Lowercase, replace spaces with hyphens, strip non-alphanumeric/hyphen characters
   - Example: `"Jordan Lee"` → `jordan-lee`

   Fallback: `git config user.email` (local part before `@`).

2. **Parse the command name from user input** (`$ARGUMENTS`):

   The argument should be a command name, with or without the `devspark.` prefix.
   Examples: `constitution`, `devspark-plan`, `implement`

   If no argument is given, list all available commands from `.devspark.work/commands/`
   (or `.devspark/defaults/commands/` if `commands/` is empty) and ask the user
   which one to personalize.

3. **Resolve the source prompt** (follow the 3-tier order):

   Look for the prompt in this order:
   1. `.devspark.work/commands/devspark.{command}.md` (team version)
   2. `.devspark/defaults/commands/devspark.{command}.md` (stock version)

   Use the first one found as the base for the personalized copy.
   If neither exists, show available commands and ask the user to pick one.

4. **Create the user directory**:

   ```text
   .devspark.work/{git-user}/commands/
   ```

5. **Check if a personalized version already exists**:

   If `.devspark.work/{git-user}/commands/devspark.{command}.md` already exists:
   - Show the user and ask if they want to overwrite or edit the existing one
   - Default: open the existing file for review

6. **Copy and annotate the prompt**:

   Copy the resolved source to `.devspark.work/{git-user}/commands/devspark.{command}.md`.

   Add a header comment block at the top:

   ```markdown
   <!-- 
     Personalized prompt for: {git-user}
     Based on: {source-path}
     Created: {date}
     
     This file takes priority over team and stock defaults when you run /devspark-{command}.
     Edit freely. To revert, delete this file.
   -->
   ```

7. **Inform the user**:

   ```text
   Created personalized prompt:
     .devspark.work/{git-user}/commands/devspark.{command}.md
   
   Resolution order for /devspark-{command}:
     1. ✅ .devspark.work/{git-user}/commands/  (this file — ACTIVE)
     2.    .devspark.work/commands/              (team default)
     3.    .devspark/defaults/commands/     (stock BSW.DevSpark)
   
   Edit it to customize the behavior for your workflow.
   To revert to the team/stock default, simply delete this file.
   ```

8. **Open the file** for editing so the user can customize it immediately.

---
source: "BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com"
description: Update an existing pull request description with current branch delta, preserving linked work items and metadata
handoffs:
  - label: Review Updated PR
    agent: devspark-pr-review
    prompt: Run a re-review of the updated pull request
  - label: Create New PR
    agent: devspark-create-pr
    prompt: Create a new pull request for this branch
scripts:
  sh: .devspark/scripts/bash/get-pr-context.sh $ARGUMENTS --json
  ps: .devspark/scripts/powershell/get-pr-context.ps1 $ARGUMENTS -Json
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Overview

This command updates an **existing** pull request description based on the current state of the branch. Unlike `/devspark-create-pr`, which is designed for initial PR creation and prompts for full context, `update-pr` focuses on the branch delta since the last review — rebuilding the PR body without re-prompting for work items or context already captured in the existing PR.

**IMPORTANT**: This command **only updates the PR description** — it does not merge, approve, or modify code.

**Use this command when**:

- You have addressed review feedback and want to update the PR description to reflect what changed
- You want to refresh the PR body after a rebase or additional commits
- You want to regenerate the PR description without losing linked issues or work items

**Use `/devspark-create-pr` instead when**:

- No PR exists yet for the branch
- You want to start fresh with a new PR description

## Prerequisites

- Project constitution at `/.knowledge/governance/constitution.md` (REQUIRED)
- Platform CLI installed and authenticated — `gh` for GitHub, `az` (with the `azure-devops` extension) for Azure DevOps (required)
- An existing open PR for the current branch
- **HARD RULE — Branch Sync**: The source (head) branch **MUST** be fully in sync with the target (base) branch. Do **NOT** proceed if the source branch is behind the target. Instruct the user to rebase or merge first.

## Platform note (GitHub vs Azure DevOps)

The host is resolved automatically (`.devspark.work/devspark.json` → `platform`, else the `origin` remote URL). The `{SCRIPT}` (get-pr-context) already handles platform-aware PR detection. Wherever this command shows a `gh` command, use the equivalent for the active platform:

| Action | GitHub | Azure DevOps |
| ------ | ------ | ------------ |
| Read PR | `gh pr view {PR} --json ...` | `az repos pr show --id {PR} --output json` |
| Update body | `gh pr edit {PR} --body "..."` | `az repos pr update --id {PR} --description "..."` |
| Auth | `gh auth login` | `az login` |

For Azure DevOps, resolve org/project/repo from `.devspark.work/devspark.json` (or the `dev.azure.com/{org}/{project}/_git/{repo}` remote) and pass `--organization https://dev.azure.com/{org}` when required.

## Outline

### 1. Initialize PR Context

Load and obey the shared preamble contract core at `/.devspark/templates/command-preamble-contract.md` (installed repos) or `templates/command-preamble-contract.md` (source repos) before step 1 — it covers the 2-tier override check for scripts. Preamble modules: `configuration`.

Run `{SCRIPT}` and parse its JSON output for:

- `PR_CONTEXT`: Current PR metadata (number, title, branches, commit SHA, files, diff)
- `CONSTITUTION_PATH`: Path to constitution file
- PR body (existing description)
- Linked issues / work items referenced in the existing PR body

**PR Number Detection**:
Detect the PR number in this order:

1. User explicitly provides PR number in arguments: `#123` or `123`
2. GitHub environment variables: `GITHUB_PR_NUMBER`, `PR_NUMBER`
3. Current branch PR detection via `gh pr view`
4. If unable to detect, error with clear instructions

**Error Handling**:
If the script fails:

- **PR not found**: Suggest `/devspark-create-pr` to create a new PR
- **Branch out of sync** (`is_behind_target: true`): **STOP immediately.**
  > "BLOCKED: The source branch `{source_branch}` is behind target branch `{target_branch}`. Sync it first before updating this PR."
  > Suggested fix: `gh pr update-branch {PR_NUMBER}` or `git fetch origin && git rebase origin/{target_branch}`
- **Constitution missing**: Guide user to run `/devspark-constitution`
- **GitHub CLI not installed**: Provide installation instructions

### 2. Load Existing PR Metadata

Read the current PR body using `gh pr view {PR_NUMBER} --json body,title,labels,milestone,assignees`:

1. **Extract linked work items**: Identify all issue/ticket references (e.g., `Closes #45`, `Fixes #102`, `AB#1234`, `Resolves #67`) — these **must be preserved** in the updated PR body verbatim
2. **Extract existing title**: Use unless user provides a new title in arguments
3. **Note existing labels, milestone, and assignees** — these are not changed by this command
4. **Identify existing PR template sections** used in the current body

#### Work-Item Linkage (optional, non-mutating)

Run this check only when `.devspark/scripts/bash/workitem.sh --operation check --json` (or the
PowerShell twin) reports `work_tracking_configured: true`. When it reports `false`, skip this
subsection entirely and say nothing about work items.

- Pass the references extracted above to the `verify-link` operation. **This operation only reads.**
  It reports whether each reference resolves and whether it sits inside the configured area path. It
  never writes to the work item, never posts a comment, and never changes a state — rebuilding a PR
  description must not mutate the board as a side effect.
- **Reproduce every reference verbatim** in the rebuilt body, including any that failed to resolve.
  Rewriting or dropping a reference silently breaks the link the developer relied on.
- Surface a warning for any reference that does not resolve, or that resolves outside the configured
  area path. Both are advisory — report them and continue.

**Posting back to the work item is a separate decision.** It is **off unless the repository opts in**,
and even then it is confirmed on its own — never folded into the confirmation for updating the PR.
Show the exact comment text before posting, and treat a refusal as final for this run. A standing
autonomy instruction does not supply this confirmation, because the effect lands outside the
repository where nobody reviewing the diff would see it.

### 3. Compute Branch Delta

Determine what changed since the last commit SHA recorded in the PR review file (if one exists at `/.devspark.work/pr-review/pr-{PR_NUMBER}.md`) or since PR creation.

**Finding the baseline commit SHA** (in order of preference):

1. Parse the `**Reviewed Commit**:` metadata line in the PR review file — use as the baseline if present
2. If the review file exists but contains no commit SHA (e.g., malformed or manually edited), fall back to the PR's first commit via `gh pr view {PR_NUMBER} --json commits --jq '.commits[0].oid'`
3. If no review file exists, use the PR creation commit (the base of the branch)

```bash
gh pr view {PR_NUMBER} --json commits --jq '.commits[-1].oid'
```

Using the delta:

- List new commits added since the last review or PR creation
- Summarize new files changed, lines added/removed
- Identify the intent of changes (from commit messages and diff)
- Note verified code, test, knowledge, and documentation changes reflected in the new commits
- For `.knowledge/` nodes touched by the new commits, note whether their evidence is test-verified
  (`verified_by: execution`) or code-only, and flag a code-only entry missing `fallback_reason` as a
  reviewer note (mirrors `/devspark-create-pr`'s Knowledge Evidence bullet)

### 4. Load Constitution for PR Template Context

Read `/.knowledge/governance/constitution.md` to identify:

- Required PR checklist items from constitution principles
- Mandatory sections for the project's PR template
- Any PR-specific governance requirements

### 5. Detect PR Template

Check for an existing PR template in order:

1. `.github/pull_request_template.md`
2. `.github/PULL_REQUEST_TEMPLATE.md`
3. `.github/PULL_REQUEST_TEMPLATE/*.md` (select the most relevant)
4. If none found, use the default BSW.DevSpark PR structure

### 6. Rebuild PR Body

Construct the updated PR body by:

1. **Preserve all linked work items** from step 2 exactly as found (do not rephrase or remove)
2. **Update the summary section** to reflect the current branch state and highlight new changes since last review
3. **Update the "Changes in this PR" section** with the branch delta from step 3
4. **Regenerate the checklist** from the constitution and PR template — pre-check items that are clearly satisfied by the current diff
5. **Update validation status** from executable evidence and the current durable delta; do not read or
  reference temporary planning artifacts
6. **Add a "Changes since last review" subsection** if a review file exists, listing what was addressed

**Do not re-prompt the user** for any information already captured in the existing PR. If critical information is missing (e.g., the PR has no description at all), add a single focused question.

### 7. Update the PR

Apply the updated body using the active platform's CLI:

```bash
# GitHub
gh pr edit {PR_NUMBER} --body "{UPDATED_BODY}"
# Azure DevOps
az repos pr update --id {PR_NUMBER} --description "{UPDATED_BODY}" --organization https://dev.azure.com/{ORG}
```

Do not change the title, labels, milestone, or assignees unless the user explicitly requested changes in their input arguments.

### 8. Output Summary to User

Display a concise summary:

```text
✅ PR #{NUMBER} Updated!

📄 PR: {PR_TITLE}
🔗 URL: {PR_URL}
🔍 Branch delta: {N} new commits since last review
📋 Linked work items preserved: {LIST}

What changed in this update:
- [Summary of new changes incorporated]

To re-review after this update:
/devspark-pr-review #{NUMBER} re-review
```

## Guidelines

### Preservation First

The primary rule of `update-pr` is **preserve existing metadata**. Linked issues, milestones, labels, and assignees must not be altered. When in doubt, keep the existing value.

### Delta-Focused Summary

The PR body update should clearly communicate **what changed since the last version** in addition to the full current state. A reader who saw the previous version should immediately understand what is new.

### No Re-Prompting

This command is designed for iterative, low-friction use. If the existing PR already has a
description, do not ask the user to re-describe the change. Ask only for information that cannot be
derived from the durable delta or validation evidence. Missing planning artifacts are normal.

### Constitution Compliance

The PR checklist must reflect current constitution principles. Pre-check items when the diff clearly satisfies them. Leave items unchecked when the diff does not address them — do not remove checklist items even if they seem inapplicable.

## Context

$ARGUMENTS

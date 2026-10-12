## Prompt Resolution

Determine the current git user by running `git config user.checklist`.
Normalize to a folder-safe slug: lowercase, replace spaces with hyphens, strip non-alphanumeric/hyphen chars.

Read and execute the instructions from the **first file that exists**:
1. `.devspark.work/{git-user}/commands/devspark.checklist.md` (personalized override)
2. `.devspark.work/commands/devspark.checklist.md` (team customization)
3. `.devspark/defaults/commands/devspark.checklist.md` (stock default)

## User Input

$ARGUMENTS

Pass the user input above to the resolved prompt.

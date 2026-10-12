## Prompt Resolution

Determine the current git user by running `git config user.explain`.
Normalize to a folder-safe slug: lowercase, replace spaces with hyphens, strip non-alphanumeric/hyphen chars.

Read and execute the instructions from the **first file that exists**:
1. `.devspark.work/{git-user}/commands/devspark.explain.md` (personalized override)
2. `.devspark.work/commands/devspark.explain.md` (team customization)
3. `.devspark/defaults/commands/devspark.explain.md` (stock default)

## User Input

$ARGUMENTS

Pass the user input above to the resolved prompt.

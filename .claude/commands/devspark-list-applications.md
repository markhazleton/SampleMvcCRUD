## Prompt Resolution

Determine the current git user by running `git config user.list-applications`.
Normalize to a folder-safe slug: lowercase, replace spaces with hyphens, strip non-alphanumeric/hyphen chars.

Read and execute the instructions from the **first file that exists**:
1. `.devspark.work/{git-user}/commands/devspark.list-applications.md` (personalized override)
2. `.devspark.work/commands/devspark.list-applications.md` (team customization)
3. `.devspark/defaults/commands/devspark.list-applications.md` (stock default)

## User Input

$ARGUMENTS

Pass the user input above to the resolved prompt.

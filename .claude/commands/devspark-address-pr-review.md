## Prompt Resolution

Determine the current git user by running `git config user.address-pr-review`.
Normalize to a folder-safe slug: lowercase, replace spaces with hyphens, strip non-alphanumeric/hyphen chars.

Read and execute the instructions from the **first file that exists**:
1. `.devspark.work/{git-user}/commands/devspark.address-pr-review.md` (personalized override)
2. `.devspark.work/commands/devspark.address-pr-review.md` (team customization)
3. `.devspark/defaults/commands/devspark.address-pr-review.md` (stock default)

## User Input

$ARGUMENTS

Pass the user input above to the resolved prompt.

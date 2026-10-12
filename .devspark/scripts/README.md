<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

# BSW.DevSpark Context Scripts

Helper scripts invoked by the slash-command prompts in `templates/commands/`.
Installed copies live at `.devspark/scripts/` in consumer repositories; team
overrides in `.devspark.work/scripts/` take precedence at run time (2-tier
script resolution).

## Layout and Parity

- `bash/` — POSIX Bash implementations
- `powershell/` — PowerShell 7+ implementations

Constitution §VI (Platform Parity) requires every `bash/<name>.sh` to have a
matching `powershell/<name>.ps1` with equivalent behavior, and vice versa.
`tests/test_script_parity_contract.py` enforces this.

### Bash version constraint

`bash/*.sh` must stay compatible with **bash 3.2** — the version Apple ships
as `/bin/bash` on every Mac and has not upgraded since 2007 (GPLv3 licensing).
Since every script uses `#!/usr/bin/env bash`, it resolves to whichever
`bash` is first on `PATH`; on a stock Mac (no Homebrew `bash` prepended to
`PATH`) that is the real 3.2 binary. Avoid bash 4+-only constructs such as
`mapfile`/`readarray`, `${var^^}`/`${var,,}` case expansion, `local -n`
namerefs, and associative arrays (`declare -A`). When testing locally on a
Mac, run scripts with `/bin/bash` explicitly (not just `bash`) to catch
regressions even if Homebrew bash is installed and earlier on `PATH`.

## Unified Branch Creation

`new-branch.(sh|ps1)` is the **single** entry point that issues
`git checkout -b` for any DevSpark route (spec, quick-spec, or quickfix). It:

- derives the next global index live (never stores a counter) by scanning
  branches, `.devspark.work/specs/*`, and `.devspark.work/quickfixes/*`
  (both legacy `QF-YYYY-NNN` and unified `NNN-fix-*` records);
- composes `NNN-<type>-<slug>` branch names (`type` ∈ `spec`, `quick`, `fix`);
- validates the composed name with `git check-ref-format --branch` before
  ever creating it;
- enforces the Branch Safety confirmation gate (fails closed — non-zero
  exit — in a non-interactive context without `--yes`/`-Yes`);
- scaffolds the type-appropriate artifact and prints JSON-only to stdout
  (diagnostics go to stderr).

`create-new-feature.(sh|ps1)` (used by `/devspark-specify`) and the
branch-creation step inside `quickfix-context.(sh|ps1)` (used by
`/devspark-quickfix`) are thin shims that delegate to `new-branch` with
`--type spec|quick` / `--type fix` respectively, while preserving their own
existing JSON contracts (`BRANCH_NAME`, `SPEC_FILE`/`ARTIFACT_PATH`,
`FEATURE_NUM`/`NUMBER`, `HAS_GIT`) for backward compatibility. Legacy
`NNN-<slug>` and `QF-YYYY-NNN` branches/records remain fully supported and
continue to be counted by the allocator — only new branches adopt the
`NNN-<type>-<slug>` syntax.

## Standalone Tools

Most scripts are invoked by a command prompt's `scripts:` frontmatter. One is
standalone:

- `generate-atomic-shims.(sh|ps1)` — maintainer tool that regenerates the
  source-repository Agent Skills `.github/skills/devspark-*/SKILL.md` from
  `templates/commands/*.md` (and, for the five source-only commands, the stop
  bodies in `eng/skills/source-stop/`), plus the published consumer skills
  `templates/consumer-skills/devspark-*/SKILL.md`. Run with `--check` / `-Check` as the
  CI drift gate; it also reports orphaned `devspark-*` skill folders, which it
  never deletes.

Release packaging tooling is maintainer-only and lives in `eng/release/`, not
here — nothing under `eng/` ships to consumer repositories.

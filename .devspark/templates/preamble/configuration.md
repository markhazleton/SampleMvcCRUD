# Preamble Module: Repository Configuration

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Part of the Command Preamble Contract (`command-preamble-contract.md`). Section numbers are stable identities that other documents cite. Commands load this module when the contract's module table lists them as consumers.

## 7. Repository Configuration ("set once" facts)

Configuration is split so shared repo facts and per-developer preferences never mix:

- **`.devspark.work/devspark.json`** (committed) holds **repo facts + team policy**: `platform`, `organization`, `project`, `repository`, and `policies.*` (e.g. `protect_main`).
- **`.devspark.work/devspark.user.json`** (git-ignored, per developer) holds **personal preferences**: `assistant.name`, `shell`, and `preferences.*` (e.g. `auto_push`).

Read them via the shared helpers in `common.ps1` / `common.sh` (`Get-DevSparkConfig`, `Get-DevSparkUserConfig`, `Get-AssistantName`, `Get-DevSparkPreference`; bash: `get_devspark_config_value`, `get_devspark_user_config_value`, `get_assistant_name`, `get_devspark_preference`). Resolution precedence is **env var → user file → repo file → default**. Recognized keys:

- `assistant.name` (user) — the developer-chosen name for the guide voice (default `Spark`; env override `DEVSPARK_ASSISTANT_NAME`). Address and sign as this name. When the developer addresses this name in natural language (e.g. "hey Spark, where am I", "Spark, what's next"), treat it as invoking `/devspark-next` and answer with the orientation (repo, branch, platform) plus the next step.
- `platform` (repo) — `github` | `azdo` | `gitlab`. Authoritative host; resolved by `platform.*` (config wins over auto-detection). Never re-probe `gh`/`az` to guess the host when this is set.
- `shell` (user) — `powershell` | `bash`. Preferred script flavor when both `sh:` and `ps:` variants exist. Lets a macOS teammate choose `bash` without touching shared repo facts.
- `preferences.auto_push` (user) — when `true`, the developer has **pre-authorized** pushing the current feature/quickfix branch to its remote after a commit, without asking each time. Treat it like a standing `--auto` for that one action.
- `policies.protect_main` (repo) — team policy: when `true`, never commit or push directly to the default branch; branch first.

Git identity defaults to `git config user.name`. It may optionally be overridden per developer in the user file via `git_user` (env `DEVSPARK_GIT_USER` wins) — useful as a friendlier display name and as the deterministic key for personal `.devspark.work/{git-user}/` prompt and script overrides. Resolve it via `Get-GitUser` / `get_git_user`.

**`git_user` uniqueness (important):** because `git_user` is also the folder key for personal overrides (`.devspark.work/{git-user}/commands/`, `.devspark.work/{git-user}/scripts/`), two developers who set the same `git_user` value — or who copy a teammate's `devspark.user.json` as a starting template without changing it — will silently share the same personal override folder and pick up each other's customized prompts/scripts. Use a value that's unique within the repo (e.g. `firstname-lastinitial` or your actual `git config user.name`, which is already unique per commit author) rather than something generic like a role name or team name.

**Safety rules for `auto_push` (non-negotiable):**

- Only ever push the current topic branch to its own upstream. Never `--force`.
- `auto_push` never overrides `protect_main`: pushing directly to the default branch always requires an explicit human decision.
- Publishing a branch is visible to others but reversible; opening/merging a PR is not covered by `auto_push` and still follows the normal PR flow.

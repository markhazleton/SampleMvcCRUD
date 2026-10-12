# Preamble Module: Artifact Version Provenance

<!-- BSW.DevSpark — © 2026 Baylor Scott & White Health. Source: https://bsw-devspark.bswhive.com -->

Part of the Command Preamble Contract (`command-preamble-contract.md`). Section numbers are stable identities that other documents cite. Commands load this module when the contract's module table lists them as consumers.

## 12. Artifact Version Provenance

> **Every DevSpark-generated lifecycle artifact records the DevSpark version that produced that artifact instance.**

Without this, evaluating whether a framework change actually improved outcomes requires reconstructing framework versions from Git dates and upgrade commits — an inference that is both laborious and easy to get wrong.

**Resolution.** Commands resolve the installed version through the shared helper (`Get-DevSparkVersion` / `get_devspark_version` in `common.ps1` / `common.sh`), which reads `.devspark/BSW.DevSpark.version`. No prompt or template invents its own version detection. When the version cannot be determined, write `unknown` — never guess, and never block on it.

**Artifact-local, not centralized.** Each artifact describes its own producer. DevSpark does not maintain a lifecycle ledger, and a spec MUST NOT accumulate `planned_with:` / `analyzed_with:` / `verified_with:` fields — that would turn the spec into a mutable database of its own history.

**Two meanings, one field name.**

| Artifact | `devspark_version` means |
|---|---|
| `spec.md`, `plan.md`, `tasks.md` | the version under which the artifact was **authored** |
| `gates/*.md`, PR-review records | the version that produced **this specific instance** |

**Provenance stays truthful.** A command writes `devspark_version` only into artifacts it is legitimately creating or regenerating. Upgrading the framework does not rewrite existing artifacts, and running a later-phase command never restamps an earlier artifact. A route may therefore legitimately show `spec.md` at one version and `gates/verify.md` at a newer one — that is information, not inconsistency.

**Optional revision.** When the installed version is a development build (a `-dev` suffix, or a source-dogfood stamp) and a commit is readily available, a command MAY additionally record `devspark_revision: <short-sha>`. Version is the field that matters; revision is never required.

**Backward compatible.** The field is additive. Artifacts without it remain valid, every consumer tolerates its absence, and no historical artifact is migrated.

**Why it is here.** Historical effectiveness analysis MUST use artifact-recorded versions when present and infer from repository history only as a fallback. This is observability for the methodology itself, not an analytics subsystem — no command collects, aggregates, or reports these values.

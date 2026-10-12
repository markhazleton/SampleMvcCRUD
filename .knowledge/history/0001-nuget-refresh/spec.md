# NuGet refresh

**Tier**: Feature
**Status**: Complete
**Ratified**: 2026-10-11, user confirmed Feature and in-place implementation.

## Product Owner TL;DR
Update every direct NuGet dependency to the latest stable version that supports a successful .NET 10 solution build; retain and explain incompatible versions.

## Intent
Refresh dependencies across the five projects without changing public contracts, data models, or application behavior. Work in place on the current branch as explicitly requested.

## Acceptance criteria
1. Inventory all direct PackageReferences against NuGet stable releases; record upgrades and any exceptions.
2. Keep related package versions aligned and preserve net10.0; allow the installed newer .NET 10 SDK using global.json rollForward latestFeature while retaining the baseline SDK.
3. A clean Release solution build succeeds, with existing warnings distinguished from new warnings.
4. Existing solution tests pass; investigate update-induced failures and run a transitive vulnerability audit.

## Affected files
Five project files, global.json, and this feature's artifacts. Two explicit null guards and a reference-identity assertion address analyzer warnings for a warning-free build.

## Tasks
- [X] T1 Inventory latest stable NuGet versions (AC1).
- [X] T2 Update compatible dependencies and SDK resolution (AC1, AC2).
- [X] T3 Clean/build Release and resolve update regressions (AC3).
- [X] T4 Run tests/audit and record evidence/exceptions (AC4).

## Backbone coverage
1-4: preserve nullable, language, analyzers and usings settings. 5-8: preserve repositories, DTO separation, DI/async and error contracts. 11: preserve CI workflows. 14: preserve .NET 10 and refresh dependencies. 15: preserve Docker configuration. 16: artifacts live in bold-docs/features. 17: prioritize security and correctness. Adopting 9,10,12,13: preserve educational security scope, existing MSTest tests and API documentation; document dependency evidence here.

## Open questions
None. Incompatible latest versions will be retained at the newest buildable stable version with an explanation.

## Rollback
Revert the package/SDK edits if validation fails; no data migration or external deployment is part of this task.

## Waivers
- Process override: Feature branch creation replaced by in-place work on main, explicitly requested by the user on 2026-10-11. No enforced backbone principle waived.

## Follow-up: GitHub dependency backlog
User authorized review and resolution of all repository PRs and Dependabot vulnerabilities in place on main. Scope includes remaining npm/tool/workflow updates, Docker lint repair, publishing validated commits and closing superseded dependency PRs.

- [X] T5 Review every open PR diff and security alert; apply compatible outstanding changes.
- [X] T6 Validate client assets, tools, clean build, tests, audits and Docker lint.
- [X] T7 Commit/push on main and resolve superseded PRs; verify GitHub state.

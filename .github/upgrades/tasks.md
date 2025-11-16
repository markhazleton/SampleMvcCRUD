# .NET 10 Migration Tasks

## Overview

This scenario upgrades the entire SampleMvcCRUD solution (7 projects) from .NET 9.0 to .NET 10.0 using the Big Bang strategy. All framework and package updates are performed atomically, followed by unified testing and a single commit.

**Progress**: 2/3 tasks complete (67%) ![67%](https://progress-bar.xyz/67)

## Tasks

### [✓] TASK-001: Atomic framework and package upgrade *(Completed: 2025-11-16 12:58)*
**References**: Plan §Phase 1, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update `<TargetFramework>` to net10.0 in all projects listed in Plan §Phase 1 (see Appendix A for file list)
- [✓] (2) Update package references per Plan §Package Update Reference (see Plan §5)
- [✓] (3) Restore all dependencies
- [✓] (4) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog
- [✓] (5) Solution builds with 0 errors and 0 warnings (**Verify**)

### [✓] TASK-002: Run full test suite *(Completed: 2025-11-16 12:59)*
**References**: Plan §Phase 2, Plan §7.1, Plan §7.2

- [✓] (1) Run tests in Mwh.Sample.Domain.Tests and Mwh.Sample.Repository.Tests projects
- [✓] (2) Fix any test failures from upgrade (reference Plan §6 for common issues)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests passed with 0 failures (**Verify**)

### [▶] TASK-003: Final atomic commit
**References**: Plan §10.2, Plan §10.3

- [▶] (1) Commit all changes with message: "feat: upgrade solution to .NET 10.0"
- [▶] (2) Changes committed successfully (**Verify**)

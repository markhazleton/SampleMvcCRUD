# UISampleSpark Constitution

## Mission

Educational reference project demonstrating ASP.NET Core CRUD patterns across multiple UI technologies.

## Technology

- **Language / platform**: C#, .NET 10 (`net10.0`, pinned via `global.json`), solution `UISampleSpark.sln`
- **API style**: mix of Minimal APIs and MVC controllers
- **Frontends demonstrated**: MVC, Razor Pages, React, Vue, htmx, Blazor
- **Testing**: MSTest, 25% coverage baseline goal
- **Containerization**: Docker, multi-stage, Alpine base, non-root
- **CI**: GitHub Actions — Test & Build, Security Scanning (CodeQL + Trivy, weekly), Docker Build & Push
- **Auth**: none (educational scope, by design)

## Core Principles

Status: **enforced** = checked today; **adopting** = goal being worked toward.

1. **Nullable reference types are mandatory** — all projects enable `<Nullable>enable</Nullable>`. *(enforced)*
2. **Latest C# language version** — all projects use `<LangVersion>latest</LangVersion>`. *(enforced)*
3. **Full code analysis** — `<AnalysisLevel>latest-all</AnalysisLevel>` with .NET analyzers; CA5xxx/CA3xxx security warnings are never suppressed. *(enforced)*
4. **Implicit usings via GlobalUsings.cs** — `<ImplicitUsings>enable</ImplicitUsings>` with a `GlobalUsings.cs`. *(enforced)*
5. **Repository pattern for all data access** — access goes through interfaces (e.g. `IEmployeeService`, `IEmployeeClient`); raw SQL (`FromSqlRaw`/`ExecuteSqlRaw`) is prohibited in favor of EF Core LINQ. *(enforced)*
6. **DTO/Entity separation** — DTOs (e.g. `EmployeeDto`) are always separate from EF entities (e.g. `Employee`). *(enforced)*
7. **Dependency injection everywhere** — services registered in DI and constructor-injected; all I/O is async/await; Domain and Repository layers use `ConfigureAwait(false)`. *(enforced)*
8. **RFC 7807 error contracts** — a global exception handler catches unhandled exceptions; API errors return ProblemDetails; controllers return proper status codes via `ActionResult<T>`/`IActionResult`. *(enforced)*
9. **Educational security scope — no authentication by design** — auth is intentionally omitted, but README/SECURITY.md must state "not production-ready", CodeQL + Trivy scans run on every commit, HTTPS is enforced outside development, and secrets never live in code (User Secrets in dev, environment variables in prod). *(adopting)*
10. **MSTest with 25% coverage baseline** — test files named `*Test.cs`/`*Tests.cs`, Arrange-Act-Assert; coverage is a baseline goal, not a hard gate. *(adopting)*
11. **Three required CI/CD workflows** — Test & Build (PRs and pushes to main), Security Scanning (CodeQL + Trivy, weekly), Docker Build & Push (multi-stage, push to Docker Hub on main). *(enforced)*
12. **Basic observability** — a `/health` endpoint and Swagger/OpenAPI docs at `/swagger`; structured logging via `ILogger<T>` and Application Insights encouraged. *(adopting)*
13. **Documentation upkeep** — README.md stays current; CHANGELOG.md, XML doc comments on public APIs, and descriptive Swagger metadata encouraged. *(adopting)*
14. **.NET 10, updated quarterly** — target the latest .NET via `global.json`; NuGet packages updated quarterly via Dependabot. *(enforced)*
15. **Docker: multi-stage, Alpine, non-root, hadolint-clean** — aggressive package updates; documented hadolint exceptions allowed. *(enforced)*
16. **AI-generated artifacts stay out of source directories** — session work, drafts, and audit reports live outside the source tree and project root. *(enforced)*
17. **Priority order**: Security > Correctness > Educational Value > Performance > Cosmetics. *(enforced)*

## Governance

Amendment history: see `constitution-history.md`. Architecture decisions: `.knowledge/decisions/`.

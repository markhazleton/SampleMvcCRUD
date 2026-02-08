# CLAUDE.md — Agent Instructions

All agents working in this repository MUST follow the project constitution at `/.documentation/memory/constitution.md`.

## Project Overview

Educational ASP.NET Core project exploring multiple front-end UI technologies (MVC, Razor Pages, React, Vue, htmx, Blazor). Not production-ready by design (no auth).

- **Framework**: .NET 10 (`global.json` specifies SDK)
- **Solution**: `UISampleSpark.sln`
- **Build/Test**: `dotnet build UISampleSpark.sln` / `dotnet test UISampleSpark.sln`

## Mandatory Coding Rules

### C# Project Settings (all .csproj files)
- `<Nullable>enable</Nullable>`
- `<LangVersion>latest</LangVersion>`
- `<AnalysisLevel>latest-all</AnalysisLevel>` with .NET analyzers enabled
- `<ImplicitUsings>enable</ImplicitUsings>` with `GlobalUsings.cs`
- Never suppress security analyzer warnings (CA5xxx, CA3xxx)

### Architecture
- **Repository pattern**: All data access through interfaces (`IEmployeeService`, `IEmployeeClient`)
- **No raw SQL**: Use EF Core LINQ only — `FromSqlRaw`/`ExecuteSqlRaw` are prohibited
- **DTO/Entity separation**: DTOs (e.g., `EmployeeDto`) must be separate from EF entities (e.g., `Employee`)
- **Dependency injection**: All services registered in DI, injected via constructors
- **Async/await**: All I/O operations must be async
- **ConfigureAwait(false)**: Required on all async calls in Domain and Repository layers

### Error Handling & APIs
- Global exception handler must be present for unhandled exceptions
- API errors must return RFC 7807 ProblemDetails format
- Controllers must return proper HTTP status codes via `ActionResult<T>` or `IActionResult`

### Security
- This is an educational project — no authentication by design
- Never commit secrets; use User Secrets (dev) or environment variables (prod)
- HTTPS redirection required in non-development environments

### Testing
- Framework: MSTest
- Test files named `*Test.cs` or `*Tests.cs`
- Follow Arrange-Act-Assert pattern
- Target: 25% code coverage baseline

### Docker
- Multi-stage builds required
- Alpine base images preferred
- Run as non-root user
- Must pass hadolint linting

## File Organization

- AI/Copilot session work: `/.documentation/copilot/session-{YYYY-MM-DD}/`
- Draft documents: `/.documentation/copilot/drafts/`
- Audit reports: `/.documentation/copilot/audit/`
- Never mix AI-generated artifacts with source code in project root

## CI/CD

Three required GitHub Actions workflows:
1. **Test & Build** — runs on PRs and pushes to main
2. **Security Scanning** — CodeQL + Trivy, weekly schedule
3. **Docker Build & Push** — multi-stage build, push to Docker Hub on main

## Priority Order

Security > Correctness > Educational Value > Performance > Cosmetics

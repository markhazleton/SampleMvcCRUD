<!--
SYNC IMPACT REPORT
==================
Version change: 0.0.0 (template) → 1.0.0
Modified principles: N/A (initial formalization)
Added sections:
  - I. Security (MANDATORY)
  - II. Error Handling & API Contracts (MANDATORY)
  - III. Data Layer Separation (MANDATORY)
  - IV. Dependency Injection & Service Architecture (MANDATORY)
  - V. Testing Standards (MANDATORY)
  - VI. API Documentation (MANDATORY)
  - VII. Code Quality Standards (MANDATORY)
  - VIII. Observability (SHOULD)
  - IX. CI/CD & Workflow (SHOULD)
  - Aspirational Principles (tracking section)
Removed sections: None
Templates requiring updates:
  - .specify/templates/plan-template.md: ✅ Compatible
  - .specify/templates/spec-template.md: ✅ Compatible
  - .specify/templates/tasks-template.md: ✅ Compatible
Follow-up TODOs: None
-->

# SampleMvcCRUD Constitution

## Guiding Philosophy

**Priority Order**: Security > Correctness > Performance > Architecture > Cosmetics

Don't obsess over Clean Architecture if you don't have auth, versioning, or consistent error handling.

---

## Core Principles

### I. Security (MANDATORY)

Security is non-negotiable. These principles take precedence over all others.

- All APIs MUST use HTTPS — `UseHttpsRedirection()` + `UseHsts()` in production
- All production APIs MUST implement JWT authentication with `[Authorize]` (TO BE IMPLEMENTED)
- Role-Based Access Control (RBAC) MUST be implemented for protected endpoints (TO BE IMPLEMENTED)
- Sensitive data MUST NOT be logged
- All user input MUST be validated before processing

**Rationale**: Security baseline. No HTTPS = game over. Auth without authorization is incomplete.

---

### II. Error Handling & API Contracts (MANDATORY)

APIs are contracts. Breaking them breaks clients.

- All API endpoints MUST return proper HTTP status codes via `ActionResult<T>`
- All API errors MUST return `ProblemDetails` format (RFC 7807)
- Global exception handling via `IExceptionHandler` SHOULD be implemented (TO BE IMPLEMENTED)
- All API endpoints MUST use `[ProducesResponseType]` attributes

**Rationale**: Standardized error handling enables debuggable APIs and prevents inconsistent responses.

---

### III. Data Layer Separation (MANDATORY)

Prevents tight coupling and accidental data leaks.

- EF entities MUST NOT be exposed directly to API consumers
- All API responses MUST use DTOs mapped from entities
- Read operations MUST use `AsNoTracking()` for performance
- All DbContext operations MUST use async/await with CancellationToken

**Rationale**: Huge performance wins at scale. Prevents accidental data leaks.

---

### IV. Dependency Injection & Service Architecture (MANDATORY)

Core to ASP.NET Core's design philosophy.

- All services MUST be registered via interface-based DI
- Services MUST use scoped lifetime for database contexts
- Business logic MUST reside in services, not controllers
- Controllers MUST be thin, delegating to injected services

**Rationale**: Testability and sanity saver. Core to ASP.NET Core's design philosophy.

---

### V. Testing Standards (MANDATORY)

Ensures correctness and maintainability.

- All services MUST have unit tests
- Test projects MUST use MSTest framework
- Tests SHOULD follow `Method_StateUnderTest_ExpectedBehavior` naming convention
- Test projects MUST be separate from source projects

**Rationale**: Tests provide safety net for refactoring and ensure correctness.

---

### VI. API Documentation (MANDATORY)

Professional APIs are documented APIs.

- All API endpoints MUST be documented with Swagger/OpenAPI
- API methods MUST include XML documentation comments
- Swagger UI MUST be available at `/swagger`

**Rationale**: Self-documenting APIs reduce onboarding friction and integration errors.

---

### VII. Code Quality Standards (MANDATORY)

Enforced at build time.

- All projects MUST enable nullable reference types (`<Nullable>enable</Nullable>`)
- All projects MUST use latest C# language version
- Code analysis MUST be enabled via `.editorconfig` and `<EnableNETAnalyzers>true</EnableNETAnalyzers>`
- Code style MUST be enforced in build (`<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`)

**Rationale**: Consistent code quality reduces bugs and improves maintainability.

---

### VIII. Observability (SHOULD)

Essential for production debugging.

- Application Insights SHOULD be configured for production telemetry
- Health check endpoint MUST be available at `/health`
- Structured logging (Serilog) SHOULD be implemented before production deployment
- Logging MUST NOT include sensitive data (passwords, tokens, PII)

**Rationale**: Essential once things go wrong in production.

---

### IX. CI/CD & Workflow (SHOULD)

Automated quality gates.

- All pushes to main SHOULD trigger CI build and tests
- GitHub Actions SHOULD run build, test, and deploy workflows
- Health check verification SHOULD run post-deployment

**Rationale**: Automated quality gates catch issues before production.

---

## Additional Constraints

### API Versioning

- API routes SHOULD follow `/api/{resource}` pattern
- Version structure SHOULD use folder-based organization (v1/, v2/)
- You *will* need versioning eventually

### Performance Standards

- Prefer async/await for all I/O operations (MUST)
- Use `AsNoTracking()` for read-only queries (MUST)
- Consider rate limiting for production APIs (SHOULD)

### Technology Stack

- .NET 10 / C# (latest LTS)
- ASP.NET Core MVC & Razor Pages
- Entity Framework Core
- Swashbuckle (Swagger/OpenAPI)
- MSTest for unit testing

---

## Aspirational Principles (NOT YET IMPLEMENTED)

| Area | Principle | Priority Tier |
|------|-----------|---------------|
| Authentication | JWT with `[Authorize]` | Tier 1 |
| Authorization | Role-Based Access Control | Tier 1 |
| Global Error Handling | `IExceptionHandler` | Tier 1 |
| Structured Logging | Serilog integration | Tier 3 |
| Rate Limiting | API throttling | Tier 3 |
| FluentValidation | Request validation | Tier 4 |

---

## Governance

- Constitution supersedes conflicting practices
- Amendments require documentation in PR description
- No formal approval process required (solo-maintained project)
- All PRs/reviews SHOULD verify compliance with MUST principles
- Aspirational principles SHOULD be implemented incrementally based on priority tier
- Use `.github/copilot-instructions.md` for runtime development guidance

**Version**: 1.0.0 | **Ratified**: 2026-01-30 | **Last Amended**: 2026-01-30

<!--
═══════════════════════════════════════════════════════════════════════════════
CONSTITUTION FORMALIZATION - SYNC IMPACT REPORT
═══════════════════════════════════════════════════════════════════════════════

VERSION CHANGE: Template → v1.0.0 (Initial Release)

DATE: 2026-02-05
METHOD: /speckit.discover-constitution → /speckit.constitution formalization
SOURCE: Analyzed 94 C# files, 10 project files, 5 workflows, 4 documentation files

PRINCIPLES ESTABLISHED (11 Core):
  I.   Code Quality & Safety (nullable, analysis, latest C#)
  II.  Architecture & Design Patterns (repository, DI, async/await)
  III. Error Handling & API Contracts (ProblemDetails, global exception handler)
  IV.  Security Posture (educational scope, no auth by design)
  V.   Testing Standards (MSTest, 25% coverage goal)
  VI.  CI/CD & DevOps (3 required workflows)
  VII. Observability & Health (health checks, Swagger)
  VIII. Documentation Standards (README, CHANGELOG, XML docs)
  IX.  Dependency Management (.NET 10, quarterly updates)
  X.   Docker & Containerization (Alpine, multi-stage, non-root)
  XI.  AI-Assisted Development & Documentation (/.documentation/copilot/ structure)

TEMPLATES REQUIRING UPDATES:
  ✅ /.documentation/templates/plan-template.md - Review for constitution alignment
  ✅ /.documentation/templates/spec-template.md - Review for constitution alignment
  ✅ /.documentation/templates/tasks-template.md - Review for constitution alignment
  ⚠️  /.github/prompts/*.md - Update agent instructions if referencing old patterns

ACTION ITEMS IDENTIFIED:
  🔴 HIGH: Implement global IExceptionHandler (Principle III requirement)
  🔴 HIGH: Create .github/workflows/test-build.yml (Principle VI requirement)
  🟡 MEDIUM: Update SECURITY.md to document educational scope (Principle IV)
  🟡 MEDIUM: Increase test coverage to 25% baseline (Principle V)
  🟢 LOW: Add ILogger<T> to Repository/Services (Principle VII - SHOULD)
  🟢 LOW: Improve XML documentation coverage (Principle VIII - SHOULD)

FOLLOW-UP TASKS:
  1. Create GitHub issues for 6 action items above
  2. Run /speckit.site-audit to validate constitution compliance
  3. Update dependent templates to reference new principles
  4. Archive discovery session to /.documentation/copilot/session-2026-02-05/
  5. Tag this commit: git tag constitution-v1.0.0

COMPATIBILITY NOTES:
  - Educational scope (Principle IV) intentionally omits authentication
  - Test coverage goal (25%) is baseline; increase incrementally
  - AI documentation structure (Principle XI) requires migration of existing drafts
  - All MUST principles already have 80%+ compliance in current codebase

═══════════════════════════════════════════════════════════════════════════════
-->
# SampleMvcCRUD Constitution

**Mission**: Educational reference project demonstrating ASP.NET Core CRUD patterns and best practices.

---

## Core Principles

### I. Code Quality & Safety (MANDATORY)

This project maintains modern C# quality standards to serve as a reference for best practices.

- **Nullable Reference Types**: All projects MUST enable `<Nullable>enable</Nullable>` (MUST)
- **Latest C# Language**: All projects MUST use `<LangVersion>latest</LangVersion>` (MUST)
- **Code Analysis**: All projects MUST enable `<AnalysisLevel>latest-all</AnalysisLevel>` with .NET analyzers (MUST)
- **Security Rules**: Security analyzer warnings (CA5xxx, CA3xxx) MUST be set to `warning` level and not suppressed (MUST)
- **Implicit Usings**: All projects MUST enable `<ImplicitUsings>enable</ImplicitUsings>` with `GlobalUsings.cs` (MUST)

**Rationale**: Demonstrates modern C# safety features and prevents null-reference errors while serving as educational best practice.

---

### II. Architecture & Design Patterns (MANDATORY)

Clean architecture with clear separation of concerns demonstrates professional patterns for learners.

- **Repository Pattern**: All data access MUST go through repository interfaces (e.g., `IEmployeeService`, `IEmployeeClient`) (MUST)
- **No Raw SQL**: Raw SQL queries (`FromSqlRaw`, `ExecuteSqlRaw`) are PROHIBITED; use EF Core LINQ (MUST)
- **DTO/Entity Separation**: DTOs (e.g., `EmployeeDto`) MUST be separate from EF entities (e.g., `Employee`) (MUST)
- **Dependency Injection**: All services MUST be registered in DI container and injected via constructors (MUST)
- **Async/Await**: All I/O operations MUST use async/await patterns (MUST)
- **ConfigureAwait(false)**: All library code (Domain, Repository layers) MUST use `.ConfigureAwait(false)` on async operations (MUST)

**Rationale**: Clear layering enables testability, maintainability, and demonstrates enterprise patterns. ConfigureAwait prevents deadlocks in library code.

---

### III. Error Handling & API Contracts (MANDATORY)

Standardized error responses demonstrate production-ready API design.

- **Global Exception Handler**: A global exception handler MUST be implemented to catch unhandled exceptions (MUST)
- **ProblemDetails**: All API errors MUST return RFC 7807 ProblemDetails format (MUST)
- **HTTP Status Codes**: Controllers MUST return proper HTTP status codes via `ActionResult<T>` or `IActionResult` (MUST)
- **Custom Response Objects**: Service layer MAY use custom response wrappers (e.g., `EmployeeResponse`) for business logic encapsulation (MAY)

**Rationale**: Consistent error handling improves API consumer experience and demonstrates RESTful best practices.

---

### IV. Security Posture (EDUCATIONAL SCOPE)

This is an **educational reference project** - security features are **intentionally simplified** for learning purposes.

- **No Authentication Required**: Authentication/authorization is NOT implemented by design (EDUCATIONAL)
- **Security Documentation**: README and SECURITY.md MUST clearly state "not production-ready" and security limitations (MUST)
- **Security Scanning**: GitHub Actions MUST run CodeQL and Trivy security scans on all commits (MUST)
- **HTTPS**: Application MUST enforce HTTPS redirection in non-development environments (MUST)
- **No Secrets in Code**: Secrets MUST use User Secrets (dev) or environment variables (prod) (MUST)

**⚠️ WARNING**: This codebase is for education/demonstration only. Do NOT deploy to production without adding:
- JWT/OAuth authentication
- Authorization policies with `[Authorize]` attributes
- Rate limiting
- Input validation middleware
- CORS policies
- API key management

**Rationale**: Keeps code accessible for learners while maintaining security awareness through scanning and documentation.

---

### V. Testing Standards (ENCOURAGED)

Testing infrastructure is present to demonstrate patterns, with incremental coverage goals.

- **MSTest Framework**: All test projects MUST use MSTest framework (MUST)
- **Coverage Goal**: Aim for **25% code coverage** as baseline; increase over time (SHOULD)
- **Test Projects**: Each layer SHOULD have corresponding test project (Domain.Tests, Repository.Tests) (SHOULD)
- **Test File Naming**: Test files MUST use `*Test.cs` or `*Tests.cs` suffix (MUST)
- **Arrange-Act-Assert**: Unit tests SHOULD follow AAA pattern for clarity (SHOULD)

**Rationale**: Demonstrates testing patterns for educational value while setting achievable coverage targets.

---

### VI. CI/CD & DevOps (MANDATORY)

Automated workflows ensure code quality and demonstrate modern DevOps practices.

**Required Workflows**:

1. **Test & Build Workflow** (MUST)
   - Run on all PRs and pushes to main
   - Execute `dotnet test` with coverage reporting
   - Fail PRs if tests fail

2. **Security Scanning** (MUST)
   - CodeQL analysis for .NET security vulnerabilities
   - Trivy container scanning for Docker images
   - Upload results to GitHub Security tab
   - Run weekly on schedule

3. **Docker Build & Push** (MUST)
   - Build multi-stage Dockerfile
   - Push to Docker Hub on main branch
   - Run smoke tests on built container
   - Weekly rebuilds for security patches

**Rationale**: Demonstrates production CI/CD patterns and ensures code quality gates are automated.

---

### VII. Observability & Health (RECOMMENDED)

Basic observability demonstrates production monitoring patterns.

- **Health Checks**: `/health` endpoint SHOULD be implemented and monitored (SHOULD)
- **Application Insights**: Application Insights telemetry MAY be configured for production deployments (MAY)
- **Structured Logging**: Services SHOULD use `ILogger<T>` with structured logging for educational value (SHOULD)
- **Swagger/OpenAPI**: API documentation MUST be available at `/swagger` (MUST)

**Rationale**: Health checks and observability are essential production patterns worth demonstrating to learners.

---

### VIII. Documentation Standards (RECOMMENDED)

As an educational resource, documentation helps learners understand patterns and intent.

- **README Maintenance**: README.md MUST be kept up-to-date with features and deployment instructions (MUST)
- **CHANGELOG**: CHANGELOG.md SHOULD document milestones and breaking changes (SHOULD)
- **XML Documentation**: Public APIs, interfaces, and DTOs SHOULD have XML doc comments for Swagger (SHOULD)
- **Code Comments**: Complex business logic SHOULD have explanatory comments (SHOULD)
- **Swagger Metadata**: API endpoints SHOULD have descriptive summaries and response documentation (SHOULD)

**Rationale**: Documentation is critical for educational projects; learners benefit from clear explanations of intent.

---

### IX. Dependency Management (MANDATORY)

Keep dependencies current to demonstrate modern .NET ecosystem.

- **.NET Version**: Project MUST target latest .NET version (currently .NET 10) (MUST)
- **Package Updates**: NuGet packages SHOULD be updated quarterly for security patches (SHOULD)
- **Dependabot**: GitHub Dependabot SHOULD be enabled for automated dependency updates (SHOULD)
- **Global SDK**: `global.json` MUST specify SDK version with `allowPrerelease` for early adopters (MUST)

**Rationale**: Demonstrates commitment to modern .NET features and security best practices.

---

### X. Docker & Containerization (MANDATORY)

Containerization demonstrates cloud-native deployment patterns.

- **Multi-Stage Build**: Dockerfile MUST use multi-stage builds to minimize image size (MUST)
- **Alpine Linux**: Base images SHOULD use Alpine for security and size benefits (SHOULD)
- **Security Updates**: Dockerfile MUST run aggressive package updates (`apk upgrade`, `apt dist-upgrade`) (MUST)
- **Non-Root User**: Containers MUST run as non-root user (MUST)
- **Hadolint**: Dockerfile MUST pass hadolint linting (with documented exceptions) (MUST)

**Rationale**: Containerization is essential for modern deployment; demonstrates security-first Docker practices.

---

### XI. AI-Assisted Development & Documentation Organization (MANDATORY)

All AI/Copilot-generated artifacts, session work, and drafts must be organized under `/.documentation/copilot/` to maintain clean separation and discoverability.

**Required Structure**:

- **Session Work**: `/.documentation/copilot/session-{YYYY-MM-DD}/` - Session-specific work, planning, and context
- **Draft Documents**: `/.documentation/copilot/drafts/` - Work-in-progress documents not yet finalized
- **Audit Reports**: `/.documentation/copilot/audit/` - Code quality audits, compliance checks, site audits
- **Prompts & Templates**: `/.documentation/copilot/prompts/` - Reusable prompts and custom workflows

**File Organization Rules**:

- All Copilot-generated session work MUST go under `/.documentation/copilot/session-{date}/` (MUST)
- Constitution drafts and major document drafts MUST use `/.documentation/copilot/drafts/` (MUST)
- Never commit Copilot artifacts to project root or mixed with source code (MUST)
- Use descriptive filenames with domain prefix (e.g., `spec-auth-feature.md`, `audit-2026-02-security.md`) (SHOULD)

**Permanent Documentation Locations**:

- **Constitution**: `/.documentation/memory/constitution.md` (finalized)
- **Architecture Decisions**: `/.documentation/architecture/` or `/docs/architecture/`
- **Feature Specs**: `/.documentation/features/{feature-name}/` or project-appropriate location
- **Release Notes**: Root `CHANGELOG.md` or `/.documentation/releases/`

**Rationale**: Clean separation of AI-generated work prevents repository clutter and preserves the decision-making trail for educational value.

---

## Additional Patterns (Observed)

These patterns are consistently applied but not formalized as hard requirements:

### Build Configuration
- **Deterministic Builds**: Disabled (`<Deterministic>false</Deterministic>`) for timestamp-based versioning
- **Assembly Versioning**: Auto-generated from build timestamp: `10.YYMM.DDHH.MMSS`
- **Trim Analysis**: Enabled for performance (`<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`)

### Web Configuration
- **Configuration Binding Generator**: Enabled for source generation performance
- **Request Delegate Generator**: Enabled for minimal API performance
- **Optimization Preference**: Set to `Speed` for ASP.NET projects

### Editor Configuration
- **Organize Usings**: `dotnet_sort_system_directives_first = true`
- **CA1707 Suppressed**: Underscores allowed in test method names
- **Nullable Warnings**: Set to `suggestion` rather than `error` for incremental adoption

---

## Governance

### Amendment Process
- Constitution amendments require discussion and PR review
- Breaking changes to principles require updating affected code
- Major principle changes should be tagged in git (e.g., `constitution-v2.0`)
- All amendments must document rationale and include migration guidance

### Compliance
- All PRs SHOULD be reviewed against constitution principles
- CI/CD workflows enforce MUST requirements automatically
- SHOULD requirements are encouraged but not blocking
- Violations of MUST principles should be caught in code review or CI

### Educational Mission Priority
- When MUST requirements conflict with educational clarity, document the trade-off
- Demonstrate best practices while keeping code accessible to learners
- Prioritize: **Security > Correctness > Educational Value > Performance > Cosmetics**

### Review Schedule
- Constitution SHOULD be reviewed annually or when .NET major versions release
- Update patterns when new C# language features are adopted
- Archive old patterns in CHANGELOG for historical reference

---

## Version History

**Version**: 1.0.0 (Initial Release)  
**Ratified**: 2026-02-05  
**Last Amended**: 2026-02-05  
**Discovery Source**: Analyzed 94 C# source files, 10 project files, 5 workflow/config files  
**Principles Formalized**: 11 core + 16 observed patterns

### Change Log
- **v1.0.0** (2026-02-05): Initial constitution ratified based on codebase discovery
  - Formalized 11 core principles from existing patterns
  - Documented educational security scope
  - Established 25% test coverage baseline goal
  - Mandated CI/CD workflows (test, security, Docker)
  - Defined AI-assisted development documentation structure

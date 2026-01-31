- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.

---

## ASP.NET Core Web API Best Practices

Ranked by **real-world impact** (security, correctness, maintainability, performance) — from most critical to nice-to-have.

**Priority: Security > Correctness > Performance > Architecture > Cosmetics**

---

### Tier 1 — Non-negotiable

1. **Protect all APIs with HTTPS**
   Security baseline. No HTTPS = game over.

2. **JWT authentication with `[Authorize]`**
   Auth comes before architecture polish.

3. **Role-Based Access Control (RBAC)**
   Auth without authorization is incomplete.

4. **Return proper HTTP status codes (`ActionResult<T>`)**
   APIs are contracts. Breaking this breaks clients.

5. **Return `ProblemDetails` for all errors**
   Standardized error handling = debuggable APIs.

6. **Global exception handling (`IExceptionHandler`)**
   Prevents crashes, leaks, and inconsistent responses.

---

### Tier 2 — High impact, professional-grade APIs

7. **Avoid exposing EF entities, map to DTOs**
   Prevents tight coupling and accidental data leaks.

8. **Async/await + `AsNoTracking()`**
   Huge performance wins at scale.

9. **Thin controllers, business logic in services**
   Testability and sanity saver.

10. **Dependency Injection with scoped services & interfaces**
    Core to ASP.NET Core's design philosophy.

11. **API versioning (`/api/v1/...`)**
    You *will* need this eventually.

---

### Tier 3 — Operational excellence & maintainability

12. **Structured logging (Serilog, etc.)**
    Essential once things go wrong in production.

13. **Avoid logging sensitive data**
    Security + compliance best practice.

14. **Health checks & rate limiting**
    Important for production & abuse protection.

15. **Store config in appsettings + Options pattern**
    Clean, testable, and future-proof.

---

### Tier 4 — Nice-to-have / situational

16. **`[ApiController]` & RESTful routing**
    Helpful defaults, but not transformative alone.

17. **FluentValidation**
    Great, but not mandatory for small APIs.

18. **Clean Architecture + feature folders**
    Powerful for large teams, overkill for small apps.

19. **Grouping services with extension methods**
    Mostly cosmetic—helps readability, not behavior.
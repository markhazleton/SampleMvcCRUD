# .NET 10 Migration Plan

## 1. Executive Summary

### Scenario
Upgrade the UISampleSpark solution from .NET 9.0 to .NET 10.0, including all associated NuGet package updates.

### Scope
- **Total Projects**: 7 projects
- **Current State**: All projects targeting net9.0
- **Target State**: All projects targeting net10.0
- **Main Application**: Blazor web application (UISampleSpark.UI)
- **Total Lines of Code**: ~11,180 LOC across all projects

### Selected Strategy
**Big Bang Strategy** - All projects will be upgraded simultaneously in a single atomic operation.

### Strategy Rationale
The Big Bang approach is ideal for this solution because:
- **Small Solution Size**: Only 7 projects with clear dependency structure
- **Homogeneous Framework**: All projects currently on net9.0
- **Clean Dependencies**: Simple dependency graph with no circular references
- **Package Compatibility**: All required packages have net10.0 versions available
- **No Security Issues**: Zero security vulnerabilities detected
- **Manageable Codebase**: ~11K LOC is small enough to handle atomically
- **Modern SDK**: All projects are already SDK-style projects

### Complexity Assessment
**Overall Complexity: LOW**

Justification:
- All projects already on modern .NET (9.0)
- Clean package upgrade path (9.0.x → 10.0.0)
- No breaking changes anticipated between .NET 9 and 10
- Well-structured solution with clear separation of concerns
- Good test coverage (2 test projects)

### Critical Issues
**✅ No security vulnerabilities detected** in current package versions.

### Recommended Approach
Execute a single, coordinated upgrade of all projects and packages simultaneously, followed by comprehensive testing.

---

## 2. Migration Strategy

### 2.1 Approach Selection

**Chosen Strategy**: Big Bang Strategy

**Justification**:
- **Project Count**: 7 projects is well within the threshold for atomic upgrades
- **Dependency Structure**: Clear hierarchical structure (Domain → Repository → Web/Console/Tests)
- **Framework Alignment**: All projects on same framework version (net9.0)
- **Package Updates**: Straightforward version bumps (9.0.x → 10.0.0)
- **Risk Level**: Low risk due to minor version jump and mature .NET upgrade path
- **Team Efficiency**: Single coordinated effort is faster than phased approach

### 2.2 Dependency-Based Ordering

While the Big Bang strategy upgrades all projects simultaneously, understanding the dependency order helps prioritize testing and troubleshooting:

**Dependency Hierarchy**:
1. **Foundation Layer**: UISampleSpark.Core (no dependencies, 5 dependants)
2. **Data Layer**: UISampleSpark.Data (depends on Domain, 3 dependants)
3. **Service Layer**: UISampleSpark.HttpClientFactory (depends on Domain)
4. **Application Layer**: 
   - UISampleSpark.UI (depends on Repository, Domain)
   - UISampleSpark.CLI (depends on Repository, Domain)
5. **Test Layer**: 
   - UISampleSpark.Core.Tests (depends on Domain)
   - UISampleSpark.Data.Tests (depends on Repository)

**Critical Path**: Domain → Repository → Web/Console

### 2.3 Parallel vs Sequential Execution

**Atomic Upgrade Phase**: All project file and package updates happen simultaneously.

**Testing Phase**: Tests will be executed after the atomic upgrade:
- Foundation tests (Domain.Tests)
- Data layer tests (Repository.Tests)
- Integration validation (Web application startup)

### 2.4 Strategy-Specific Considerations

**Big Bang Strategy Principles Applied**:
- **Single Commit**: All framework and package updates in one commit
- **No Intermediate States**: Solution moves from net9.0 to net10.0 atomically
- **Unified Testing**: Test entire solution after upgrade completes
- **Fast Completion**: Entire upgrade completable in single development session

---

## 3. Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

```
Level 0 (Foundation):
  └─ UISampleSpark.Core (0 dependencies, 5 dependants)

Level 1 (Data Layer):
  └─ UISampleSpark.Data (1 dependency: Domain)
  └─ UISampleSpark.HttpClientFactory (1 dependency: Domain)

Level 2 (Application Layer):
  └─ UISampleSpark.UI (2 dependencies: Repository, Domain)
  └─ UISampleSpark.CLI (2 dependencies: Repository, Domain)

Level 3 (Test Layer):
  └─ UISampleSpark.Core.Tests (1 dependency: Domain)
  └─ UISampleSpark.Data.Tests (1 dependency: Repository)
```

### 3.2 Project Groupings

**Phase 0: Preparation**
- Verify .NET 10 SDK installation
- Create upgrade branch (✅ completed: upgrade-to-NET10)

**Phase 1: Atomic Upgrade**
All projects upgraded simultaneously:
- Foundation: UISampleSpark.Core
- Data: UISampleSpark.Data, UISampleSpark.HttpClientFactory
- Applications: UISampleSpark.UI, UISampleSpark.CLI
- Tests: UISampleSpark.Core.Tests, UISampleSpark.Data.Tests

**Phase 2: Test Validation**
Execute all test projects to verify upgrade success.

---

## 4. Project-by-Project Migration Plans

### Project: UISampleSpark.Core

**Current State**
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project dependencies
- Dependants: 5 projects (Repository, HttpClientFactory, Web, Console, Domain.Tests)
- Package Count: 1
- LOC: 1,567
- Files: 27

**Target State**
- Target Framework: net10.0
- Updated Packages: 1

**Migration Steps**

1. **Prerequisites**
   - None (foundation layer with no dependencies)

2. **Framework Update**
   - Update `UISampleSpark.Core.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | System.Drawing.Common | 9.0.8 | 10.0.0 | Framework compatibility |

4. **Expected Breaking Changes**
   - **System.Drawing.Common**: No breaking changes expected between 9.0 and 10.0
   - This package provides cross-platform graphics capabilities
   - API surface remains stable in .NET 10

5. **Code Modifications**
   - No code changes anticipated
   - Domain models should remain unchanged

6. **Testing Strategy**
   - Unit tests: Execute UISampleSpark.Core.Tests after upgrade
   - Verify all domain logic tests pass
   - Confirm no serialization or data type issues

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests pass (18 test files, 2,195 LOC)
   - [ ] No security warnings

---

### Project: UISampleSpark.Data

**Current State**
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 1 (UISampleSpark.Core)
- Dependants: 3 (Web, Console, Repository.Tests)
- Package Count: 4
- LOC: 1,014
- Files: 12

**Target State**
- Target Framework: net10.0
- Updated Packages: 3 (Entity Framework Core packages)

**Migration Steps**

1. **Prerequisites**
   - UISampleSpark.Core must be upgraded (happens simultaneously in Big Bang)

2. **Framework Update**
   - Update `UISampleSpark.Data.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.EntityFrameworkCore | 9.0.8 | 10.0.0 | Framework compatibility, new features |
   | Microsoft.EntityFrameworkCore.InMemory | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.EntityFrameworkCore.Sqlite | 9.0.8 | 10.0.0 | Framework compatibility |
   | Bogus | 35.6.3 | (no change) | Already compatible |

4. **Expected Breaking Changes**
   - **Entity Framework Core 10.0**: Review [EF Core 10.0 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes)
   - Potential areas:
     - Query translation improvements may affect complex LINQ queries
     - Model building conventions may have subtle changes
     - SQLite provider enhancements
   - Most EF Core upgrades between major versions are backward compatible

5. **Code Modifications**
   - Review DbContext implementations
   - Check for any obsolete API usage warnings after build
   - Verify migration compatibility if using EF migrations

6. **Testing Strategy**
   - Unit tests: Execute UISampleSpark.Data.Tests after upgrade
   - Verify all repository operations work correctly
   - Test in-memory database scenarios
   - Validate SQLite database operations

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests pass (10 test files, 1,188 LOC)
   - [ ] DbContext initialization successful
   - [ ] No security warnings

---

### Project: UISampleSpark.HttpClientFactory

**Current State**
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 1 (UISampleSpark.Core)
- Dependants: 0
- Package Count: 2
- LOC: 263
- Files: 4

**Target State**
- Target Framework: net10.0
- Updated Packages: 2

**Migration Steps**

1. **Prerequisites**
   - UISampleSpark.Core must be upgraded (happens simultaneously in Big Bang)

2. **Framework Update**
   - Update `UISampleSpark.HttpClientFactory.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.Extensions.Http | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Text.Json | 9.0.8 | 10.0.0 | Framework compatibility, performance improvements |

4. **Expected Breaking Changes**
   - **System.Text.Json 10.0**: Review [JSON serialization improvements](https://learn.microsoft.com/en-us/dotnet/core/compatibility/serialization/)
   - Potential areas:
     - Enhanced source generation support
     - Improved performance characteristics
     - New serialization options
   - **Microsoft.Extensions.Http**: Typically no breaking changes in minor updates

5. **Code Modifications**
   - Review HTTP client factory registrations
   - Check JSON serialization/deserialization code
   - Verify any custom serializer options

6. **Testing Strategy**
   - Integration tests: Verify HTTP client factory instantiation
   - Test JSON serialization/deserialization scenarios
   - Validate any retry policies or handlers

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] HTTP client factory works correctly
   - [ ] JSON serialization/deserialization validated
   - [ ] No security warnings

---

### Project: UISampleSpark.UI

**Current State**
- Target Framework: net9.0
- Project Type: AspNetCore (Blazor)
- Dependencies: 2 (UISampleSpark.Data, UISampleSpark.Core)
- Dependants: 0
- Package Count: 15
- LOC: 4,883
- Files: 91

**Target State**
- Target Framework: net10.0
- Updated Packages: 7

**Migration Steps**

1. **Prerequisites**
   - UISampleSpark.Data and UISampleSpark.Core must be upgraded (happens simultaneously in Big Bang)

2. **Framework Update**
   - Update `UISampleSpark.UI.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.EntityFrameworkCore | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.EntityFrameworkCore.InMemory | 9.0.8 | 10.0.0 | Framework compatibility |
   | Microsoft.EntityFrameworkCore.SqlServer | 9.0.8 | 10.0.0 | Framework compatibility, SQL Server improvements |
   | Microsoft.EntityFrameworkCore.Tools | 9.0.8 | 10.0.0 | Framework compatibility, tooling support |
   | Microsoft.VisualStudio.Web.CodeGeneration.Design | 9.0.0 | 10.0.0-rc.1.25458.5 | Code generation support for .NET 10 |
   | System.Formats.Asn1 | 9.0.8 | 10.0.0 | Framework compatibility |
   | System.Text.Json | 9.0.8 | 10.0.0 | Framework compatibility |

   **Compatible Packages (no update needed)**:
   - Azure.Extensions.AspNetCore.Configuration.Secrets (1.4.0)
   - Azure.Identity (1.15.0)
   - Microsoft.ApplicationInsights (2.23.0)
   - Microsoft.ApplicationInsights.AspNetCore (2.23.0)
   - Swashbuckle.AspNetCore (9.0.4)
   - WebSpark.Bootswatch (1.20.1)
   - WebSpark.HttpClientUtility (1.1.0)
   - Westwind.AspNetCore.Markdown (3.24.0)

4. **Expected Breaking Changes**
   - **Blazor .NET 10**: Review [ASP.NET Core 10.0 breaking changes](https://learn.microsoft.com/en-us/aspnet/core/migration/90-to-100)
   - Potential areas for Blazor applications:
     - Enhanced rendering modes
     - Improved component lifecycle
     - WebAssembly enhancements
     - SignalR improvements
   - **EF Core**: Same considerations as UISampleSpark.Data
   - **System.Text.Json**: Enhanced serialization capabilities

5. **Code Modifications**
   - Review Program.cs for any obsolete middleware registrations
   - Check Blazor component lifecycle methods
   - Verify dependency injection registrations
   - Review authentication/authorization configurations
   - Check Azure Key Vault integration (Azure.Extensions packages)
   - Verify Application Insights telemetry configuration

6. **Testing Strategy**
   - Manual testing: Start application and verify core functionality
   - Key scenarios for Blazor application:
     - Application starts successfully
     - Database connectivity (SQL Server and In-Memory)
     - Blazor components render correctly
     - Navigation between pages works
     - CRUD operations function properly
     - Swagger UI accessible
     - Application Insights logging active
     - Azure Key Vault configuration retrieval
   - Performance validation: Check startup time and page load performance

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Application starts without exceptions
   - [ ] Database context initializes successfully
   - [ ] Blazor components render correctly
   - [ ] API endpoints respond correctly
   - [ ] Swagger UI accessible
   - [ ] Application Insights configured
   - [ ] Azure services integration works
   - [ ] No security warnings

---

### Project: UISampleSpark.CLI

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp
- Dependencies: 2 (UISampleSpark.Data, UISampleSpark.Core)
- Dependants: 0
- Package Count: 1
- LOC: 70
- Files: 3

**Target State**
- Target Framework: net10.0
- Updated Packages: 0 (Bogus is compatible)

**Migration Steps**

1. **Prerequisites**
   - UISampleSpark.Data and UISampleSpark.Core must be upgraded (happens simultaneously in Big Bang)

2. **Framework Update**
   - Update `UISampleSpark.CLI.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Bogus | 35.6.3 | (no change) | Already compatible with .NET 10 |

4. **Expected Breaking Changes**
   - No package updates required
   - Minimal risk - console app with simple dependencies

5. **Code Modifications**
   - No code changes anticipated
   - Verify any file I/O or console output operations

6. **Testing Strategy**
   - Manual testing: Run console application
   - Verify expected output
   - Validate repository interactions

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Console application runs successfully
   - [ ] Expected output generated
   - [ ] No security warnings

---

### Project: UISampleSpark.Core.Tests

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test Project)
- Dependencies: 1 (UISampleSpark.Core)
- Dependants: 0
- Package Count: 4
- LOC: 2,195
- Files: 18

**Target State**
- Target Framework: net10.0
- Updated Packages: 0 (all test packages compatible)

**Migration Steps**

1. **Prerequisites**
   - UISampleSpark.Core must be upgraded (happens simultaneously in Big Bang)

2. **Framework Update**
   - Update `UISampleSpark.Core.Tests.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | coverlet.collector | 6.0.4 | (no change) | Compatible |
   | Microsoft.NET.Test.Sdk | 17.14.1 | (no change) | Compatible |
   | MSTest.TestAdapter | 3.10.4 | (no change) | Compatible |
   | MSTest.TestFramework | 3.10.4 | (no change) | Compatible |

4. **Expected Breaking Changes**
   - No breaking changes expected
   - MSTest framework is stable across .NET versions

5. **Code Modifications**
   - No code changes anticipated
   - Test methods should work without modification

6. **Testing Strategy**
   - Execute all tests in this project
   - Verify test discovery works correctly
   - Confirm code coverage collection functions

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests discovered
   - [ ] All unit tests pass
   - [ ] Code coverage collection works
   - [ ] No security warnings

---

### Project: UISampleSpark.Data.Tests

**Current State**
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test Project)
- Dependencies: 1 (UISampleSpark.Data)
- Dependants: 0
- Package Count: 5
- LOC: 1,188
- Files: 10

**Target State**
- Target Framework: net10.0
- Updated Packages: 0 (all test packages compatible)

**Migration Steps**

1. **Prerequisites**
   - UISampleSpark.Data must be upgraded (happens simultaneously in Big Bang)

2. **Framework Update**
   - Update `UISampleSpark.Data.Tests.csproj`: Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

3. **Package Updates**

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | coverlet.collector | 6.0.4 | (no change) | Compatible |
   | Microsoft.NET.Test.Sdk | 17.14.1 | (no change) | Compatible |
   | Moq | 4.20.72 | (no change) | Compatible |
   | MSTest.TestAdapter | 3.10.4 | (no change) | Compatible |
   | MSTest.TestFramework | 3.10.4 | (no change) | Compatible |

4. **Expected Breaking Changes**
   - No breaking changes expected
   - Moq framework is compatible across .NET versions

5. **Code Modifications**
   - No code changes anticipated
   - Mock setups should work without modification

6. **Testing Strategy**
   - Execute all tests in this project
   - Verify mock objects work correctly
   - Confirm repository test scenarios pass

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests discovered
   - [ ] All unit tests pass
   - [ ] Moq framework works correctly
   - [ ] Code coverage collection works
   - [ ] No security warnings

---

## 5. Package Update Reference

### Common Package Updates (affecting multiple projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.EntityFrameworkCore | 9.0.8 | 10.0.0 | Repository, Web | Framework compatibility, new features |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.8 | 10.0.0 | Repository, Web | Framework compatibility |
| System.Text.Json | 9.0.8 | 10.0.0 | HttpClientFactory, Web | Framework compatibility, performance |

### Project-Specific Package Updates

**UISampleSpark.Core**:
- System.Drawing.Common: 9.0.8 → 10.0.0

**UISampleSpark.Data**:
- Microsoft.EntityFrameworkCore.Sqlite: 9.0.8 → 10.0.0

**UISampleSpark.HttpClientFactory**:
- Microsoft.Extensions.Http: 9.0.8 → 10.0.0

**UISampleSpark.UI**:
- Microsoft.EntityFrameworkCore.SqlServer: 9.0.8 → 10.0.0
- Microsoft.EntityFrameworkCore.Tools: 9.0.8 → 10.0.0
- Microsoft.VisualStudio.Web.CodeGeneration.Design: 9.0.0 → 10.0.0-rc.1.25458.5
- System.Formats.Asn1: 9.0.8 → 10.0.0

### Compatible Packages (no update required)

These packages are already compatible with .NET 10 and require no changes:
- Azure.Extensions.AspNetCore.Configuration.Secrets (1.4.0)
- Azure.Identity (1.15.0)
- Bogus (35.6.3)
- coverlet.collector (6.0.4)
- Microsoft.ApplicationInsights (2.23.0)
- Microsoft.ApplicationInsights.AspNetCore (2.23.0)
- Microsoft.NET.Test.Sdk (17.14.1)
- Moq (4.20.72)
- MSTest.TestAdapter (3.10.4)
- MSTest.TestFramework (3.10.4)
- Swashbuckle.AspNetCore (9.0.4)
- WebSpark.Bootswatch (1.20.1)
- WebSpark.HttpClientUtility (1.1.0)
- Westwind.AspNetCore.Markdown (3.24.0)

---

## 6. Breaking Changes Catalog

### .NET 9.0 → 10.0 Framework Changes

**Review Official Documentation**:
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [ASP.NET Core 10.0 Migration Guide](https://learn.microsoft.com/en-us/aspnet/core/migration/90-to-100)

**Expected Areas of Impact**:

1. **Entity Framework Core 10.0**
   - Query translation improvements
   - Enhanced performance optimizations
   - Provider-specific improvements (SQL Server, SQLite)
   - Potential changes in model building conventions
   - Review: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes

2. **ASP.NET Core 10.0 (Blazor)**
   - Enhanced rendering modes
   - Improved streaming rendering
   - WebAssembly enhancements
   - SignalR improvements
   - Middleware ordering changes

3. **System.Text.Json**
   - Enhanced source generation
   - Improved performance characteristics
   - New serialization options
   - Potential behavior changes in edge cases

4. **General .NET 10 Runtime**
   - Performance improvements
   - GC enhancements
   - JIT compiler improvements
   - Potential API surface changes

### Mitigation Strategy

Since this is a minor version upgrade (9 → 10) and all packages have clear upgrade paths:
- Most breaking changes are expected to be minimal
- Comprehensive testing will identify any issues
- The Big Bang approach allows us to address all issues in one coordinated effort

---

## 7. Testing Strategy

### 7.1 Multi-Level Testing

#### Build Validation (Immediate)
After atomic upgrade completes:
- [ ] All 7 projects build without errors
- [ ] All 7 projects build without warnings
- [ ] NuGet package restore succeeds
- [ ] No dependency conflicts

#### Unit Testing (Automated)
Execute all test projects:
- [ ] **UISampleSpark.Core.Tests**: All domain logic tests pass
  - 18 test files
  - 2,195 LOC
  - Focus: Domain model validation, business logic
- [ ] **UISampleSpark.Data.Tests**: All repository tests pass
  - 10 test files
  - 1,188 LOC
  - Focus: Data access, EF Core operations, mocking

#### Integration Testing (Manual)
Main application validation:
- [ ] **UISampleSpark.UI** (Blazor application):
  - Application starts without errors
  - Database initialization successful
  - Blazor components render correctly
  - Navigation works across all pages
  - CRUD operations function correctly
  - API endpoints respond
  - Swagger UI accessible
  - Application Insights telemetry active
  - Azure Key Vault configuration loads

- [ ] **UISampleSpark.CLI**:
  - Console application runs successfully
  - Expected output generated
  - Repository interactions work

### 7.2 Testing Sequence

Following Big Bang strategy, testing occurs after atomic upgrade:

**Step 1: Build Verification**
```bash
dotnet restore
dotnet build --no-restore
```
Expected: 0 errors, 0 warnings

**Step 2: Run Domain Tests**
```bash
dotnet test UISampleSpark.Core.Tests\UISampleSpark.Core.Tests.csproj
```
Expected: All tests pass

**Step 3: Run Repository Tests**
```bash
dotnet test UISampleSpark.Data.Tests\UISampleSpark.Data.Tests.csproj
```
Expected: All tests pass

**Step 4: Run Blazor Application**
```bash
dotnet run --project UISampleSpark.UI\UISampleSpark.UI.csproj
```
Expected: Application starts, navigate to https://localhost:5001 and verify functionality

**Step 5: Run Console Application**
```bash
dotnet run --project UISampleSpark.CLI\UISampleSpark.CLI.csproj
```
Expected: Successful execution with expected output

### 7.3 Success Criteria

The upgrade is successful when:
- [ ] All 7 projects build with 0 errors and 0 warnings
- [ ] All unit tests pass (Domain.Tests + Repository.Tests)
- [ ] Blazor application starts and core functionality verified
- [ ] Console application runs successfully
- [ ] No security warnings from NuGet packages
- [ ] No performance degradation observed

---

## 8. Risk Management

### 8.1 Risk Assessment

**Overall Risk: LOW**

Justification:
- Minor version upgrade (9 → 10)
- Microsoft's strong backward compatibility commitment
- No deprecated APIs in use (based on assessment)
- Good test coverage
- Small, manageable codebase

### 8.2 Identified Risks and Mitigation

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| EF Core query translation changes | Low | Medium | Execute all repository tests; review query logs |
| Blazor rendering behavior changes | Low | Medium | Manual testing of all Blazor components |
| System.Text.Json serialization changes | Low | Low | Test API endpoints and data serialization |
| Azure SDK compatibility issues | Very Low | Low | Packages marked as compatible; test Azure integration |
| Build failures due to package conflicts | Very Low | High | Clear dependency resolution; atomic package updates |

### 8.3 Contingency Plans

**If Build Fails After Framework Update**:
1. Review build errors and identify root cause
2. Check for package version conflicts using `dotnet list package --include-transitive`
3. Address package dependencies one at a time
4. Consult .NET 10 migration documentation

**If Tests Fail**:
1. Isolate failing tests
2. Review test code for framework-specific assumptions
3. Check EF Core query translations if data tests fail
4. Update test assertions if behavior changed intentionally in .NET 10

**If Blazor Application Fails to Start**:
1. Review Program.cs for obsolete middleware
2. Check dependency injection registrations
3. Verify database connection strings
4. Review Azure configuration loading

**Rollback Plan**:
- Git branch: upgrade-to-NET10 allows easy rollback
- Revert to main branch: `git checkout main`
- All changes isolated to upgrade branch
- No impact on production until merge

---

## 9. Timeline and Effort Estimates

### 9.1 Big Bang Strategy Timeline

**Estimated Total Duration: 2-4 hours** (single development session)

### 9.2 Phase Breakdown

#### Phase 0: Preparation (✅ Complete)
- ✅ .NET 10 SDK verification
- ✅ Branch creation (upgrade-to-NET10)
- ✅ Assessment analysis
**Actual Time**: Complete

#### Phase 1: Atomic Upgrade (Estimated: 30-45 minutes)
**Operations**:
- Update all 7 project files (TargetFramework)
- Update all package references (11 packages across projects)
- Restore dependencies
- Build solution
- Fix any compilation errors

**Estimated Time**: 30-45 minutes
- Project file updates: 10 minutes
- Package updates: 10 minutes
- Build and resolve issues: 15-25 minutes

**Deliverable**: Solution builds with 0 errors, 0 warnings

#### Phase 2: Testing (Estimated: 60-90 minutes)
**Operations**:
- Execute unit tests (Domain.Tests, Repository.Tests)
- Address any test failures
- Manual testing of Blazor application
- Console application verification
- Performance validation

**Estimated Time**: 60-90 minutes
- Automated tests: 10 minutes
- Test failure resolution (if any): 20-30 minutes
- Manual testing: 30-40 minutes
- Documentation: 10 minutes

**Deliverable**: All tests pass, applications verified

#### Phase 3: Finalization (Estimated: 15-30 minutes)
**Operations**:
- Final validation
- Commit changes
- Create pull request
- Documentation updates

**Estimated Time**: 15-30 minutes

**Deliverable**: Ready for code review and merge

### 9.3 Per-Project Effort Estimate

| Project | Complexity | Changes | Estimated Time | Risk |
|---------|------------|---------|----------------|------|
| UISampleSpark.Core | Low | TFM + 1 package | 5 minutes | Low |
| UISampleSpark.Data | Low | TFM + 3 packages | 10 minutes | Low |
| UISampleSpark.HttpClientFactory | Low | TFM + 2 packages | 5 minutes | Low |
| UISampleSpark.UI | Medium | TFM + 7 packages | 15 minutes | Low-Medium |
| UISampleSpark.CLI | Low | TFM only | 5 minutes | Low |
| UISampleSpark.Core.Tests | Low | TFM only | 5 minutes | Low |
| UISampleSpark.Data.Tests | Low | TFM only | 5 minutes | Low |

**Note**: Times reflect individual project complexity; Big Bang executes all simultaneously.

### 9.4 Resource Requirements

**Developer Skills**:
- .NET development experience
- Entity Framework Core knowledge
- Blazor/ASP.NET Core familiarity
- Git version control proficiency

**Team Capacity**:
- Single developer sufficient for Big Bang approach
- Entire upgrade can be completed in one session

**Testing Resources**:
- Development environment with .NET 10 SDK
- Access to test databases (SQL Server, SQLite)
- Azure subscription (for Azure integration testing)

---

## 10. Source Control Strategy

### 10.1 Big Bang Source Control Approach

Following the Big Bang strategy, all changes will be committed atomically:

**Branch Strategy**:
- ✅ **Upgrade Branch Created**: `upgrade-to-NET10` (created from `main`)
- **Source Branch**: `main`
- **Integration**: Create PR from `upgrade-to-NET10` to `main` after validation

### 10.2 Commit Strategy

**Recommended Commit Plan** (Single Atomic Commit):

**Commit 1: Atomic .NET 10 Upgrade** (after Phase 1 & 2 complete)
```
feat: upgrade solution to .NET 10.0

- Update all projects from net9.0 to net10.0
- Update Entity Framework Core packages to 10.0.0
- Update System.Text.Json to 10.0.0
- Update System.Drawing.Common to 10.0.0
- Update Microsoft.Extensions.Http to 10.0.0
- Update System.Formats.Asn1 to 10.0.0
- Update Microsoft.VisualStudio.Web.CodeGeneration.Design to 10.0.0-rc.1

All projects build successfully.
All unit tests pass.
Applications verified manually.

Breaking changes: None identified.

Projects upgraded:
- UISampleSpark.Core
- UISampleSpark.Data
- UISampleSpark.HttpClientFactory
- UISampleSpark.UI
- UISampleSpark.CLI
- UISampleSpark.Core.Tests
- UISampleSpark.Data.Tests
```

**Alternative: Separate Test Validation Commit** (if preferred):

If you prefer to separate the upgrade from test verification:

**Commit 1: Atomic .NET 10 Upgrade**
```
feat: upgrade solution to .NET 10.0

- Update all projects from net9.0 to net10.0
- Update all NuGet packages to version 10.0.0
- All projects build successfully with 0 errors, 0 warnings
```

**Commit 2: Test Validation**
```
test: verify .NET 10 upgrade

- All unit tests pass (Domain.Tests, Repository.Tests)
- Blazor application starts and functions correctly
- Console application verified
- No breaking changes identified
```

### 10.3 Commit Message Format

Use conventional commits format:
- `feat:` for new features or upgrades
- `fix:` if addressing issues discovered during upgrade
- `test:` for test-related changes
- `docs:` for documentation updates

### 10.4 Pull Request Strategy

**PR Title**: `Upgrade solution to .NET 10.0`

**PR Description Template**:
```markdown
## Description
Upgrades the entire UISampleSpark solution from .NET 9.0 to .NET 10.0 using the Big Bang migration strategy.

## Changes
- ✅ All 7 projects updated to net10.0
- ✅ 11 NuGet packages updated to version 10.0.0
- ✅ Solution builds with 0 errors, 0 warnings
- ✅ All unit tests pass
- ✅ Blazor application verified manually
- ✅ Console application verified

## Testing
- [x] Domain.Tests: All tests pass
- [x] Repository.Tests: All tests pass
- [x] Blazor application starts and renders correctly
- [x] Console application runs successfully
- [x] No security warnings

## Breaking Changes
None identified

## Migration Strategy
Big Bang - all projects upgraded simultaneously in single atomic operation

## Documentation
- Assessment report: `.github/upgrades/assessment.md`
- Migration plan: `.github/upgrades/plan.md`

## Checklist
- [x] Code builds successfully
- [x] Tests pass
- [x] No breaking changes
- [x] Documentation updated
```

### 10.5 Review and Merge Process

**Review Checklist**:
- [ ] All project files updated to net10.0
- [ ] All package references updated correctly
- [ ] No dependency version conflicts
- [ ] Build succeeds with no warnings
- [ ] All automated tests pass
- [ ] Manual testing completed
- [ ] Commit messages follow conventions
- [ ] PR description complete

**Merge Criteria**:
- All CI/CD checks pass (if configured)
- Code review approved
- All conversations resolved
- Branch is up to date with main

**Post-Merge**:
- Delete upgrade branch (optional)
- Tag release: `v10.0` or `net10-upgrade`
- Update documentation if needed

---

## 11. Success Criteria

### 11.1 Technical Success Criteria

- [x] **Preparation Complete**
  - [x] .NET 10 SDK installed
  - [x] Upgrade branch created (upgrade-to-NET10)
  - [x] Assessment completed

- [ ] **Atomic Upgrade Complete**
  - [ ] All 7 projects updated to net10.0
  - [ ] All 11 package updates applied
  - [ ] Zero security vulnerabilities in dependencies
  - [ ] Solution restores without errors
  - [ ] All projects build without errors
  - [ ] All projects build without warnings
  - [ ] No package dependency conflicts

- [ ] **Testing Complete**
  - [ ] UISampleSpark.Core.Tests: All tests pass
  - [ ] UISampleSpark.Data.Tests: All tests pass
  - [ ] UISampleSpark.UI: Application starts and core functionality verified
  - [ ] UISampleSpark.CLI: Runs successfully with expected output
  - [ ] Performance within acceptable thresholds (no degradation)

### 11.2 Big Bang Strategy Success Criteria

- [ ] **Single Atomic Operation**: All framework and package updates completed in one coordinated effort
- [ ] **No Intermediate States**: Solution successfully moved from net9.0 to net10.0 without partial states
- [ ] **Unified Testing**: Entire solution tested as a unit after upgrade
- [ ] **Fast Completion**: Upgrade completed within estimated 2-4 hour timeframe

### 11.3 Quality Criteria

- [ ] **Code Quality**
  - [ ] No new code smells introduced
  - [ ] No obsolete API usage warnings
  - [ ] Code style consistency maintained

- [ ] **Test Coverage**
  - [ ] Test coverage maintained (2 test projects with 28 test files)
  - [ ] No tests disabled or skipped
  - [ ] Test execution time acceptable

- [ ] **Documentation**
  - [ ] Migration plan documented (this file)
  - [ ] Assessment report generated
  - [ ] Commit messages clear and descriptive
  - [ ] PR description complete

### 11.4 Process Criteria

- [ ] **Source Control**
  - [ ] All changes committed to upgrade-to-NET10 branch
  - [ ] Commit messages follow conventional commits format
  - [ ] Atomic commit(s) following Big Bang strategy
  - [ ] PR created and ready for review

- [ ] **Validation**
  - [ ] Build verification completed
  - [ ] Unit tests executed and passed
  - [ ] Integration testing completed
  - [ ] Manual testing checklist completed

### 11.5 Deployment Readiness

- [ ] **Pre-Deployment**
  - [ ] Code review completed
  - [ ] All tests passing in CI/CD (if configured)
  - [ ] Performance benchmarks met
  - [ ] Security scan clean

- [ ] **Deployment**
  - [ ] Deployment plan reviewed
  - [ ] Rollback procedure documented
  - [ ] Monitoring strategy defined

---

## 12. Next Steps

### 12.1 Immediate Actions (Ready to Execute)

1. **Proceed to Execution Stage**: Once this plan is approved, transition to execution
2. **Execute Atomic Upgrade**: Update all project files and packages simultaneously
3. **Build and Validate**: Ensure solution builds with 0 errors
4. **Run Tests**: Execute all unit tests
5. **Manual Validation**: Test Blazor application and Console application
6. **Commit Changes**: Atomic commit following Big Bang strategy
7. **Create PR**: Submit for code review

### 12.2 Post-Upgrade Tasks

- **Performance Monitoring**: Monitor application performance in development
- **Documentation Updates**: Update README if needed
- **Team Communication**: Notify team of upgrade completion
- **CI/CD Updates**: Ensure build pipelines support .NET 10

### 12.3 Future Considerations

- **Package Updates**: Continue monitoring for package security updates
- **Framework Updates**: Plan for future .NET 11 upgrade (when available)
- **Feature Adoption**: Explore new .NET 10 features for future enhancements
- **Performance Optimization**: Leverage .NET 10 performance improvements

---

## 13. References

### 13.1 Official Microsoft Documentation

- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [Migrate from ASP.NET Core 9.0 to 10.0](https://learn.microsoft.com/en-us/aspnet/core/migration/90-to-100)
- [Entity Framework Core 10.0 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/)
- [Entity Framework Core 10.0 Breaking Changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes)
- [Blazor in .NET 10](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

### 13.2 Project Resources

- **Solution File**: `C:\GitHub\MarkHazleton\UISampleSpark\UISampleSpark.UI.sln`
- **Assessment Report**: `C:\GitHub\MarkHazleton\UISampleSpark\.github\upgrades\assessment.md`
- **Migration Plan**: `C:\GitHub\MarkHazleton\UISampleSpark\.github\upgrades\plan.md`
- **Upgrade Branch**: `upgrade-to-NET10`
- **Source Branch**: `main`

### 13.3 Package Documentation

- [Entity Framework Core Packages](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)
- [System.Drawing.Common](https://www.nuget.org/packages/System.Drawing.Common/)
- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/)

---

## Appendix A: Project File Locations

All project files to be modified:

1. `UISampleSpark.Core\UISampleSpark.Core.csproj`
2. `UISampleSpark.Data\UISampleSpark.Data.csproj`
3. `UISampleSpark.HttpClientFactory\UISampleSpark.HttpClientFactory.csproj`
4. `UISampleSpark.UI\UISampleSpark.UI.csproj`
5. `UISampleSpark.CLI\UISampleSpark.CLI.csproj`
6. `UISampleSpark.Core.Tests\UISampleSpark.Core.Tests.csproj`
7. `UISampleSpark.Data.Tests\UISampleSpark.Data.Tests.csproj`

---

## Appendix B: Quick Reference Commands

### Build Commands
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Build without restore
dotnet build --no-restore

# Clean solution
dotnet clean
```

### Test Commands
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test UISampleSpark.Core.Tests\UISampleSpark.Core.Tests.csproj
dotnet test UISampleSpark.Data.Tests\UISampleSpark.Data.Tests.csproj
```

### Run Commands
```bash
# Run Blazor application
dotnet run --project UISampleSpark.UI\UISampleSpark.UI.csproj

# Run Console application
dotnet run --project UISampleSpark.CLI\UISampleSpark.CLI.csproj
```

### Package Commands
```bash
# List packages
dotnet list package

# List outdated packages
dotnet list package --outdated

# List vulnerable packages
dotnet list package --vulnerable
```

---

**Plan Version**: 1.0  
**Created**: 2025-01-29  
**Strategy**: Big Bang  
**Target Framework**: .NET 10.0  
**Status**: Ready for Execution
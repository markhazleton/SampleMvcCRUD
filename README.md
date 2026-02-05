# SampleMvcCRUD

A .NET 10 (ASP.NET Core) application demonstrating multiple approaches to implementing a modern, maintainable CRUD (Create, Read, Update, Delete) user interface for Employee and Department management. This repository is a reference and educational resource that showcases patterns (MVC, Razor Pages, SPA-style), tooling, theming, and deployment techniques.

[![Docker Image](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/docker-image.yml/badge.svg)](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/docker-image.yml)
[![CodeQL](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/codeql-analysis.yml)

---
## Project History
- **2019** – Launched the repository, delivered the first MVC CRUD walkthrough with AJAX partials, and set the contributor/code-of-conduct foundation.
- **2020** – Experimented with React and Blazor front ends, standardized REST/SOAP client abstractions, and stood up Azure Pipelines plus App Service deployments.
- **2021** – Completed the .NET 5→6 modernization, layered in Bootstrap 5 theming, hardened CI/CD (GitHub Actions + Azure Pipelines), and increased automated tests.
- **2022-2023** – Adopted .NET 7 then .NET 8, added TreeView/PivotTable demos, refined guard clauses and validation, and expanded Docker/Azure Linux automation.
- **2024-2025** – Introduced the minimal API sample, Aspire experiments, Bootswatch theme switching, extensive workflow tuning, and the .NET 9 then .NET 10 migrations.
- **Tagging** – Milestones in the [CHANGELOG](CHANGELOG.md) include suggested tags (e.g., `net10-ga`, `ui-bootswatch-switcher`) you can apply in git for quick navigation.

See the [CHANGELOG](CHANGELOG.md) for the full milestone timeline, and open `reports/git-spark-report.html` to explore commit velocity visualized by Mark Hazleton's `git-spark` npm package.

---
## Live Deployments
- Windows IIS VM (.NET 9): https://samplecrud.markhazleton.com/
- Docker Hub Image: https://hub.docker.com/r/markhazleton/mwhsampleweb

---
## Goals
- Demonstrate multiple CRUD UI strategies
- Showcase clean architecture, DI, and unit testing
- Provide examples of theming and UX enhancements
- Show Azure deployment and containerization approaches

---
## Features & Architecture
- UI Patterns:
  - MVC Controllers & Views (traditional multi-view CRUD)
  - Razor Pages (page-centric CRUD)
  - Single Page style (AJAX + partial views + modals)
  - Pivot Table reporting (PivotTable.js)
- API Endpoints:
  - REST endpoints for Employees & Departments (Swagger/OpenAPI enabled)
- Theming & UX:
  - Bootswatch theme switcher (light/dark, instant swap)
  - Bootstrap 5 + Bootstrap Icons
  - Modal-based CRUD forms via jQuery/AJAX
- Data Layer:
  - In-memory EF Core database (EmployeeContext)
  - Repository/service pattern via `IEmployeeService` & `EmployeeDatabaseService`
- Observability & Health:
  - Application Insights telemetry
  - Health checks at `/health`
- DevOps & CI/CD:
  - GitHub Actions: build, test, Docker image push
  - Dockerfile for container builds
- Testing:
  - Domain & Repository unit test projects

---
## Technology Stack
- .NET 10 / C#
- ASP.NET Core MVC & Razor Pages
- Entity Framework Core (InMemory & SqlServer packages referenced for future use)
- Swashbuckle (Swagger/OpenAPI)
- Bootswatch theming (WebSpark.Bootswatch & WebSpark.HttpClientUtility)
- Markdown rendering (Westwind.AspNetCore.Markdown)

---
## HttpClient Usage
HttpClient is utilized via `WebSpark.HttpClientUtility` package which provides `IHttpRequestResultService` for making HTTP requests. This is actively used by `WebSpark.Bootswatch` to fetch Bootswatch theme data from CDN sources, enabling the dynamic theme switcher functionality. The base `AddHttpClient()` factory registration is also available for future external API integrations.

---
## Getting Started
### Prerequisites
- .NET 10 SDK
- (Optional) Docker

### Run Locally
```pwsh
git clone https://github.com/markhazleton/SampleMvcCRUD.git
cd SampleMvcCRUD
 dotnet restore
 cd Mwh.Sample.Web
 dotnet run
```
Visit https://localhost:5001

### Run with Docker
```pwsh
docker build -t mwhsampleweb ./Mwh.Sample.Web
docker run -p 8080:80 mwhsampleweb
```

---
## Project Structure
```
Mwh.Sample.Web/              Web application (MVC + Razor Pages + SPA-style CRUD)
Mwh.Sample.Domain/           Domain models (Employee/Department DTOs, responses)
Mwh.Sample.Repository/       EF Core context, service implementations, mock generator
Mwh.Sample.Domain.Tests/     Unit tests for domain logic
Mwh.Sample.Repository.Tests/ Unit tests for repository/service layer
Mwh.Sample.Console/          Console app seeding & demonstration (SQLite EF example)
```
(There is no HttpClientFactory sample project; previous reference removed.)

---
## Data Model
Employees belong to Departments. CRUD operations demonstrate validation, paging (`PagingParameterModel`), and simple relationships.

---
## Swagger/OpenAPI
Interactive API docs available at `/swagger` when running locally or deployed.

---
## Theme Switching
Implemented via Bootswatch services and `<bootswatch-theme-switcher />` integration.

---
## Health & Status
- `/health` returns liveness status
- `/status` provides application metadata

---
## Contributing
Contributions welcome. Submit issues or PRs.

---
## License
MIT License © 2018-2025 Mark Hazleton
See `LICENSE` for details.

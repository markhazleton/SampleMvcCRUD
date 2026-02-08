# UISampleSpark (Web Distribution Summary)

A .NET 10 ASP.NET Core application showcasing multiple CRUD UI strategies (MVC, Razor Pages, AJAX modal CRUD, and PivotTable reporting) for Employees and Departments.

## Key Features
- MVC, Razor Pages, and Single Page style (AJAX + partial views)
- PivotTable.js employee data analysis
- Swagger/OpenAPI at `/swagger`
- Bootswatch theme switching (light/dark)
- In-memory EF Core data store
- Health check at `/health`
- Optional Azure Key Vault integration (via `VaultUri` env var)

## Tech Stack
- .NET 10 / ASP.NET Core
- Entity Framework Core (InMemory)
- Swashbuckle (Swagger)
- Bootswatch (WebSpark.Bootswatch & WebSpark.HttpClientUtility)
- Markdown rendering (Westwind)

## HttpClient
HttpClient is used via `WebSpark.HttpClientUtility` to fetch Bootswatch themes from CDN sources for the dynamic theme switcher. Base `AddHttpClient()` is also registered for future external API use.

## Run Locally
```pwsh
dotnet restore
cd UISampleSpark.UI
dotnet run
```
Browse: https://localhost:5001

## License
MIT � 2018-2025 Mark Hazleton

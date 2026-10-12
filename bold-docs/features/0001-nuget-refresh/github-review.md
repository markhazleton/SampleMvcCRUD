# GitHub dependency review
Product Owner TL;DR: All 28 open PRs are dependency updates. Twenty-two NuGet PRs are already superseded by cf701b8; six remaining proposals are implemented together on main, including the EF tool manifest.

Reviewed each PR's changed-file patches; no application logic edits or open-PR inline review comments were found. Review applies heightened scrutiny to bot proposals without their own ratified specifications. Local main is ahead of origin/main by the validated NuGet commit, with no commits behind.

| PR | Reviewed outcome |
| --- | --- |
| 306 | DataTables core 3.1.3, synchronized with Bootstrap adapter. |
| 305 | DataTables Bootstrap adapter 3.1.3, synchronized with core. |
| 304 | SkiaSharp Linux 4.153.1 already in cf701b8. |
| 303 | SkiaSharp 4.153.1 already in cf701b8. |
| 302 | fs-extra 11.4.1 applied, client asset copying passes. |
| 298 | Moq 4.21.0 already in cf701b8. |
| 296 | coverlet 10.1.0 already in cf701b8. |
| 294 | MSTest framework proposal 4.4.1 superseded by 4.5.1. |
| 293 | MSTest adapter/framework 4.4.1 superseded by aligned 4.5.1. |
| 289 | Test SDK 18.10.1 already in cf701b8. |
| 288 | SkiaSharp Linux 4.152.0 superseded by 4.153.1. |
| 287 | SkiaSharp 4.152.0 superseded by 4.153.1. |
| 286 | EF SQLite 10.0.12 already in cf701b8. |
| 285 | Core EF pair 10.0.12 already aligned in cf701b8. |
| 284 | UI EF 10.0.12 already in cf701b8. |
| 275 | OTLP 1.18.0 superseded by 1.19.1. |
| 274 | OpenTelemetry API 1.18.0 superseded by 1.19.1. |
| 273 | OpenAPI 2.12.2 already in cf701b8; 3.x remains incompatible. |
| 272 | EF pair 10.0.11 superseded by aligned 10.0.12. |
| 267 | API description tooling 10.0.11 superseded by 10.0.12. |
| 266 | EF Tools package 10.0.11 superseded by 10.0.12. |
| 265 | SQL Server provider 10.0.11 superseded by 10.0.12. |
| 262 | ASP.NET OpenAPI 10.0.11 superseded by 10.0.12. |
| 260 | dotnet-ef manifest updated to 10.0.12 and executable verified. |
| 255 | ApiTestSpark 2.0.1 superseded by 2.1.0. |
| 249 | CodeQL 4.37.4 superseded by verified latest action release 4.38.3, consistent across workflows. |
| 244 | SQLite bundle 3.0.5 already in cf701b8. |
| 222 | setup-dotnet v6 applied to both workflows. |

Additional findings: Docker CI fails at Hadolint DL3066 (named user); fixed with explicit UID/GID 10001. Node build tooling moved to Node 24. Dependabot referenced deleted Data/Data.Tests projects; removed the former and replaced the latter with the actual UI.Tests project.

Validation: clean Release build 0 warnings/errors, 246 passing tests, npm ci/build and legacy jQuery DataTables DOM smoke check pass, EF tool restore/version pass, Hadolint with the workflow's existing documented exceptions passes, npm/NuGet audits report no vulnerabilities. GitHub open security alerts initially empty. Local Docker daemon unavailable; hosted image build pending.

## Resolution
All 28 originally open PRs were closed after their replacements were published on main (cf701b8, 8bacb50, 33cd3d0). Dependabot opened PR #307 during refresh: its Bootstrap Icons 1.13.2 patch was reviewed, applied with regenerated CSS/fonts, validated and published as 7d7c39e, then closed. Total: 29 reviewed and resolved PRs.

Final GitHub queries returned zero open PRs and zero open Dependabot security alerts. npm outdated returned no outdated direct npm dependencies. Hosted Test & Coverage run 38198905340 passed. CodeQL run 38198905343 passed setup and autobuild and is analyzing; Docker run 38198905367 passed lint and is building/pushing. Full hosted CodeQL/image results remain pending; local validations above passed. User explicitly requested main-only work; GitHub accepted direct pushes using the authenticated account's existing branch-rule bypass permission.

# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NET 9.0.

## Table of Contents

- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [Mwh.Sample.Console\Mwh.Sample.Console.csproj](#mwhsampleconsolemwhsampleconsolecsproj)
  - [Mwh.Sample.Domain.Tests\Mwh.Sample.Domain.Tests.csproj](#mwhsampledomaintestsmwhsampledomaintestscsproj)
  - [Mwh.Sample.Domain\Mwh.Sample.Domain.csproj](#mwhsampledomainmwhsampledomaincsproj)
  - [Mwh.Sample.HttpClientFactory\Mwh.Sample.HttpClientFactory.csproj](#mwhsamplehttpclientfactorymwhsamplehttpclientfactorycsproj)
  - [Mwh.Sample.Repository.Tests\Mwh.Sample.Repository.Tests.csproj](#mwhsamplerepositorytestsmwhsamplerepositorytestscsproj)
  - [Mwh.Sample.Repository\Mwh.Sample.Repository.csproj](#mwhsamplerepositorymwhsamplerepositorycsproj)
  - [Mwh.Sample.Web\Mwh.Sample.Web.csproj](#mwhsamplewebmwhsamplewebcsproj)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)


## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;Mwh.Sample.Web.csproj</b><br/><small>net9.0</small>"]
    P2["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
    P3["<b>📦&nbsp;Mwh.Sample.HttpClientFactory.csproj</b><br/><small>net9.0</small>"]
    P4["<b>📦&nbsp;Mwh.Sample.Repository.csproj</b><br/><small>net9.0</small>"]
    P5["<b>📦&nbsp;Mwh.Sample.Domain.Tests.csproj</b><br/><small>net9.0</small>"]
    P6["<b>📦&nbsp;Mwh.Sample.Repository.Tests.csproj</b><br/><small>net9.0</small>"]
    P7["<b>📦&nbsp;Mwh.Sample.Console.csproj</b><br/><small>net9.0</small>"]
    P1 --> P4
    P1 --> P2
    P3 --> P2
    P4 --> P2
    P5 --> P2
    P6 --> P4
    P7 --> P4
    P7 --> P2
    click P1 "#mwhsamplewebmwhsamplewebcsproj"
    click P2 "#mwhsampledomainmwhsampledomaincsproj"
    click P3 "#mwhsamplehttpclientfactorymwhsamplehttpclientfactorycsproj"
    click P4 "#mwhsamplerepositorymwhsamplerepositorycsproj"
    click P5 "#mwhsampledomaintestsmwhsampledomaintestscsproj"
    click P6 "#mwhsamplerepositorytestsmwhsamplerepositorytestscsproj"
    click P7 "#mwhsampleconsolemwhsampleconsolecsproj"

```

## Project Details

<a id="mwhsampleconsolemwhsampleconsolecsproj"></a>
### Mwh.Sample.Console\Mwh.Sample.Console.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 2
- **Dependants**: 0
- **Number of Files**: 3
- **Lines of Code**: 70

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Mwh.Sample.Console.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.Console.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsampleconsolemwhsampleconsolecsproj"
    end
    subgraph downstream["Dependencies (2"]
        P4["<b>📦&nbsp;Mwh.Sample.Repository.csproj</b><br/><small>net9.0</small>"]
        P2["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
        click P4 "#mwhsamplerepositorymwhsamplerepositorycsproj"
        click P2 "#mwhsampledomainmwhsampledomaincsproj"
    end
    MAIN --> P4
    MAIN --> P2

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| Bogus | Explicit | 35.6.3 |  | ✅Compatible |

<a id="mwhsampledomaintestsmwhsampledomaintestscsproj"></a>
### Mwh.Sample.Domain.Tests\Mwh.Sample.Domain.Tests.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 18
- **Lines of Code**: 2195

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Mwh.Sample.Domain.Tests.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.Domain.Tests.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsampledomaintestsmwhsampledomaintestscsproj"
    end
    subgraph downstream["Dependencies (1"]
        P2["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
        click P2 "#mwhsampledomainmwhsampledomaincsproj"
    end
    MAIN --> P2

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| coverlet.collector | Explicit | 6.0.4 |  | ✅Compatible |
| Microsoft.NET.Test.Sdk | Explicit | 17.14.1 |  | ✅Compatible |
| MSTest.TestAdapter | Explicit | 3.10.4 |  | ✅Compatible |
| MSTest.TestFramework | Explicit | 3.10.4 |  | ✅Compatible |

<a id="mwhsampledomainmwhsampledomaincsproj"></a>
### Mwh.Sample.Domain\Mwh.Sample.Domain.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 5
- **Number of Files**: 27
- **Lines of Code**: 1567

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (5)"]
        P1["<b>📦&nbsp;Mwh.Sample.Web.csproj</b><br/><small>net9.0</small>"]
        P3["<b>📦&nbsp;Mwh.Sample.HttpClientFactory.csproj</b><br/><small>net9.0</small>"]
        P4["<b>📦&nbsp;Mwh.Sample.Repository.csproj</b><br/><small>net9.0</small>"]
        P5["<b>📦&nbsp;Mwh.Sample.Domain.Tests.csproj</b><br/><small>net9.0</small>"]
        P7["<b>📦&nbsp;Mwh.Sample.Console.csproj</b><br/><small>net9.0</small>"]
        click P1 "#mwhsamplewebmwhsamplewebcsproj"
        click P3 "#mwhsamplehttpclientfactorymwhsamplehttpclientfactorycsproj"
        click P4 "#mwhsamplerepositorymwhsamplerepositorycsproj"
        click P5 "#mwhsampledomaintestsmwhsampledomaintestscsproj"
        click P7 "#mwhsampleconsolemwhsampleconsolecsproj"
    end
    subgraph current["Mwh.Sample.Domain.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsampledomainmwhsampledomaincsproj"
    end
    P1 --> MAIN
    P3 --> MAIN
    P4 --> MAIN
    P5 --> MAIN
    P7 --> MAIN

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| System.Drawing.Common | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |

<a id="mwhsamplehttpclientfactorymwhsamplehttpclientfactorycsproj"></a>
### Mwh.Sample.HttpClientFactory\Mwh.Sample.HttpClientFactory.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 4
- **Lines of Code**: 263

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Mwh.Sample.HttpClientFactory.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.HttpClientFactory.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsamplehttpclientfactorymwhsamplehttpclientfactorycsproj"
    end
    subgraph downstream["Dependencies (1"]
        P2["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
        click P2 "#mwhsampledomainmwhsampledomaincsproj"
    end
    MAIN --> P2

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| Microsoft.Extensions.Http | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| System.Text.Json | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |

<a id="mwhsamplerepositorytestsmwhsamplerepositorytestscsproj"></a>
### Mwh.Sample.Repository.Tests\Mwh.Sample.Repository.Tests.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 10
- **Lines of Code**: 1188

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Mwh.Sample.Repository.Tests.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.Repository.Tests.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsamplerepositorytestsmwhsamplerepositorytestscsproj"
    end
    subgraph downstream["Dependencies (1"]
        P4["<b>📦&nbsp;Mwh.Sample.Repository.csproj</b><br/><small>net9.0</small>"]
        click P4 "#mwhsamplerepositorymwhsamplerepositorycsproj"
    end
    MAIN --> P4

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| coverlet.collector | Explicit | 6.0.4 |  | ✅Compatible |
| Microsoft.NET.Test.Sdk | Explicit | 17.14.1 |  | ✅Compatible |
| Moq | Explicit | 4.20.72 |  | ✅Compatible |
| MSTest.TestAdapter | Explicit | 3.10.4 |  | ✅Compatible |
| MSTest.TestFramework | Explicit | 3.10.4 |  | ✅Compatible |

<a id="mwhsamplerepositorymwhsamplerepositorycsproj"></a>
### Mwh.Sample.Repository\Mwh.Sample.Repository.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 3
- **Number of Files**: 12
- **Lines of Code**: 1014

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (3)"]
        P1["<b>📦&nbsp;Mwh.Sample.Web.csproj</b><br/><small>net9.0</small>"]
        P6["<b>📦&nbsp;Mwh.Sample.Repository.Tests.csproj</b><br/><small>net9.0</small>"]
        P7["<b>📦&nbsp;Mwh.Sample.Console.csproj</b><br/><small>net9.0</small>"]
        click P1 "#mwhsamplewebmwhsamplewebcsproj"
        click P6 "#mwhsamplerepositorytestsmwhsamplerepositorytestscsproj"
        click P7 "#mwhsampleconsolemwhsampleconsolecsproj"
    end
    subgraph current["Mwh.Sample.Repository.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.Repository.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsamplerepositorymwhsamplerepositorycsproj"
    end
    subgraph downstream["Dependencies (1"]
        P2["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
        click P2 "#mwhsampledomainmwhsampledomaincsproj"
    end
    P1 --> MAIN
    P6 --> MAIN
    P7 --> MAIN
    MAIN --> P2

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| Bogus | Explicit | 35.6.3 |  | ✅Compatible |
| Microsoft.EntityFrameworkCore | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.InMemory | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Sqlite | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |

<a id="mwhsamplewebmwhsamplewebcsproj"></a>
### Mwh.Sample.Web\Mwh.Sample.Web.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 2
- **Dependants**: 0
- **Number of Files**: 91
- **Lines of Code**: 4883

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Mwh.Sample.Web.csproj"]
        MAIN["<b>📦&nbsp;Mwh.Sample.Web.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#mwhsamplewebmwhsamplewebcsproj"
    end
    subgraph downstream["Dependencies (2"]
        P4["<b>📦&nbsp;Mwh.Sample.Repository.csproj</b><br/><small>net9.0</small>"]
        P2["<b>📦&nbsp;Mwh.Sample.Domain.csproj</b><br/><small>net9.0</small>"]
        click P4 "#mwhsamplerepositorymwhsamplerepositorycsproj"
        click P2 "#mwhsampledomainmwhsampledomaincsproj"
    end
    MAIN --> P4
    MAIN --> P2

```

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| Azure.Extensions.AspNetCore.Configuration.Secrets | Explicit | 1.4.0 |  | ✅Compatible |
| Azure.Identity | Explicit | 1.15.0 |  | ✅Compatible |
| Microsoft.ApplicationInsights | Explicit | 2.23.0 |  | ✅Compatible |
| Microsoft.ApplicationInsights.AspNetCore | Explicit | 2.23.0 |  | ✅Compatible |
| Microsoft.EntityFrameworkCore | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.InMemory | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.SqlServer | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Tools | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | Explicit | 9.0.0 | 10.0.0-rc.1.25458.5 | NuGet package upgrade is recommended |
| Swashbuckle.AspNetCore | Explicit | 9.0.4 |  | ✅Compatible |
| System.Formats.Asn1 | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| System.Text.Json | Explicit | 9.0.8 | 10.0.0 | NuGet package upgrade is recommended |
| WebSpark.Bootswatch | Explicit | 1.20.1 |  | ✅Compatible |
| WebSpark.HttpClientUtility | Explicit | 1.1.0 |  | ✅Compatible |
| Westwind.AspNetCore.Markdown | Explicit | 3.24.0 |  | ✅Compatible |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| Azure.Extensions.AspNetCore.Configuration.Secrets | 1.4.0 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| Azure.Identity | 1.15.0 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| Bogus | 35.6.3 |  | [Mwh.Sample.Console.csproj](#mwhsampleconsolecsproj)<br/>[Mwh.Sample.Repository.csproj](#mwhsamplerepositorycsproj) | ✅Compatible |
| coverlet.collector | 6.0.4 |  | [Mwh.Sample.Domain.Tests.csproj](#mwhsampledomaintestscsproj)<br/>[Mwh.Sample.Repository.Tests.csproj](#mwhsamplerepositorytestscsproj) | ✅Compatible |
| Microsoft.ApplicationInsights | 2.23.0 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| Microsoft.ApplicationInsights.AspNetCore | 2.23.0 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| Microsoft.EntityFrameworkCore | 9.0.8 | 10.0.0 | [Mwh.Sample.Repository.csproj](#mwhsamplerepositorycsproj)<br/>[Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.8 | 10.0.0 | [Mwh.Sample.Repository.csproj](#mwhsamplerepositorycsproj)<br/>[Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Sqlite | 9.0.8 | 10.0.0 | [Mwh.Sample.Repository.csproj](#mwhsamplerepositorycsproj) | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.8 | 10.0.0 | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| Microsoft.EntityFrameworkCore.Tools | 9.0.8 | 10.0.0 | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| Microsoft.Extensions.Http | 9.0.8 | 10.0.0 | [Mwh.Sample.HttpClientFactory.csproj](#mwhsamplehttpclientfactorycsproj) | NuGet package upgrade is recommended |
| Microsoft.NET.Test.Sdk | 17.14.1 |  | [Mwh.Sample.Domain.Tests.csproj](#mwhsampledomaintestscsproj)<br/>[Mwh.Sample.Repository.Tests.csproj](#mwhsamplerepositorytestscsproj) | ✅Compatible |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 9.0.0 | 10.0.0-rc.1.25458.5 | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| Moq | 4.20.72 |  | [Mwh.Sample.Repository.Tests.csproj](#mwhsamplerepositorytestscsproj) | ✅Compatible |
| MSTest.TestAdapter | 3.10.4 |  | [Mwh.Sample.Domain.Tests.csproj](#mwhsampledomaintestscsproj)<br/>[Mwh.Sample.Repository.Tests.csproj](#mwhsamplerepositorytestscsproj) | ✅Compatible |
| MSTest.TestFramework | 3.10.4 |  | [Mwh.Sample.Domain.Tests.csproj](#mwhsampledomaintestscsproj)<br/>[Mwh.Sample.Repository.Tests.csproj](#mwhsamplerepositorytestscsproj) | ✅Compatible |
| Swashbuckle.AspNetCore | 9.0.4 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| System.Drawing.Common | 9.0.8 | 10.0.0 | [Mwh.Sample.Domain.csproj](#mwhsampledomaincsproj) | NuGet package upgrade is recommended |
| System.Formats.Asn1 | 9.0.8 | 10.0.0 | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| System.Text.Json | 9.0.8 | 10.0.0 | [Mwh.Sample.HttpClientFactory.csproj](#mwhsamplehttpclientfactorycsproj)<br/>[Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | NuGet package upgrade is recommended |
| WebSpark.Bootswatch | 1.20.1 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| WebSpark.HttpClientUtility | 1.1.0 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |
| Westwind.AspNetCore.Markdown | 3.24.0 |  | [Mwh.Sample.Web.csproj](#mwhsamplewebcsproj) | ✅Compatible |


# SampleMvcCRUD

An ASP.NET Core MVC application demonstrating multiple approaches to implementing a modern, maintainable CRUD (Create, Read, Update, Delete) user interface. This repository is designed as a reference and educational resource for developers interested in best practices for web application architecture, theming, API design, and deployment.

[![.NET](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/main_mwhsampleweb.yml/badge.svg)](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/main_mwhsampleweb.yml)
[![Docker Image](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/docker-image.yml/badge.svg)](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/docker-image.yml)

---

## 🌐 Live Demo & Deployments

- **Windows IIS VM (.NET 9):** [samplecrud.markhazleton.com](https://samplecrud.markhazleton.com/)
- **Azure App Service (.NET 9, Linux, GitHub Actions):** [mwhsampleweb.azurewebsites.net](https://mwhsampleweb.azurewebsites.net/)
- **Azure App Service (Docker Image):** [samplecrud.azurewebsites.net](https://samplecrud.azurewebsites.net/)
- **Docker Hub Image:** [markhazleton/mwhsampleweb](https://hub.docker.com/r/markhazleton/mwhsampleweb)

---

## 🚀 Project Goals

- Demonstrate multiple ways to build CRUD UIs in ASP.NET Core MVC
- Showcase best practices for architecture, testing, and deployment
- Provide a reference for theming, API design, and modern web techniques
- Enable easy customization and extension for your own projects

---

## 🏗️ Features & Architecture

- **Multiple CRUD Implementations:**
  - Classic MVC Controllers & Views
  - Razor Pages
  - Single Page (JavaScript-driven) UI
  - Pivot Table integration (PivotTable.js)
- **API-First Design:**
  - RESTful endpoints for Employees and Departments
  - Swagger/OpenAPI documentation ([API Docs](/swagger/))
- **Modern UI/UX:**
  - Responsive Bootstrap 5 layout
  - [WebSpark.Bootswatch](https://www.nuget.org/packages/WebSpark.Bootswatch/) theme switcher (light/dark, instant theme change)
  - Bootstrap Icons
  - DataTables integration for advanced tables
- **Security & Configuration:**
  - Azure Key Vault integration for secrets
  - App Insights telemetry
  - Health checks endpoint (`/health`)
- **DevOps & CI/CD:**
  - GitHub Actions for build, test, Docker, and Azure deployment
  - Azure DevOps pipeline example
  - Dockerfile for containerized builds
- **Testing:**
  - Unit tests for domain and repository layers
- **Extensible Architecture:**
  - Clean separation of Domain, Repository, and Web projects
  - Dependency Injection throughout
  - Example HttpClientFactory usage

---

## 🖌️ Theme Switcher (WebSpark.Bootswatch)

This app features a dynamic theme switcher using [WebSpark.Bootswatch](https://www.nuget.org/packages/WebSpark.Bootswatch/):

- Instantly change the site's look with any [Bootswatch](https://bootswatch.com/) theme
- Light/dark mode support
- User preferences are saved in the browser
- Implemented via `<bootswatch-theme-switcher />` tag helper and JavaScript

---

## 🏁 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- (Optional) [Docker](https://www.docker.com/)

### Run Locally

```pwsh
# Clone the repository
git clone https://github.com/markhazleton/SampleMvcCRUD.git
cd SampleMvcCRUD

# Restore and run the web app
dotnet restore
cd Mwh.Sample.Web
dotnet run
```

Visit [https://localhost:5001](https://localhost:5001) in your browser.

### Run with Docker

```pwsh
docker build -t mwhsampleweb ./Mwh.Sample.Web
docker run -p 8080:80 mwhsampleweb
```

---

## 🧩 Project Structure

- `Mwh.Sample.Web/` - Main ASP.NET Core MVC web app
- `Mwh.Sample.Domain/` - Domain models and interfaces
- `Mwh.Sample.Repository/` - Data access and repository pattern
- `Mwh.Sample.HttpClientFactory/` - HttpClient usage examples
- `SampleMinimalApi/` - Minimal API example
- `Mwh.Sample.Domain.Tests/`, `Mwh.Sample.Repository.Tests/` - Unit tests

---

## 📚 Key Techniques Demonstrated

- **MVC, Razor Pages, and SPA patterns in one solution**
- **API-first development with Swagger/OpenAPI**
- **Modern Bootstrap theming and instant theme switching**
- **Azure Key Vault for secure configuration**
- **Health checks and Application Insights**
- **CI/CD with GitHub Actions and Azure DevOps**
- **Docker containerization**
- **Extensible, testable architecture**

---

## 📝 Customization & Contribution

SampleMvcCRUD is open source and welcomes contributions!

- **Issues:** [File an issue](https://github.com/markhazleton/SampleMvcCRUD/issues)
- **Pull Requests:** Fork, branch, and submit your improvements
- **Documentation:** Help improve this README or add more docs
- **Feature Ideas:** React/Vue/Mobile UI contributions are welcome

See [CONTRIBUTING.md](CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

---

## 👤 About the Author

**Mark Hazleton** is a solutions architect and lifelong learner with a passion for building technology that delivers real business value. With extensive experience in both on-premises and cloud-based solutions, Mark has worked with organizations of all sizes—from small business websites to large enterprise projects. He is dedicated to pragmatic, outcome-focused software development, and is an advocate for clear communication, agile practices, and continuous learning.

- [GitHub Profile](https://github.com/markhazleton)
- [LinkedIn](https://www.linkedin.com/in/markhazleton)
- [Personal Website](https://markhazleton.com)

*For all questions, contributions, and support, please use the [GitHub repository issues](https://github.com/markhazleton/SampleMvcCRUD/issues) and pull requests. Community participation is welcome and encouraged!*

---

## 🙏 Acknowledgements

Thanks to the open source community, teachers, and developers who share their knowledge and code.

---

## ⚖️ License

Copyright 2018-2025 Mark Hazleton  
Code released under the MIT License. See [LICENSE](LICENSE).

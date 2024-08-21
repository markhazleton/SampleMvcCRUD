# SampleMvcCRUD

A .NET Application designed to demonstrate multiple approaches for implementing a simple maintenance (CRUD) user interface for the web.

For deployments, there are GitHub Actions with Continuous Integration/Continuous Deployment (CI/CD) pipelines targeting Azure App Services and Docker containers.

[![.NET](https://github.com/markhazleton/samplemvccrud/actions/workflows/main_samplecrud.yml/badge.svg)]([main_mwhsampleweb.yml](https://github.com/markhazleton/SampleMvcCRUD/blob/main/.github/workflows/main_samplecrud.yml))

[![.NET](https://github.com/markhazleton/samplemvccrud/actions/workflows/docker-image.yml/badge.svg)]([docker-image.yml](https://github.com/markhazleton/SampleMvcCRUD/blob/main/.github/workflows/docker-image.yml))

https://github.com/markhazleton/SampleMvcCRUD/blob/main/.github/workflows/docker-image.yml

## Docker

- The Docker image is available on Docker Hub: [markhazleton/mwhsampleweb](https://hub.docker.com/repository/docker/markhazleton/mwhsampleweb). A GitHub Action is configured to create an updated image each time the main branch is updated.

## Hosting

The .NET 8 Web application is hosted on:

- **Microsoft Azure Virtual Machine (VM)** running IIS [https://samplecrud.markhazleton.com/](https://samplecrud.markhazleton.com/)
- **Microsoft Azure App Service**, Linux, using GitHub Actions CI/CD [https://samplecrud.azurewebsites.net/](https://samplecrud.azurewebsites.net/)

Source code is maintained on GitHub: [SampleMvcCRUD](https://github.com/markhazleton/SampleMvcCRUD).

## Usage

This application demonstrates various techniques in the ASP.NET MVC Framework for common maintenance activities (CRUD: Create, Read, Update, Delete). It is a project to showcase the integration between Azure and GitHub for a web application.

### Topics Covered:

1. [Azure DevOps](/Sample-CRUD-Application/Azure-DevOps-Pipelines) - CI/CD with Azure Pipelines.
2. GitHub Actions for building and deploying to Azure App Service.
3. [Application Development Approach](https://dev.azure.com/markhazleton/SampleMvcCRUD/_wiki/wikis/SampleMvcCRUD.wiki/24/Application-Approach)
4. Integration of Azure DevOps and GitHub Repository.
5. Azure Application Insights for monitoring Azure App Services.
6. Unit Testing as part of the CI Pipeline.
7. [Code Rush - Continuous Code Quality Analysis](https://dev.azure.com/markhazleton/SampleMvcCRUD/_wiki/wikis/SampleMvcCRUD.wiki/22/Code-Rush-Continuous-Code-Quality-Analysis)
8. [Git Flow Branching Strategy](https://dev.azure.com/markhazleton/SampleMvcCRUD/_wiki/wikis/SampleMvcCRUD.wiki/11/Branching-Strategy)
9. Swagger and OpenAPI for documentation.
10. Docker for containerization and deployment under Linux.

## Microsoft .NET Versions

1. .NET Standard for common projects.
2. .NET 8.0 Web Application.

## Author

[Mark Hazleton](https://markhazleton.com)

## Copyright and License

Copyright 2018-2024 [Mark Hazleton](https://markhazleton.com)

Code released under the MIT License.

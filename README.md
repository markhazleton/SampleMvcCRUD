# SampleMvcCRUD
An Asp.Net MVC Application to demonstrate multiple ways of implementing a simple maintenance (CRUD) user interface

[![.NET](https://github.com/markhazleton/samplemvccrud/actions/workflows/main_mwhsampleweb.yml/badge.svg)]([main_mwhsampleweb.yml](https://github.com/markhazleton/SampleMvcCRUD/blob/main/.github/workflows/main_mwhsampleweb.yml))

[![.NET](https://github.com/markhazleton/samplemvccrud/actions/workflows/docker-image.yml/badge.svg)]([docker-image.yml](https://github.com/markhazleton/SampleMvcCRUD/blob/main/.github/workflows/docker-image.yml))



https://github.com/markhazleton/SampleMvcCRUD/blob/main/.github/workflows/docker-image.yml


For deployments there are multiple approaches:

1)  *current* [GitHub Action](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/main_mwhsampleweb.yml) with Continuous Integration/Continuous Deployment (CI/CD) to Docker Hub and Azure App Services 
1)  *current* [GitHub Action](https://github.com/markhazleton/SampleMvcCRUD/actions/workflows/docker-image.yml) to push Docker Image to Docker Hub, then have Azure App Service configured to pull latest version from Docker Hub. 
1)  [Azure DevOps project](https://dev.azure.com/markhazleton/SampleMvcCRUD) with Continuous Integration/Continuous Deployment (CI/CD) pipelines to Azure App Services 

Web application is hosted:
- Amazon Web Services (AWS)  Virtual Machine - Windows 2012 IIS - Net 6 [markhazletonsamplecrud.controlorigins.com](https://markhazletonsamplecrud.controlorigins.com/)
- Microsoft Azure AppService .Net 7 Linux Deployed via GitHub Actions [mwhsampleweb.azurewebsites.net](https://mwhsampleweb.azurewebsites.net/) 
- Microsoft Azure AppService .Net 7 Linux Image from docker hub [samplemvccrud.azurewebsites.net](https://samplemvccrud.azurewebsites.net/) 
- Docker Hub Image [markhazleton/mwhsampleweb](https://hub.docker.com/r/markhazleton/mwhsampleweb)

## Customization

SampleMvcCrud is open source and you’re encouraged to contribute.

## Contributing

You can contribute in several ways.
- **Issues:** Provide a detailed report of any bugs you encounter and open an issue on [GitHub](https://github.com/markhazleton/SampleMvcCrud/issues).
- **Documentation:** If you'd like to fix a typo or beef up the docs, you can fork the project, make your changes, and submit a pull request.
- **Code:** Make a fix and submit it as a pull request. 
- **Platform:**  Right now this has a MVC Web solution, I would love a React/Vue or Mobile version, but my skills are not there yet. 

## Author

Mark Hazleton
+ https://markhazleton.controlorigins.com 

## Thanks
To the many teachers and developers that post sample code under open source license.

## Copyright and License
Copyright 2018-2023 Mark Hazleton
Code released under the MIT License.



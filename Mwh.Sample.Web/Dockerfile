# Use the .NET 9.0 ASP.NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 9.0 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the NuGet configuration and project files
COPY ["nuget.config", "."]
COPY ["Mwh.Sample.Web/Mwh.Sample.Web.csproj", "Mwh.Sample.Web/"]
COPY ["Mwh.Sample.Repository/Mwh.Sample.Repository.csproj", "Mwh.Sample.Repository/"]
COPY ["Mwh.Sample.Domain/Mwh.Sample.Domain.csproj", "Mwh.Sample.Domain/"]
COPY ["Mwh.Sample.HttpClientFactory/Mwh.Sample.HttpClientFactory.csproj", "Mwh.Sample.HttpClientFactory/"]

# Restore dependencies
RUN dotnet restore "Mwh.Sample.Web/Mwh.Sample.Web.csproj"

# Copy all the source code
COPY . .

# Set the working directory to the web project and build the project in release mode
WORKDIR "/src/Mwh.Sample.Web"
RUN dotnet build "Mwh.Sample.Web.csproj" -c Release -o /app/build

# Publish the application to the publish directory
FROM build AS publish
RUN dotnet publish "Mwh.Sample.Web.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mwh.Sample.Web.dll"]

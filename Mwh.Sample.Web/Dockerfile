# Pin the .NET 10 runtime/SDK (nightly until GA) so restore supports net10.0
ARG DOTNET_VERSION=10.0

# Use the .NET 10 ASP.NET runtime as the base image
FROM mcr.microsoft.com/dotnet/nightly/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 10 SDK image for building the application
FROM mcr.microsoft.com/dotnet/nightly/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Set shell options for better error handling
SHELL ["/bin/bash", "-o", "pipefail", "-c"]

# Install Node.js and npm
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y --no-install-recommends nodejs && \
    npm install -g npm@latest && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy solution and nuget configuration
COPY ["Mwh.Sample.Web.sln", "./"]
COPY ["nuget.config", "./"]

# Copy all project files - ensuring proper directory structure
COPY ["Mwh.Sample.Web/*.csproj", "Mwh.Sample.Web/"]
COPY ["Mwh.Sample.Repository/*.csproj", "Mwh.Sample.Repository/"]
COPY ["Mwh.Sample.Domain/*.csproj", "Mwh.Sample.Domain/"]
COPY ["Mwh.Sample.Console/*.csproj", "Mwh.Sample.Console/"]
COPY ["Mwh.Sample.Domain.Tests/*.csproj", "Mwh.Sample.Domain.Tests/"]
COPY ["Mwh.Sample.Repository.Tests/*.csproj", "Mwh.Sample.Repository.Tests/"]

# Restore dependencies
RUN dotnet restore "Mwh.Sample.Web/Mwh.Sample.Web.csproj"

# Copy all the source code
COPY . .

# Set the working directory to the web project and build the project in release mode
WORKDIR "/src/Mwh.Sample.Web"
RUN dotnet build "Mwh.Sample.Web.csproj" -c Release -o /app/build

# Publish the application to the publish directory
FROM build AS publish
RUN dotnet publish "Mwh.Sample.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mwh.Sample.Web.dll"]

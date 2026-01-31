# Pin the .NET 10 runtime/SDK (nightly until GA) so restore supports net10.0
ARG DOTNET_VERSION=10.0

# Use the .NET 10 ASP.NET runtime as the base image (Alpine for security)
FROM mcr.microsoft.com/dotnet/nightly/aspnet:${DOTNET_VERSION}-alpine AS base

# Update Alpine packages to get latest security patches
RUN apk update && apk upgrade --no-cache && \
    # Install ICU for globalization support
    apk add --no-cache icu-libs icu-data-full && \
    # Remove unnecessary packages and clean up
    rm -rf /var/cache/apk/*

# Create non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
WORKDIR /app
EXPOSE 8080

# Use the .NET 10 SDK image for building the application
FROM mcr.microsoft.com/dotnet/nightly/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Set shell options for better error handling
SHELL ["/bin/bash", "-o", "pipefail", "-c"]

# Update OS packages to latest security patches
RUN apt-get update && \
    apt-get upgrade -y && \
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

# Copy published application
COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user for security
USER appuser

# Configure ASP.NET Core for container environment
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Mwh.Sample.Web.dll"]

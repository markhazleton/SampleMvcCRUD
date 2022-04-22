#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Mwh.Sample.Web/Mwh.Sample.Web.csproj", "Mwh.Sample.Web/"]
COPY ["Mwh.Sample.Repository/Mwh.Sample.Repository.csproj", "Mwh.Sample.Repository/"]
COPY ["Mwh.Sample.Domain/Mwh.Sample.Domain.csproj", "Mwh.Sample.Domain/"]
COPY ["Mwh.Sample.SwaggerCore/Mwh.Sample.SwaggerCore.csproj", "Mwh.Sample.SwaggerCore/"]
COPY ["Mwh.Sample.HttpClientFactory/Mwh.Sample.HttpClientFactory.csproj", "Mwh.Sample.HttpClientFactory/"]
RUN dotnet restore "Mwh.Sample.Web/Mwh.Sample.Web.csproj"
COPY . .
WORKDIR "/src/Mwh.Sample.Web"
RUN dotnet build "Mwh.Sample.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mwh.Sample.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mwh.Sample.Web.dll"]
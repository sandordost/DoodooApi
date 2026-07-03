# Multi-project build. Build context is the repository root so the Api host and the
# module/shared class libraries it references are all available.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
# Npgsql loads the Kerberos/GSSAPI library during connection startup; it is not present
# in the slim aspnet runtime image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore Api/DoodooApi.csproj
RUN dotnet publish Api/DoodooApi.csproj -c $BUILD_CONFIGURATION -o /app/api /p:UseAppHost=false
RUN dotnet restore migration/Doodoo.MigrationService/Doodoo.MigrationService.csproj
RUN dotnet publish migration/Doodoo.MigrationService/Doodoo.MigrationService.csproj -c $BUILD_CONFIGURATION -o /app/migrator /p:UseAppHost=false

# ---------- Migrator (run-once) ----------
FROM base AS migrator
WORKDIR /app
COPY --from=build /app/migrator .
ENTRYPOINT ["dotnet", "Doodoo.MigrationService.dll"]

# ---------- API (default/last stage, so an untargeted build produces the app) ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/api .
ENTRYPOINT ["dotnet", "DoodooApi.dll"]

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

# ---------- Restore (cached layer: only re-runs when a .csproj changes) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project files first so `dotnet restore` is cached independently of source edits.
COPY Api/DoodooApi.csproj Api/
COPY migration/Doodoo.MigrationService/Doodoo.MigrationService.csproj migration/Doodoo.MigrationService/
COPY shared/Doodoo.SharedKernel/Doodoo.SharedKernel.csproj shared/Doodoo.SharedKernel/
COPY shared/Doodoo.Messaging.Contracts/Doodoo.Messaging.Contracts.csproj shared/Doodoo.Messaging.Contracts/
COPY aspire/Doodoo.ServiceDefaults/Doodoo.ServiceDefaults.csproj aspire/Doodoo.ServiceDefaults/
COPY modules/Doodoo.Modules.Users/Doodoo.Modules.Users.csproj modules/Doodoo.Modules.Users/
COPY modules/Doodoo.Modules.Currency/Doodoo.Modules.Currency.csproj modules/Doodoo.Modules.Currency/
COPY modules/Doodoo.Modules.Rewards/Doodoo.Modules.Rewards.csproj modules/Doodoo.Modules.Rewards/
COPY modules/Doodoo.Modules.Todos/Doodoo.Modules.Todos.csproj modules/Doodoo.Modules.Todos/
COPY modules/Doodoo.Modules.Inventory/Doodoo.Modules.Inventory.csproj modules/Doodoo.Modules.Inventory/

RUN dotnet restore Api/DoodooApi.csproj
RUN dotnet restore migration/Doodoo.MigrationService/Doodoo.MigrationService.csproj

# Now bring in the rest of the source and publish (restore already done).
COPY . .
RUN dotnet publish Api/DoodooApi.csproj -c $BUILD_CONFIGURATION -o /app/api --no-restore /p:UseAppHost=false
RUN dotnet publish migration/Doodoo.MigrationService/Doodoo.MigrationService.csproj -c $BUILD_CONFIGURATION -o /app/migrator --no-restore /p:UseAppHost=false

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

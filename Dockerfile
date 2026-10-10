# =============================================================================
# CoffeeNChill Functions — Azure Functions (isolated worker) container
# Multi-stage build: SDK stage compiles, runtime stage is leaner
# =============================================================================

# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as a distinct layer (better Docker caching)
COPY ["CoffeeNChill.Functions.csproj", "./"]
RUN dotnet restore "CoffeeNChill.Functions.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "CoffeeNChill.Functions.csproj" \
        -c Release \
        -o /app/publish \
        /p:UseAppHost=false

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS runtime
WORKDIR /home/site/wwwroot

# Azure Functions runtime expects these environment variables
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    FUNCTIONS_WORKER_RUNTIME=dotnet-isolated \
    ASPNETCORE_URLS=http://+:80

# Copy published output from build stage
COPY --from=build /app/publish .

EXPOSE 80
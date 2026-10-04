# ==============================================================================
# Multi-Stage Dockerfile for Survey System Project (.NET 8 Web API)
# Optimized for production, layer caching, security, and minimal image size.
# ==============================================================================

# ------------------------------------------------------------------------------
# Stage 1: Base Runtime Image
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Configure ASP.NET Core default environment for containers
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_ENVIRONMENT=Production

# ------------------------------------------------------------------------------
# Stage 2: SDK Build Stage
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first to maximize Docker layer caching
COPY ["API Layer/API Layer.csproj", "API Layer/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure Layer/Infrastructure Layer.csproj", "Infrastructure Layer/"]
COPY ["Application.UnitTests/Application.UnitTests.csproj", "Application.UnitTests/"]
COPY ["Domain.UnitTests/Domain.UnitTests.csproj", "Domain.UnitTests/"]

# Restore project dependencies
RUN dotnet restore "API Layer/API Layer.csproj"

# Copy the entire source code tree
COPY . .

# Build API project
WORKDIR "/src/API Layer"
RUN dotnet build "API Layer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ------------------------------------------------------------------------------
# Stage 3: Publish Stage
# ------------------------------------------------------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API Layer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ------------------------------------------------------------------------------
# Stage 4: Final Production Runtime
# ------------------------------------------------------------------------------
FROM base AS final
WORKDIR /app

# Create directory for Serilog file sink logs
RUN mkdir -p /app/Logs

# Copy published application binaries from publish stage
COPY --from=publish /app/publish .

# Set container entry point
ENTRYPOINT ["dotnet", "API Layer.dll"]

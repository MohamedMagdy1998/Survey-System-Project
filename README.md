# Survey System API - Docker, Multi-Environment Compose & CI/CD

Enterprise-grade survey and polling platform built with **.NET 8 Web API**, following **Clean Architecture** principles. Fully containerized with a production multi-stage `Dockerfile`, DRY multi-environment Docker Compose architecture (Development, Testing, Production), automated GitHub Actions CI/CD pipeline, and Docker Hub deployment.

---

## 📑 Table of Contents

- [Architectural Topology](#-architectural-topology)
- [Project File Layout](#-project-file-layout)
- [Prerequisites](#-prerequisites)
- [Environment Configuration](#-environment-configuration)
- [Docker Compose Multi-Environment Workflows](#-docker-compose-multi-environment-workflows)
  - [Development Environment](#1-development-environment)
  - [Testing Environment](#2-testing-environment)
  - [Production Environment](#3-production-environment)
- [Key Service Endpoints & Dashboards](#-key-service-endpoints--dashboards)
- [CI/CD Pipeline & Docker Hub Deployment](#-cicd-pipeline--docker-hub-deployment)
- [Local Development & Testing](#-local-development--testing)
- [Security & Operational Best Practices](#-security--operational-best-practices)

---

## 🏛 Architectural Topology

The system is organized into modular services running within an isolated container network:

```
                          ┌─────────────────────────────┐
                          │   Client / Browser / Postman │
                          └──────────────┬──────────────┘
                                         │
                                         ▼
                   ┌───────────────────────────────────────────┐
                   │    survey-api (ASP.NET Core .NET 8)       │
                   │    - Endpoints & Rate Limiting            │
                   │    - Hangfire Background Scheduler        │
                   │    - JWT Authentication & Health Checks   │
                   └──────┬────────────────────────────┬───────┘
                          │                            │
                          ▼                            ▼
         ┌──────────────────────────────┐    ┌──────────────────────────────┐
         │ survey-sqlserver (MSSQL 2022)│    │ survey-seq (Datalust Seq)    │
         │ - SurveyDB (Domain/App Data) │    │ - Centralized JSON Ingestion │
         │ - SurveyBasketJobs (Hangfire)│    │ - Search & Structured Logs   │
         └──────────────▲───────────────┘    └──────────────────────────────┘
                        │
         ┌──────────────┴───────────────┐
         │ survey-db-init (Provisioner) │
         │ Auto-creates missing DBs     │
         └──────────────────────────────┘
```

---

## 📂 Project File Layout

```text
Survey-System-Project/
├── Dockerfile                      # Multi-stage production build (Base, Build, Publish, Final)
├── .dockerignore                   # Excludes build artifacts, secrets, and IDE configs
├── .env.example                    # Master environment variable template with full docs
├── .env.development                # Local development credentials, ports & settings
├── .env.testing                    # Isolated test credentials, offset ports & test DBs
├── .env.production                 # Production configuration template (hardened secrets)
├── docker-compose.yml              # Base compose (shared networks, volumes & core services)
├── docker-compose.dev.yml          # Dev override (exposed host ports & log volume mount)
├── docker-compose.test.yml         # Test override (strict health checks & isolated test ports)
├── docker-compose.prod.yml         # Prod override (resource quotas, network isolation, log rotation)
├── .github/
│   └── workflows/
│       └── ci-cd.yml               # Automated test, build, compose validation & Docker Hub push
├── API Layer/                      # Presentation layer (Controllers, Middlewares, DI)
├── Application/                    # Use cases, interfaces, DTOs, business rules
├── Application.UnitTests/          # 250+ xUnit tests for application handlers and services
├── Domain/                         # Entities, specifications, enums, domain errors
├── Domain.UnitTests/               # 110+ xUnit tests for domain logic and models
├── Infrastructure Layer/           # EF Core DbContext, migrations, repositories, Hangfire
└── README.md                       # Comprehensive guide and operational documentation
```

---

## 🛠 Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v25.0+ with Docker Compose v2.20+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local testing outside containers)
- [Git](https://git-scm.com/)

---

## ⚙️ Environment Configuration

Configuration is managed via `.env` files corresponding to each operational environment:

| File | Purpose | Ports (API / SQL / Seq) | Default DB |
| :--- | :--- | :--- | :--- |
| `.env.example` | Canonical template of all environment variables | N/A | N/A |
| `.env.development` | Local development with host tooling access | `8080` / `1433` / `5341` | `SurveyDB` |
| `.env.testing` | Isolated integration and CI test runs | `8081` / `1434` / `5342` | `SurveyDB_Test` |
| `.env.production` | Hardened production configuration | `8080` / Internal / Internal | `SurveyDB` |

### Key Configuration Variables

- **Database Credentials**: `SA_PASSWORD`, `DB_NAME`, `HANGFIRE_DB_NAME`
- **Internal Connection Strings**: `DEFAULT_CONNECTION_STRING`, `HANGFIRE_CONNECTION_STRING`
- **Security & Authentication**:
  - `JWT_KEY`: Cryptographically secure HMAC-SHA256 secret (minimum 32 characters)
  - `JWT_ISSUER`: Expected token issuer (e.g., `SurveyBasketApp`)
  - `JWT_AUDIENCE`: Token audience
  - `JWT_EXPIRY_IN_MINUTES`: Access token expiration window
- **Observability**:
  - `SEQ_SERVER_URL`: Seq ingestion endpoint (`http://seq:5341` inside container network)
  - `LOG_FILE_PATH`: Relative or container log file destination (`/app/Logs/Log-.txt`)
- **Rate Limiting**:
  - `RATE_LIMIT_PERMIT`: Concurrency limit per client
  - `RATE_LIMIT_QUEUE`: Concurrency queue depth

---

## 🚀 Docker Compose Multi-Environment Workflows

The compose architecture employs **DRY inheritance**. `docker-compose.yml` holds all shared definitions, while environment-specific files provide clean overrides.

### 1. Development Environment

Optimized for developer ergonomics. Binds SQL Server and Seq directly to localhost for client access (SSMS, Azure Data Studio, Seq UI) and mounts `./Logs` for real-time inspection.

```bash
# Start all development services in the background
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.development up -d

# View live streaming logs for the API service
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.development logs -f api

# Stop and remove development containers
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.development down
```

### 2. Testing Environment

Spins up isolated instances using offset host ports and dedicated test databases (`SurveyDB_Test`), ensuring that test executions never collide with active local development.

```bash
# Start testing environment
docker compose -f docker-compose.yml -f docker-compose.test.yml --env-file .env.testing up -d

# Execute smoke test against the container healthcheck
curl -f http://localhost:8081/health

# Stop and clean up test resources (with volumes)
docker compose -f docker-compose.yml -f docker-compose.test.yml --env-file .env.testing down -v
```

### 3. Production Environment

Production-hardened deployment:
- **Zero Host Exposure of Data Tier**: SQL Server and Seq reside strictly on an internal backend network (`survey_prod_backend_network`) inaccessible to the public internet.
- **Resource Constraints**: Strict CPU limits (`2.0` cores) and memory quotas (`1024MB` API, `2048MB` SQL Server).
- **Log Hygiene**: Docker `json-file` log driver with `20MB` max-size and `5` file rotations.
- **High Availability**: `restart: always` across all critical components.

```bash
# Verify production compose schema before deployment
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production config

# Launch production stack
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.production up -d --build

# Monitor resource consumption
docker stats
```

---

## 🌐 Key Service Endpoints & Dashboards

When running under the **Development** profile (`.env.development`):

| Service / Tool | URL | Description |
| :--- | :--- | :--- |
| **Swagger UI** | `http://localhost:8080/swagger` | Interactive OpenAPI documentation & testing |
| **Health Check** | `http://localhost:8080/health` | Service health status JSON endpoint |
| **Hangfire Dashboard** | `http://localhost:8080/jobs` | Background job processing & scheduled triggers |
| **Seq Log Viewer** | `http://localhost:5341` | Structured log search, filtering & analytics |
| **SQL Server** | `localhost,1433` | Connect with SSMS / Azure Data Studio (`sa` / `DevPassword123!`) |

---

## 🔄 CI/CD Pipeline & Docker Hub Deployment

The GitHub Actions workflow at [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml) automates continuous testing and delivery:

```
[ Push to main / master / tag ]
               │
               ▼
┌──────────────────────────────┐
│ Job 1: Build & Unit Tests    │
│ - Restores .NET 8 packages   │
│ - Builds solution (Release)  │
│ - Runs Domain.UnitTests      │
│ - Runs Application.UnitTests │
│ - Uploads coverage artifacts │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Job 2: Compose Validation    │
│ - Validates Dev compose      │
│ - Validates Test compose     │
│ - Validates Prod compose     │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Job 3: Docker Hub Push       │
│ - Docker Buildx multi-arch   │
│ - Authenticates via secrets  │
│ - Tags: latest, sha-<git-sha>│
│ - GitHub Actions layer cache │
└──────────────────────────────┘
```

### Required GitHub Secrets

To enable automated Docker Hub deployment, configure these secrets in your repository settings (**Settings > Secrets and variables > Actions**):

| Secret Name | Description |
| :--- | :--- |
| `DOCKERHUB_USERNAME` | Your Docker Hub username or organization ID |
| `DOCKERHUB_TOKEN` | Docker Hub Personal Access Token (PAT) with `Read & Write` permissions |

---

## 🧪 Local Development & Testing

To run tests without Docker on your local workstation:

```bash
# Restore all NuGet packages
dotnet restore

# Run the complete test suite (Domain + Application unit tests)
dotnet test --configuration Release --verbosity normal

# Run tests with code coverage collection
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🔒 Security & Operational Best Practices

1. **Production Secrets**: Never commit real production secrets to version control. Replace all `CHANGE_ME_IN_PRODUCTION_*` placeholders in `.env.production` using a secure secret manager (e.g., Azure Key Vault, AWS Secrets Manager, GitHub Secrets).
2. **Reverse Proxy / SSL**: In production, place the API container behind a TLS-terminating reverse proxy (Nginx, Traefik, Cloudflare, or AWS ALB) mapped to `http://localhost:8080`.
3. **Database Backups**: Always back up the persistent volume `survey_sqlserver_data` regularly in production.
4. **Non-Root Execution**: The runtime container is built upon `mcr.microsoft.com/dotnet/aspnet:8.0` with minimal operating system attack surface.

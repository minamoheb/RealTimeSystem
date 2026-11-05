# Real-Time System Monitoring Dashboard

## Project Overview

Real-Time System is a full-stack monitoring platform for infrastructure/system health.
It provides:

- Near-real-time server metrics collection
- Alerting and acknowledgement workflows
- Report generation with background processing (Hangfire)
- JWT authentication and role-based authorization
- SignalR push updates for live dashboards

## Features

- **Monitoring**: server list, status, and latest metric snapshots
- **Alerts**: paged alerts and acknowledgement endpoint
- **Reports**: queued report generation and analysis pipeline
- **Maintenance**: schedule maintenance windows
- **Identity**: login, refresh token, logout, current user profile
- **Versioned API**: `v1` and `v2` endpoints (`v2` includes `healthScore`)

## Technology Stack

- **Backend**: ASP.NET Core 10, EF Core 10, SQL Server, Hangfire, SignalR, Serilog
- **Frontend**: Angular 19, RxJS, Chart.js
- **Testing**: xUnit for backend, Karma/Jasmine for frontend
- **Containerization**: Docker + Docker Compose
- **CI/CD**: GitHub Actions

## Architecture Diagram

```mermaid
flowchart LR
    U[Browser Client] --> W[Angular Frontend (Nginx)]
    W -->|REST| A[ASP.NET Core WebAPI]
    W -->|WebSocket/SignalR| A
    A --> D[(SQL Server)]
    A --> H[(Hangfire Storage in SQL Server)]
    A --> L[(Serilog Logs)]
```

See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed architecture and design notes.

## Getting Started

### Prerequisites

- .NET SDK 10
- Node.js 22+
- npm 10+
- Docker Desktop (for containerized run)
- SQL Server (local/development setup without Docker)

### Installation

```powershell
git clone <your-repository-url>
cd Real-TimeSystem
```

### Run Locally (Without Docker)

Backend:

```powershell
dotnet run --project src/RealTimeSystem.WebAPI
```

`ASPNETCORE_ENVIRONMENT` selects the settings file. Launch profiles set it for you.

| Environment | API settings | UI command | API address |
| --- | --- | --- | --- |
| Development | `appsettings.Development.json` (local database, seed, demo JWT key) | `npm start` | `http://localhost:5080` |
| Staging | `appsettings.Staging.json` | `npx ng serve --configuration=staging` | `http://localhost:5081` |
| Production | `appsettings.Production.json` | `npx ng build --configuration=production` | `http://localhost:5082` |

Staging and Production do not store the database password or JWT key. Set them in the environment before starting:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Staging"
$env:ConnectionStrings__SqlServer = "server=.;database=RealTimeMonitoring_Staging;trusted_connection=true;trustservercertificate=true"
$env:Jwt__Key = "replace-with-a-staging-key-at-least-32-characters"
$env:Cors__Origins__0 = "https://staging.example.com"
dotnet run --project src/RealTimeSystem.WebAPI --launch-profile Staging
```

The Angular staging and production builds call the API on the same host (`/api`, `/hubs`). Development calls `http://localhost:5080` directly.

Frontend:

```powershell
cd client
npm install
npm start
```

URLs:

- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`
- Health: `http://localhost:5080/health`
- Frontend: `http://localhost:4200`
- Hangfire Dashboard: `http://localhost:5080/hangfire` (development)

The sign-in form is empty. Demo accounts are documented here, not filled in on the page.

Default seeded users:

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@monitor.local | Admin123! |
| User | analyst@monitor.local | User123! |

### Run with Docker

```powershell
docker compose up --build
```

URLs:

- Frontend: `http://localhost:8088`
- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`

## Testing Instructions

Backend:

```powershell
dotnet test tests/RealTimeSystem.Domain.Tests/RealTimeSystem.Domain.Tests.csproj
dotnet test tests/RealTimeSystem.Application.Tests/RealTimeSystem.Application.Tests.csproj
dotnet test tests/RealTimeSystem.WebAPI.Tests/RealTimeSystem.WebAPI.Tests.csproj
```

Frontend:

```powershell
cd client
npm test -- --watch=false --browsers=ChromeHeadless
```

## API Documentation

- Swagger UI: `http://localhost:5080/swagger`
- Additional API guide: [API.md](./API.md)

## CI/CD

- GitHub Actions workflow: [ci-cd.yml](./.github/workflows/ci-cd.yml)
- Pipeline includes restore, build, tests, Docker build, and image publish to GHCR.

## Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for:

- environment variables
- database and migrations
- docker deployment steps
- cloud-agnostic rollout guidance
- troubleshooting

## Screenshots / GIFs

Add your captured screenshots/GIFs under `docs/screenshots/` and reference them here during submission.

## Known Limitations / Future Improvements

- Redis backplane is not enabled (single-node SignalR deployment by default)
- Hangfire workers run inside API service (can be split into dedicated worker host later)
- No blue/green deployment automation yet (pipeline provides build/test/image stages + deployment guidance)
- Add stronger secret management integration (Key Vault / external secret stores)

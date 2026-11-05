# DEPLOYMENT GUIDE

## 1. Prerequisites and Dependencies

### Runtime Dependencies

- Docker Engine + Docker Compose plugin (recommended)
- OR:
  - .NET SDK / Runtime 10
  - SQL Server 2019+ (or Azure SQL equivalent)
  - Node.js 22+ (if running frontend without Docker)

### Application Dependencies

- SQL Server database for application and Hangfire persistence
- Valid JWT signing key (minimum 32 characters)

---

## 2. Environment Variables Configuration

Key runtime variables:

| Variable | Required | Example |
| --- | --- | --- |
| `Database__Provider` | Yes | `SqlServer` |
| `ConnectionStrings__SqlServer` | Yes | `Server=db,1433;Database=RealTimeMonitoring;User Id=sa;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=true` |
| `Database__ApplyMigrations` | Recommended | `true` |
| `Seed__Enabled` | Optional | `true` (non-production) |
| `Jwt__Issuer` | Yes | `RealTimeSystem` |
| `Jwt__Audience` | Yes | `RealTimeSystem.Client` |
| `Jwt__Key` | Yes | long random secret (32+ chars) |
| `Hangfire__Enabled` | Optional | `true` |
| `Cors__Origins__0` | Recommended | `https://your-frontend-host` |

> Do not commit production secrets into source control.

---

## 3. Database Setup and Migrations

### Local/Server Setup

1. Create SQL Server instance/database access.
2. Set `ConnectionStrings__SqlServer`.
3. Keep `Database__ApplyMigrations=true` for automatic startup migration.

### Manual Migration (optional)

```powershell
dotnet ef database update `
  --project src/RealTimeSystem.Infrastructure/RealTimeSystem.Infrastructure.csproj `
  --startup-project src/RealTimeSystem.WebAPI/RealTimeSystem.WebAPI.csproj `
  --context ApplicationDbContext
```

For production, prefer migration execution in a controlled release step.

---

## 4. Docker Deployment Steps

### Standard Compose Deployment

```powershell
docker compose up --build -d
```

Services:

- `web` (Angular + Nginx): `http://localhost:8088`
- `api` (ASP.NET Core): `http://localhost:5080`
- `db` (SQL Server): `localhost:1433`

### Stop Deployment

```powershell
docker compose down
```

### Remove Volumes (destructive)

```powershell
docker compose down -v
```

---

## 5. Cloud Deployment Considerations (Cloud-Agnostic)

- Use managed SQL service where possible.
- Externalize secrets (secret manager / key vault equivalent).
- Use managed container hosting (Kubernetes, Container Apps, ECS/Fargate, App Service containers, etc.).
- Route traffic through TLS terminator / ingress with HTTPS only.
- Restrict API ingress to trusted frontend origins.
- Run multiple API replicas behind load balancer.
- Use persistent storage only where needed (or external object storage/logging).

---

## 6. Scaling Considerations

- **API Horizontal Scale**:
  - Add instances based on CPU/RPS
  - Ensure stateless API behavior
- **SignalR Scale**:
  - Add backplane/service (Redis/Azure SignalR) when running multiple API replicas
- **Background Jobs**:
  - Split Hangfire workers into dedicated worker host when throughput grows
  - Tune queue/worker count
- **Database**:
  - Add indexing and query tuning for metrics/alerts/reporting paths
  - Monitor connection pool limits

---

## 7. Monitoring and Logging Setup

- Serilog logs to console and rolling files.
- Expose `/health` endpoint for liveness probes.
- Recommended production integrations:
  - centralized logs (ELK/OpenSearch/Splunk/Cloud logging)
  - metrics/alerts stack (Prometheus/Grafana or cloud-native monitoring)
  - uptime and synthetic checks for API + frontend

---

## 8. Troubleshooting Common Issues

### API cannot connect to SQL Server

- Verify SQL server container/service is healthy.
- Check `ConnectionStrings__SqlServer` host, credentials, and port.
- Confirm firewall/network allows connection.

### JWT authentication fails

- Verify `Jwt__Issuer`, `Jwt__Audience`, and `Jwt__Key`.
- Ensure key length is at least 32 characters.
- Check token expiration and server clock drift.

### Hangfire dashboard unavailable

- Ensure `Hangfire__Enabled=true`.
- Confirm application starts without migration failures.
- Check `/hangfire` access policy (dev-only localhost policy in current setup).

### Frontend cannot reach backend

- Verify Nginx proxy in `client/nginx.conf` points to `api:8080`.
- Confirm CORS origins include frontend URL.

### Migration errors after schema changes

- Inspect migration history table and pending migrations.
- Validate key-type conversion migration order before applying in production.

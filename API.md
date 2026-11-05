# API REFERENCE

Base URL (local): `http://localhost:5080`  
Swagger: `/swagger`

## Authentication Flow

1. `POST /api/v1/auth/login` with credentials
2. Receive `accessToken` and `refreshToken`
3. Send `Authorization: Bearer <accessToken>` on protected endpoints
4. Refresh token via `POST /api/v1/auth/refresh`
5. Revoke session via `POST /api/v1/auth/logout`

SignalR clients pass JWT as `access_token` query string for hub negotiation.

---

## Auth Endpoints

### POST `/api/v1/auth/login`

Request:

```json
{
  "email": "admin@monitor.local",
  "password": "Admin123!"
}
```

Response (200):

```json
{
  "accessToken": "eyJ...",
  "expiresAtUtc": "2026-11-04T12:00:00Z",
  "refreshToken": "base64-token",
  "refreshTokenExpiresAtUtc": "2026-11-11T12:00:00Z",
  "user": {
    "id": 1,
    "email": "admin@monitor.local",
    "fullName": "Amina Hassan",
    "roles": ["Admin"]
  }
}
```

### POST `/api/v1/auth/refresh`

```json
{
  "refreshToken": "base64-token"
}
```

### POST `/api/v1/auth/logout`

```json
{
  "refreshToken": "base64-token"
}
```

### GET `/api/v1/auth/me`

Returns current authenticated user profile.

---

## Servers

### GET `/api/v1/servers`

Paged server list. Supports query filters/sorting/pagination from `GetServersQuery`.

### GET `/api/v1/servers/{id}`

Returns server details.

### GET `/api/v1/servers/{id}/metrics`

Query params:

- `fromUtc`
- `toUtc`
- `page` (default 1)
- `pageSize` (default 200)

### POST `/api/v1/servers` (Admin)

```json
{
  "name": "edge-gateway-02",
  "hostName": "10.0.1.12",
  "environmentName": "Production",
  "description": "New gateway node"
}
```

### PUT `/api/v1/servers/{id}` (Admin)

### DELETE `/api/v1/servers/{id}` (Admin)

---

## Servers v2

### GET `/api/v2/servers`

Returns the v2 server contract including computed `healthScore`.

---

## Metrics

### GET `/api/v1/metrics/latest`

Query params:

- `page` (default 1)
- `pageSize` (default 20)

---

## Alerts

### GET `/api/v1/alerts`

Paged alert list.

### POST `/api/v1/alerts/{id}/acknowledge`

Acknowledge an alert.

---

## Reports

### GET `/api/v1/reports`

Paged reports.

### GET `/api/v1/reports/{id}`

Report details/status.

### POST `/api/v1/reports`

```json
{
  "title": "Last 6 hours summary",
  "fromUtc": "2026-11-04T06:00:00Z",
  "toUtc": "2026-11-04T12:00:00Z"
}
```

Returns `202 Accepted`, then report pipeline runs in background.

---

## Jobs

### GET `/api/v1/jobs`

Returns job overview.

### POST `/api/v1/jobs/collect` (Admin)

Triggers immediate metrics collection.

### POST `/api/v1/jobs/maintenance` (Admin)

```json
{
  "serverId": 2,
  "reason": "Kernel patching",
  "delayMinutes": 30
}
```

---

## Users (Admin)

### GET `/api/v1/users`

Paged users.

### POST `/api/v1/users`

```json
{
  "email": "new.user@monitor.local",
  "fullName": "New User",
  "password": "P@ssw0rd123!",
  "roles": ["User"]
}
```

### PUT `/api/v1/users/{id}`

### DELETE `/api/v1/users/{id}`

---

## SignalR Hub

Hub route: `/hubs/monitoring`

### Hub Methods

- `WatchServer(serverId: long)`

### Server-to-Client Events

- `MetricUpdated`
- `AlertRaised`
- `ReportUpdated`
- `MaintenanceScheduled`
- `PresenceUpdated`

---

## Common HTTP Statuses

- `200 OK`, `201 Created`, `202 Accepted`, `204 No Content`
- `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`
- `409 Conflict`, `429 Too Many Requests`

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface UserProfile {
  id: number;
  email: string;
  fullName: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  user: UserProfile;
}

export interface ServerDto {
  id: number;
  name: string;
  hostName: string;
  environmentName: string;
  description: string | null;
  status: string;
  isActive: boolean;
  createdAtUtc: string;
  cpuPercent: number | null;
  memoryPercent: number | null;
  diskPercent: number | null;
  networkMbps: number | null;
  responseTimeMs: number | null;
  collectedAtUtc: string | null;
  openAlertCount: number;
}

export interface LiveMetricDto {
  serverId: number;
  serverName: string;
  status: string;
  cpuPercent: number;
  memoryPercent: number;
  diskPercent: number;
  networkMbps: number;
  responseTimeMs: number;
  collectedAtUtc: string;
}

export interface MetricHistoryDto {
  id: number;
  serverId: number;
  cpuPercent: number;
  memoryPercent: number;
  diskPercent: number;
  networkMbps: number;
  responseTimeMs: number;
  collectedAtUtc: string;
}

export interface AlertDto {
  id: number;
  serverId: number;
  serverName: string;
  metricName: string;
  value: number;
  threshold: number;
  severity: string;
  message: string;
  isAcknowledged: boolean;
  createdAtUtc: string;
  acknowledgedAtUtc: string | null;
}

export interface ReportDto {
  id: number;
  title: string;
  fromUtc: string;
  toUtc: string;
  status: string;
  summary: string | null;
  failureReason: string | null;
  requestedAtUtc: string;
  completedAtUtc: string | null;
  requestedByEmail: string;
}

export interface UserDto {
  id: number;
  email: string;
  fullName: string;
  isActive: boolean;
  createdAtUtc: string;
  roles: string[];
}

export interface JobListItem {
  id: string;
  name: string;
  state: string;
  createdAtUtc: string | null;
}

export interface JobOverview {
  enqueued: number;
  processing: number;
  scheduled: number;
  succeeded: number;
  failed: number;
  recent: JobListItem[];
}

export interface PresenceUser {
  userId: number;
  name: string;
  connections: number;
}

export interface LivePoint {
  at: string;
  cpu: number;
  memory: number;
  disk: number;
  network: number;
}

export interface ApiError {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { AlertDto, LiveMetricDto, LivePoint, PresenceUser, ReportDto, ServerDto } from './models';

interface MonitoringState {
  servers: ServerDto[];
  alerts: AlertDto[];
  presence: PresenceUser[];
  live: LivePoint[];
  connected: boolean;
  latestReport: ReportDto | null;
}

const initialState: MonitoringState = {
  servers: [],
  alerts: [],
  presence: [],
  live: [],
  connected: false,
  latestReport: null
};

export const MonitoringStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    setServers(servers: ServerDto[]) {
      patchState(store, { servers });
    },
    setAlerts(alerts: AlertDto[]) {
      patchState(store, { alerts });
    },
    setPresence(presence: PresenceUser[]) {
      patchState(store, { presence });
    },
    setConnected(connected: boolean) {
      patchState(store, { connected });
    },
    setLatestReport(latestReport: ReportDto) {
      patchState(store, { latestReport });
    },
    applyMetric(metric: LiveMetricDto) {
      const servers = store.servers().map((server) => server.id === metric.serverId
        ? {
            ...server,
            status: metric.status,
            cpuPercent: metric.cpuPercent,
            memoryPercent: metric.memoryPercent,
            diskPercent: metric.diskPercent,
            networkMbps: metric.networkMbps,
            responseTimeMs: metric.responseTimeMs,
            collectedAtUtc: metric.collectedAtUtc
          }
        : server);
      const live = [
        ...store.live(),
        {
          at: metric.collectedAtUtc,
          cpu: metric.cpuPercent,
          memory: metric.memoryPercent,
          disk: metric.diskPercent,
          network: metric.networkMbps
        }
      ].slice(-40);
      patchState(store, { servers, live });
    },
    pushAlert(alert: AlertDto) {
      const exists = store.alerts().some((item) => item.id === alert.id);
      const alerts = exists ? store.alerts() : [alert, ...store.alerts()].slice(0, 40);
      const servers = store.servers().map((server) => server.id === alert.serverId
        ? { ...server, status: server.status === 'Offline' ? server.status : 'Degraded', openAlertCount: server.openAlertCount + (exists ? 0 : 1) }
        : server);
      patchState(store, { alerts, servers });
    },
    acknowledge(id: number) {
      patchState(store, {
        alerts: store.alerts().map((alert) => alert.id === id ? { ...alert, isAcknowledged: true } : alert)
      });
    }
  }))
);

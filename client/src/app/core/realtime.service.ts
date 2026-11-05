import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { apiUrl } from './api-config';
import { AuthService } from './auth.service';
import { AlertDto, LiveMetricDto, PresenceUser, ReportDto } from './models';
import { MonitoringStore } from './monitoring.store';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly auth = inject(AuthService);
  private readonly store = inject(MonitoringStore);
  private connection?: HubConnection;

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(apiUrl('/hubs/monitoring'), { accessTokenFactory: () => this.auth.token() ?? '' })
      .withAutomaticReconnect()
      .build();

    connection.on('MetricUpdated', (metric: LiveMetricDto) => this.store.applyMetric(metric));
    connection.on('AlertRaised', (alert: AlertDto) => this.store.pushAlert(alert));
    connection.on('PresenceUpdated', (presence: PresenceUser[]) => this.store.setPresence(presence));
    connection.on('ReportUpdated', (report: ReportDto) => this.store.setLatestReport(report));
    connection.onreconnected(() => this.store.setConnected(true));
    connection.onclose(() => this.store.setConnected(false));

    await connection.start();
    this.connection = connection;
    this.store.setConnected(true);
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
    this.store.setConnected(false);
  }
}

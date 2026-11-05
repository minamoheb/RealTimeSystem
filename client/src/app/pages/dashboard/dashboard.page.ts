import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { errorMessage } from '../../core/format';
import { MonitoringStore } from '../../core/monitoring.store';
import { MetricChartComponent } from '../../shared/metric-chart/metric-chart.component';
import { AlertListComponent, KpiCardComponent, ServerCardComponent } from '../../shared/widgets';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, KpiCardComponent, ServerCardComponent, AlertListComponent, MetricChartComponent],
  templateUrl: './dashboard.page.html',
  styleUrl: './dashboard.page.css'
})
export class DashboardPage {
  readonly store = inject(MonitoringStore);
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);
  readonly loading = signal(true);
  readonly collecting = signal(false);
  readonly error = signal('');
  readonly degraded = computed(() => this.store.servers().filter((server) => server.status === 'Degraded').length);
  readonly openAlerts = computed(() => this.store.alerts().filter((alert) => !alert.isAcknowledged).length);
  readonly visibleAlerts = computed(() => this.store.alerts().slice(0, 6));
  readonly cpuLabels = computed(() => this.store.servers().map((server) => server.name));
  readonly cpuSeries = computed(() => [{
    label: 'CPU %',
    color: '#d6ff4a',
    data: this.store.servers().map((server) => server.cpuPercent ?? 0)
  }]);

  constructor() {
    this.api.servers().subscribe({
      next: (page) => {
        this.store.setServers(page.items);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(errorMessage(error, 'Unable to load servers.'));
        this.loading.set(false);
      }
    });
    this.api.alerts(false).subscribe({
      next: (page) => this.store.setAlerts(page.items),
      error: () => undefined
    });
  }

  collect(): void {
    this.collecting.set(true);
    this.api.collectNow().subscribe({
      next: () => this.collecting.set(false),
      error: (error) => {
        this.error.set(errorMessage(error, 'Collection could not be queued.'));
        this.collecting.set(false);
      }
    });
  }

  ack(id: number): void {
    this.api.acknowledge(id).subscribe({
      next: () => this.store.acknowledge(id),
      error: (error) => this.error.set(errorMessage(error, 'Could not acknowledge the alert.'))
    });
  }
}

import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { errorMessage, formatPercent } from '../../core/format';
import { AlertDto, MetricHistoryDto, ServerDto } from '../../core/models';
import { MetricChartComponent } from '../../shared/metric-chart/metric-chart.component';
import { AlertListComponent, StatusPillComponent } from '../../shared/widgets';

@Component({
  selector: 'app-server-detail',
  standalone: true,
  imports: [RouterLink, StatusPillComponent, MetricChartComponent, AlertListComponent],
  templateUrl: './server-detail.page.html',
  styleUrl: './server-detail.page.css'
})
export class ServerDetailPage {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  readonly server = signal<ServerDto | null>(null);
  readonly history = signal<MetricHistoryDto[]>([]);
  readonly alerts = signal<AlertDto[]>([]);
  readonly error = signal('');
  readonly percent = formatPercent;
  readonly labels = signal<string[]>([]);

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id) || id <= 0) {
      this.error.set('Invalid server id.');
      return;
    }

    this.api.server(id).subscribe({
      next: (server) => this.server.set(server),
      error: () => this.error.set('That server could not be loaded.')
    });
    this.api.metrics(id).subscribe({
      next: (page) => {
        this.history.set(page.items);
        this.labels.set(page.items.map((item) => new Date(item.collectedAtUtc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })));
      },
      error: () => this.error.set('Metric history could not be loaded.')
    });
    this.api.alerts(false, id).subscribe({
      next: (page) => this.alerts.set(page.items),
      error: () => this.error.set('Alerts for this server could not be loaded.')
    });
  }

  ack(id: number): void {
    this.api.acknowledge(id).subscribe({
      next: (alert) => this.alerts.update((alerts) => alerts.map((item) => item.id === alert.id ? alert : item)),
      error: (error) => this.error.set(errorMessage(error, 'Could not acknowledge the alert.'))
    });
  }

  remove(): void {
    const server = this.server();
    if (!server) {
      return;
    }

    this.api.deleteServer(server.id).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: (error) => this.error.set(errorMessage(error, 'The server could not be removed.'))
    });
  }

  series(key: 'cpuPercent' | 'memoryPercent' | 'diskPercent' | 'responseTimeMs', color: string) {
    return [{ label: key, data: this.history().map((item) => item[key]), color }];
  }
}

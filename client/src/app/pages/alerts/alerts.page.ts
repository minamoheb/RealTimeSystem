import { Component, inject, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { errorMessage } from '../../core/format';
import { AlertDto } from '../../core/models';
import { AlertListComponent } from '../../shared/widgets';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [AlertListComponent],
  templateUrl: './alerts.page.html'
})
export class AlertsPage {
  private readonly api = inject(ApiService);
  readonly alerts = signal<AlertDto[]>([]);
  readonly error = signal('');
  readonly openOnly = signal(false);

  constructor() {
    this.load();
  }

  toggleOpen(): void {
    this.openOnly.update((value) => !value);
    this.load();
  }

  ack(id: number): void {
    this.api.acknowledge(id).subscribe({
      next: (alert) => this.alerts.update((alerts) => alerts.map((item) => item.id === alert.id ? alert : item)),
      error: (error) => this.error.set(errorMessage(error, 'Could not acknowledge the alert.'))
    });
  }

  private load(): void {
    this.api.alerts(this.openOnly()).subscribe({
      next: (page) => this.alerts.set(page.items),
      error: (error) => this.error.set(errorMessage(error, 'Alerts could not be loaded.'))
    });
  }
}

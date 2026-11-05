import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { apiOrigin } from '../../core/api-config';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { errorMessage } from '../../core/format';
import { JobOverview, ServerDto } from '../../core/models';
import { KpiCardComponent } from '../../shared/widgets';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, KpiCardComponent, DatePipe],
  templateUrl: './jobs.page.html',
  styleUrl: './jobs.page.css'
})
export class JobsPage {
  readonly auth = inject(AuthService);
  readonly hangfireUrl = `${apiOrigin}/hangfire`;
  private readonly api = inject(ApiService);
  private readonly forms = inject(FormBuilder);
  readonly overview = signal<JobOverview | null>(null);
  readonly servers = signal<ServerDto[]>([]);
  readonly message = signal('');
  readonly error = signal('');
  readonly form = this.forms.nonNullable.group({
    serverId: [0, Validators.min(1)],
    reason: ['Patch window', Validators.required],
    delayMinutes: [2, [Validators.required, Validators.min(1), Validators.max(1440)]]
  });

  constructor() {
    this.reload();
    if (this.auth.isAdmin()) {
      this.api.servers().subscribe({
        next: (page) => {
          this.servers.set(page.items);
          if (page.items[0]) {
            this.form.controls.serverId.setValue(page.items[0].id);
          }
        }
      });
    }
  }

  openHangfire(): void {
    const open = () => window.open(this.hangfireUrl, '_blank', 'noopener');
    if (!this.auth.isAdmin()) {
      open();
      return;
    }

    this.api.prepareHangfire().subscribe({ next: open, error: open });
  }

  retry(id: string): void {
    this.error.set('');
    this.api.retryJob(id).subscribe({
      next: () => {
        this.message.set('Failed job queued again.');
        this.reload();
      },
      error: (error) => this.error.set(errorMessage(error, 'The job could not be retried.'))
    });
  }

  private reload(): void {
    this.api.jobs().subscribe({
      next: (overview) => this.overview.set(overview),
      error: (error) => this.error.set(errorMessage(error, 'Job status is unavailable.'))
    });
  }

  schedule(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.api.scheduleMaintenance(value.serverId, value.reason, value.delayMinutes).subscribe({
      next: () => this.message.set(`Maintenance queued to run in ${value.delayMinutes} minutes.`),
      error: (error) => this.error.set(errorMessage(error, 'Maintenance could not be scheduled.'))
    });
  }
}

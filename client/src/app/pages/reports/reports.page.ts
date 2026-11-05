import { Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { errorMessage } from '../../core/format';
import { ReportDto } from '../../core/models';
import { MonitoringStore } from '../../core/monitoring.store';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './reports.page.html',
  styleUrl: './reports.page.css'
})
export class ReportsPage {
  private readonly api = inject(ApiService);
  private readonly store = inject(MonitoringStore);
  private readonly forms = inject(FormBuilder);
  readonly reports = signal<ReportDto[]>([]);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly form = this.forms.nonNullable.group({
    title: ['Fleet window', Validators.required],
    from: [this.localInput(new Date(Date.now() - 6 * 60 * 60 * 1000)), Validators.required],
    to: [this.localInput(new Date()), Validators.required]
  });

  constructor() {
    this.reload();
    effect(() => {
      const latest = this.store.latestReport();
      if (!latest) {
        return;
      }

      this.reports.update((reports) => {
        const index = reports.findIndex((report) => report.id === latest.id);
        if (index < 0) {
          return [latest, ...reports];
        }

        const next = [...reports];
        next[index] = { ...next[index], ...latest };
        return next;
      });
    });
  }

  generate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.error.set('');
    const value = this.form.getRawValue();
    this.api.createReport(value.title, new Date(value.from).toISOString(), new Date(value.to).toISOString()).subscribe({
      next: () => {
        this.busy.set(false);
        this.reload();
      },
      error: (error) => {
        this.error.set(errorMessage(error, 'The report could not be queued.'));
        this.busy.set(false);
      }
    });
  }

  private reload(): void {
    this.api.reports().subscribe({
      next: (page) => this.reports.set(page.items),
      error: (error) => this.error.set(errorMessage(error, 'Reports could not be loaded.'))
    });
  }

  private localInput(date: Date): string {
    const pad = (value: number) => String(value).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }
}

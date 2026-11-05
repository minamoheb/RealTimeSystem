import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { errorMessage } from '../../core/format';

@Component({
  selector: 'app-server-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './server-form.page.html'
})
export class ServerFormPage {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly forms = inject(FormBuilder);
  readonly serverId = signal<number | null>(null);
  readonly error = signal('');
  readonly busy = signal(false);
  readonly form = this.forms.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    hostName: ['', [Validators.required, Validators.maxLength(200)]],
    environmentName: ['Production', [Validators.required, Validators.maxLength(40)]],
    description: ['', Validators.maxLength(500)]
  });

  constructor() {
    const raw = this.route.snapshot.paramMap.get('id');
    if (!raw) {
      return;
    }

    const id = Number(raw);
    if (!Number.isFinite(id) || id <= 0) {
      this.error.set('Invalid server id.');
      return;
    }

    this.serverId.set(id);
    this.api.server(id).subscribe({
      next: (server) => this.form.patchValue({
        name: server.name,
        hostName: server.hostName,
        environmentName: server.environmentName,
        description: server.description ?? ''
      }),
      error: () => this.error.set('That server could not be loaded.')
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body = {
      name: value.name.trim(),
      hostName: value.hostName.trim(),
      environmentName: value.environmentName.trim(),
      description: value.description.trim() ? value.description.trim() : null
    };
    this.busy.set(true);
    this.error.set('');
    const id = this.serverId();
    const request = id === null ? this.api.createServer(body) : this.api.updateServer(id, body);
    request.subscribe({
      next: (server) => void this.router.navigate(['/servers', server.id]),
      error: (error) => {
        this.error.set(errorMessage(error, 'The server could not be saved.'));
        this.busy.set(false);
      }
    });
  }
}

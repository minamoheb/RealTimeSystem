import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { errorMessage } from '../../core/format';
import { UserDto } from '../../core/models';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './users.page.html',
  styleUrl: './users.page.css'
})
export class UsersPage {
  private readonly api = inject(ApiService);
  private readonly forms = inject(FormBuilder);
  readonly users = signal<UserDto[]>([]);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly form = this.forms.nonNullable.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['User', Validators.required]
  });

  constructor() {
    this.reload();
  }

  create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    const value = this.form.getRawValue();
    this.api.createUser(value.email, value.fullName, value.password, [value.role]).subscribe({
      next: () => {
        this.busy.set(false);
        this.form.reset({ fullName: '', email: '', password: '', role: 'User' });
        this.reload();
      },
      error: (error) => {
        this.error.set(errorMessage(error, 'User could not be created.'));
        this.busy.set(false);
      }
    });
  }

  deactivate(user: UserDto): void {
    this.api.deactivateUser(user.id).subscribe({
      next: () => this.reload(),
      error: (error) => this.error.set(errorMessage(error, 'User could not be deactivated.'))
    });
  }

  private reload(): void {
    this.api.users().subscribe({
      next: (page) => this.users.set(page.items),
      error: (error) => this.error.set(errorMessage(error, 'Users could not be loaded.'))
    });
  }
}

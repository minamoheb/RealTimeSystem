import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { apiUrl } from './api-config';
import { AuthResponse, UserProfile } from './models';

const ACCESS = 'rts.access';
const REFRESH = 'rts.refresh';
const USER = 'rts.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly token = signal<string | null>(localStorage.getItem(ACCESS));
  readonly user = signal<UserProfile | null>(this.readUser());
  readonly isAdmin = computed(() => this.user()?.roles.includes('Admin') ?? false);

  isAuthenticated(): boolean {
    return !!this.token();
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(apiUrl('/api/v1/auth/login'), { email, password }).pipe(
      tap((response) => this.persist(response))
    );
  }

  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(apiUrl('/api/v1/auth/refresh'), { refreshToken: localStorage.getItem(REFRESH) }).pipe(
      tap((response) => this.persist(response))
    );
  }

  logout(): void {
    const refreshToken = localStorage.getItem(REFRESH);
    if (this.token()) {
      this.http.post(apiUrl('/api/v1/auth/logout'), { refreshToken }).subscribe({ error: () => undefined });
    }

    localStorage.removeItem(ACCESS);
    localStorage.removeItem(REFRESH);
    localStorage.removeItem(USER);
    this.token.set(null);
    this.user.set(null);
    void this.router.navigateByUrl('/login');
  }

  private persist(response: AuthResponse): void {
    localStorage.setItem(ACCESS, response.accessToken);
    localStorage.setItem(REFRESH, response.refreshToken);
    localStorage.setItem(USER, JSON.stringify(response.user));
    this.token.set(response.accessToken);
    this.user.set(response.user);
  }

  private readUser(): UserProfile | null {
    const raw = localStorage.getItem(USER);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as UserProfile;
    } catch {
      return null;
    }
  }
}

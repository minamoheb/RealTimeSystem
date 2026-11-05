import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { RealtimeService } from '../../core/realtime.service';
import { MonitoringStore } from '../../core/monitoring.store';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css'
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  readonly store = inject(MonitoringStore);
  private readonly realtime = inject(RealtimeService);
  readonly theme = signal(localStorage.getItem('rts.theme') === 'light' ? 'light' : 'dark');

  constructor() {
    effect(() => {
      document.documentElement.dataset['theme'] = this.theme();
      localStorage.setItem('rts.theme', this.theme());
    });
    void this.realtime.start().catch(() => this.store.setConnected(false));
  }

  toggleTheme(): void {
    this.theme.update((theme) => theme === 'dark' ? 'light' : 'dark');
  }

  logout(): void {
    void this.realtime.stop();
    this.auth.logout();
  }
}

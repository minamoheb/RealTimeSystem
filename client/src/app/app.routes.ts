import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/login/login.page').then((module) => module.LoginPage) },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell.component').then((module) => module.ShellComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.page').then((module) => module.DashboardPage) },
      { path: 'alerts', loadComponent: () => import('./pages/alerts/alerts.page').then((module) => module.AlertsPage) },
      { path: 'servers/new', canActivate: [adminGuard], loadComponent: () => import('./pages/server-form/server-form.page').then((module) => module.ServerFormPage) },
      { path: 'servers/:id/edit', canActivate: [adminGuard], loadComponent: () => import('./pages/server-form/server-form.page').then((module) => module.ServerFormPage) },
      { path: 'servers/:id', loadComponent: () => import('./pages/server-detail/server-detail.page').then((module) => module.ServerDetailPage) },
      { path: 'reports', loadComponent: () => import('./pages/reports/reports.page').then((module) => module.ReportsPage) },
      { path: 'jobs', loadComponent: () => import('./pages/jobs/jobs.page').then((module) => module.JobsPage) },
      { path: 'users', canActivate: [adminGuard], loadComponent: () => import('./pages/users/users.page').then((module) => module.UsersPage) }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

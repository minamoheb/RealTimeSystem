import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { apiUrl } from './api-config';
import { AlertDto, JobOverview, MetricHistoryDto, Paged, ReportDto, ServerDto, UserDto } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  servers(search = '', environment = '', status = '') {
    return this.http.get<Paged<ServerDto>>(apiUrl('/api/v1/servers'), {
      params: this.params({ page: 1, pageSize: 50, search, environment, status, sortBy: 'name' })
    });
  }

  server(id: number) {
    return this.http.get<ServerDto>(apiUrl(`/api/v1/servers/${id}`));
  }

  metrics(id: number) {
    return this.http.get<Paged<MetricHistoryDto>>(apiUrl(`/api/v1/servers/${id}/metrics`), {
      params: { page: 1, pageSize: 200 }
    });
  }

  alerts(unacknowledgedOnly = false, serverId?: number) {
    return this.http.get<Paged<AlertDto>>(apiUrl('/api/v1/alerts'), {
      params: this.params({ page: 1, pageSize: 50, unacknowledgedOnly, serverId: serverId ?? '' })
    });
  }

  createServer(body: { name: string; hostName: string; environmentName: string; description: string | null }) {
    return this.http.post<ServerDto>(apiUrl('/api/v1/servers'), body);
  }

  updateServer(id: number, body: { name: string; hostName: string; environmentName: string; description: string | null }) {
    return this.http.put<ServerDto>(apiUrl(`/api/v1/servers/${id}`), body);
  }

  deleteServer(id: number) {
    return this.http.delete(apiUrl(`/api/v1/servers/${id}`));
  }

  acknowledge(id: number) {
    return this.http.post<AlertDto>(apiUrl(`/api/v1/alerts/${id}/acknowledge`), {});
  }

  reports() {
    return this.http.get<Paged<ReportDto>>(apiUrl('/api/v1/reports'), { params: { page: 1, pageSize: 30 } });
  }

  createReport(title: string, fromUtc: string, toUtc: string) {
    return this.http.post<ReportDto>(apiUrl('/api/v1/reports'), { title, fromUtc, toUtc });
  }

  jobs() {
    return this.http.get<JobOverview>(apiUrl('/api/v1/jobs'));
  }

  collectNow() {
    return this.http.post(apiUrl('/api/v1/jobs/collect'), {});
  }

  retryJob(id: string) {
    return this.http.post(apiUrl(`/api/v1/jobs/${encodeURIComponent(id)}/retry`), {});
  }

  prepareHangfire() {
    return this.http.post(apiUrl('/api/v1/jobs/hangfire-session'), {}, { withCredentials: true });
  }

  scheduleMaintenance(serverId: number, reason: string, delayMinutes: number) {
    return this.http.post(apiUrl('/api/v1/jobs/maintenance'), { serverId, reason, delayMinutes });
  }

  users(search = '') {
    return this.http.get<Paged<UserDto>>(apiUrl('/api/v1/users'), { params: this.params({ page: 1, pageSize: 50, search }) });
  }

  createUser(email: string, fullName: string, password: string, roles: string[]) {
    return this.http.post<UserDto>(apiUrl('/api/v1/users'), { email, fullName, password, roles });
  }

  deactivateUser(id: number) {
    return this.http.delete(apiUrl(`/api/v1/users/${id}`));
  }

  private params(values: Record<string, string | number | boolean>) {
    const query: Record<string, string> = {};
    for (const [key, value] of Object.entries(values)) {
      if (value !== '') {
        query[key] = String(value);
      }
    }

    return query;
  }
}

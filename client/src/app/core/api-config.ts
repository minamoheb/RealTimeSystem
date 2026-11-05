import { environment } from '../../environments/environment';

/** Empty origin means the UI and API share a host, so requests stay relative. */
export const apiOrigin = environment.apiOrigin.replace(/\/$/, '');

export function apiUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return `${apiOrigin}${normalized}`;
}

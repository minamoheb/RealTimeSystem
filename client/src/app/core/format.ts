export function formatPercent(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) {
    return '—';
  }

  return `${value.toFixed(1)}%`;
}

export function formatNumber(value: number | null | undefined, suffix = ''): string {
  if (value === null || value === undefined || Number.isNaN(value)) {
    return '—';
  }

  return `${value.toFixed(0)}${suffix}`;
}

export function toneFor(value: number | null | undefined, warnAt = 80, critAt = 90): 'ok' | 'warn' | 'crit' | 'muted' {
  if (value === null || value === undefined) {
    return 'muted';
  }

  if (value >= critAt) {
    return 'crit';
  }

  if (value >= warnAt) {
    return 'warn';
  }

  return 'ok';
}

export function errorMessage(error: unknown, fallback: string): string {
  const body = (error as { error?: { detail?: string; title?: string; errors?: Record<string, string[]> } })?.error;
  const first = body?.errors ? Object.values(body.errors)[0]?.[0] : undefined;
  return first || body?.detail || body?.title || fallback;
}

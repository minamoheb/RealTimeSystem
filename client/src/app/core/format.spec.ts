import { formatNumber, formatPercent, toneFor } from './format';

describe('format helpers', () => {
  it('formats missing numbers as a dash', () => {
    expect(formatPercent(null)).toBe('—');
    expect(formatNumber(undefined, ' ms')).toBe('—');
  });

  it('formats percentages and suffixes', () => {
    expect(formatPercent(12.34)).toBe('12.3%');
    expect(formatNumber(18.2, ' ms')).toBe('18 ms');
  });

  it('picks a tone from thresholds', () => {
    expect(toneFor(10)).toBe('ok');
    expect(toneFor(80)).toBe('warn');
    expect(toneFor(90)).toBe('crit');
    expect(toneFor(null)).toBe('muted');
  });
});

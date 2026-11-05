namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record MetricHistoryDto(
        long Id,
        long ServerId,
        double CpuPercent,
        double MemoryPercent,
        double DiskPercent,
        double NetworkMbps,
        double ResponseTimeMs,
        DateTime CollectedAtUtc);
    public sealed record LiveMetricDto(
    long ServerId,
    string ServerName,
    string Status,
    double CpuPercent,
    double MemoryPercent,
    double DiskPercent,
    double NetworkMbps,
    double ResponseTimeMs,
    DateTime CollectedAtUtc);

}

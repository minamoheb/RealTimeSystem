using RealTimeSystem.Domain.Common;

namespace RealTimeSystem.Domain.Events
{
    public sealed record MetricCollectedDomainEvent(
        long ServerId,
        string ServerName,
        string Status,
        double CpuPercent,
        double MemoryPercent,
        double DiskPercent,
        double NetworkMbps,
        double ResponseTimeMs,
        DateTime CollectedAtUtc) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public sealed record ThresholdBreachedDomainEvent(
        long AlertId,
        long ServerId,
        string ServerName,
        string MetricName,
        double Value,
        double Threshold,
        string Severity,
        string Message,
        DateTime CreatedAtUtc) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public sealed record ReportStatusChangedDomainEvent(
        long ReportId,
        string Title,
        string Status,
        string? Summary,
        DateTime RequestedAtUtc,
        DateTime? CompletedAtUtc,
        string RequestedByEmail) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    public sealed record MaintenanceScheduledDomainEvent(
        long MaintenanceWindowId,
        long ServerId,
        string ServerName,
        string Reason,
        DateTime ScheduledForUtc,
        string Status) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}

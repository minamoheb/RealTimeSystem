namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record AlertDto(
        long Id,
        long ServerId,
        string ServerName,
        string MetricName,
        double Value,
        double Threshold,
        string Severity,
        string Message,
        bool IsAcknowledged,
        DateTime CreatedAtUtc,
        DateTime? AcknowledgedAtUtc);
}

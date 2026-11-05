namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record MaintenanceNoticeDto(
        long Id,
        long ServerId,
        string ServerName,
        string Reason,
        DateTime ScheduledForUtc,
        string Status);

    public sealed record ScheduleMaintenanceRequestDto(long ServerId, string Reason, int DelayMinutes);
}

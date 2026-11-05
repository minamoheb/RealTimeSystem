namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record ServerDto(
        long Id,
        string Name,
        string HostName,
        string EnvironmentName,
        string? Description,
        string Status,
        bool IsActive,
        DateTime CreatedAtUtc,
        double? CpuPercent,
        double? MemoryPercent,
        double? DiskPercent,
        double? NetworkMbps,
        double? ResponseTimeMs,
        DateTime? CollectedAtUtc,
        int OpenAlertCount);
    public sealed record ServerV2Dto(
        long Id,
        string Name,
        string HostName,
        string EnvironmentName,
        string Status,
        int HealthScore,
        double? CpuPercent,
        double? MemoryPercent,
        double? DiskPercent,
        double? ResponseTimeMs,
        int OpenAlertCount);


}


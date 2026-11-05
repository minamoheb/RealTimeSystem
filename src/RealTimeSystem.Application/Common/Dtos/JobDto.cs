namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record JobOverviewDto(
        long Enqueued,
        long Processing,
        long Scheduled,
        long Succeeded,
        long Failed,
        IReadOnlyList<JobListItemDto> Recent);
    public sealed record JobListItemDto(string Id, string Name, string State, DateTime? CreatedAtUtc);
}


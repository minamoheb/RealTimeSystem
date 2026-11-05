namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record ReportDto(
        long Id,
        string Title,
        DateTime FromUtc,
        DateTime ToUtc,
        string Status,
        string? Summary,
        string? FailureReason,
        DateTime RequestedAtUtc,
        DateTime? CompletedAtUtc,
        string RequestedByEmail);
    public sealed record CreateReportRequestDto(string Title, DateTime FromUtc, DateTime ToUtc);
}

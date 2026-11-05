using RealTimeSystem.Domain.Common;
using RealTimeSystem.Domain.Enums;
using RealTimeSystem.Domain.Events;

namespace RealTimeSystem.Domain.Entities
{
    public sealed class Report : BaseEntity, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public string Title { get; set; } = string.Empty;

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.Queued;

        public string? Summary { get; set; }

        public string? StoragePath { get; set; }

        public string? FailureReason { get; set; }

        public long RequestedByUserId { get; set; }

        public string RequestedByEmail { get; set; } = string.Empty;

        public DateTime RequestedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public User RequestedBy { get; set; } = null!;

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        public void MarkGenerating() => SetStatus(ReportStatus.Generating, completed: false);

        public void MarkAnalyzing() => SetStatus(ReportStatus.Analyzing, completed: false);

        public void MarkReady(string summary, DateTime utcNow)
        {
            Summary = summary;
            CompletedAtUtc = utcNow;
            SetStatus(ReportStatus.Ready, completed: true);
        }

        public void MarkFailed(string reason, DateTime utcNow)
        {
            FailureReason = reason;
            CompletedAtUtc = utcNow;
            SetStatus(ReportStatus.Failed, completed: true);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();

        private void SetStatus(ReportStatus status, bool completed)
        {
            Status = status;
            _domainEvents.Add(new ReportStatusChangedDomainEvent(
                Id,
                Title,
                Status.ToString(),
                Summary,
                RequestedAtUtc,
                completed ? CompletedAtUtc : null,
                RequestedByEmail));
        }
    }
}

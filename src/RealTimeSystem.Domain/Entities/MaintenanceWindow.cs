using RealTimeSystem.Domain.Common;
using RealTimeSystem.Domain.Enums;
using RealTimeSystem.Domain.Events;

namespace RealTimeSystem.Domain.Entities
{
    public sealed class MaintenanceWindow : BaseEntity, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public long ServerId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;

        public DateTime ScheduledForUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public MonitoredServer Server { get; set; } = null!;

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        public void NotifyScheduled(string serverName)
        {
            _domainEvents.Add(CreateEvent(serverName));
        }

        public void Complete(DateTime utcNow, string serverName)
        {
            Status = MaintenanceStatus.Completed;
            CompletedAtUtc = utcNow;
            _domainEvents.Add(CreateEvent(serverName));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();

        private MaintenanceScheduledDomainEvent CreateEvent(string serverName) =>
            new(Id, ServerId, serverName, Reason, ScheduledForUtc, Status.ToString());
    }
}

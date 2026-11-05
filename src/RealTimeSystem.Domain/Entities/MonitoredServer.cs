using RealTimeSystem.Domain.Common;
using RealTimeSystem.Domain.Enums;
using RealTimeSystem.Domain.Events;

namespace RealTimeSystem.Domain.Entities
{
    public sealed class MonitoredServer : BaseEntity, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public string Name { get; set; } = string.Empty;

        public string HostName { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ServerStatus Status { get; set; } = ServerStatus.Healthy;

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<MetricSample> Metrics { get; set; } = new List<MetricSample>();

        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

        public ICollection<MaintenanceWindow> MaintenanceWindows { get; set; } = new List<MaintenanceWindow>();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        public void RecordSample(MetricSample sample)
        {
            PublishSample(sample);
        }

        public void PublishSample(MetricSample sample)
        {
            _domainEvents.Add(new MetricCollectedDomainEvent(
                Id,
                Name,
                Status.ToString(),
                sample.CpuPercent,
                sample.MemoryPercent,
                sample.DiskPercent,
                sample.NetworkMbps,
                sample.ResponseTimeMs,
                sample.CollectedAtUtc));
        }

        public void RaiseAlert(Alert alert)
        {
            Alerts.Add(alert);
            NoteAlert(alert);
        }

        public void NoteAlert(Alert alert)
        {
            if (alert.Severity != AlertSeverity.Info &&
                Status is not (ServerStatus.Maintenance or ServerStatus.Offline))
            {
                Status = ServerStatus.Degraded;
            }

            _domainEvents.Add(new ThresholdBreachedDomainEvent(
                alert.Id,
                Id,
                Name,
                alert.MetricName,
                alert.Value,
                alert.Threshold,
                alert.Severity.ToString(),
                alert.Message,
                alert.CreatedAtUtc));
        }

        public void MarkHealthy()
        {
            if (Status is ServerStatus.Maintenance or ServerStatus.Offline)
            {
                return;
            }

            Status = ServerStatus.Healthy;
        }

        public void ResumeFromMaintenance()
        {
            if (Status == ServerStatus.Maintenance)
            {
                Status = ServerStatus.Healthy;
            }
        }

        public void MarkMaintenance()
        {
            if (IsActive)
            {
                Status = ServerStatus.Maintenance;
            }
        }

        public void Deactivate()
        {
            IsActive = false;
            Status = ServerStatus.Offline;
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}

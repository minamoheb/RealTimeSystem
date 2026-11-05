using RealTimeSystem.Domain.Enums;

namespace RealTimeSystem.Domain.Entities
{
    public sealed class Alert : BaseEntity
    {
        public long ServerId { get; set; }

        public string MetricName { get; set; } = string.Empty;

        public double Value { get; set; }

        public double Threshold { get; set; }

        public AlertSeverity Severity { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsAcknowledged { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? AcknowledgedAtUtc { get; set; }

        public long? AcknowledgedByUserId { get; set; }

        public MonitoredServer Server { get; set; } = null!;

        public void Acknowledge(long userId, DateTime utcNow)
        {
            if (IsAcknowledged)
            {
                return;
            }

            IsAcknowledged = true;
            AcknowledgedByUserId = userId;
            AcknowledgedAtUtc = utcNow;
        }
    }
}

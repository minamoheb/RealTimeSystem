namespace RealTimeSystem.Domain.Entities
{
    public sealed class MetricSample : BaseEntity
    {
        public long ServerId { get; set; }

        public double CpuPercent { get; set; }

        public double MemoryPercent { get; set; }

        public double DiskPercent { get; set; }

        public double NetworkMbps { get; set; }

        public double ResponseTimeMs { get; set; }

        public DateTime CollectedAtUtc { get; set; }

        public MonitoredServer Server { get; set; } = null!;
    }
}

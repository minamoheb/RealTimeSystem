using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Entities;

namespace RealTimeSystem.Infrastructure.Services.Monitoring
{
    public sealed class SimulatedMetricSampleFactory : IMetricSampleFactory
    {
        public MetricSample Create(long serverId, MetricSample? previous, DateTime utcNow)
        {
            return new MetricSample
            {
                ServerId = serverId,
                CpuPercent = Walk(previous?.CpuPercent, 8, 96, 7, 0.08),
                MemoryPercent = Walk(previous?.MemoryPercent, 25, 97, 3.5, 0.05),
                DiskPercent = Walk(previous?.DiskPercent, 20, 98, 1.2, 0.03),
                NetworkMbps = Walk(previous?.NetworkMbps, 5, 900, 35, 0.06),
                ResponseTimeMs = Walk(previous?.ResponseTimeMs, 20, 1400, 45, 0.07),
                CollectedAtUtc = utcNow
            };
        }

        private static double Walk(double? current, double min, double max, double drift, double spikeChance)
        {
            var baseline = current ?? ((min + max) / 3);
            var value = baseline + (Random.Shared.NextDouble() * 2 - 1) * drift;
            if (Random.Shared.NextDouble() < spikeChance)
            {
                value += (max - min) * 0.22;
            }

            return Math.Round(Math.Clamp(value, min, max), 2);
        }
    }
}

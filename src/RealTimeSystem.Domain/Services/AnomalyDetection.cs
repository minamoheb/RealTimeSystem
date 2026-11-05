using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Domain.Enums;
using RealTimeSystem.Domain.Exceptions;

namespace RealTimeSystem.Domain.Services
{
    public interface IAnomalyRule
    {
        string MetricName { get; }

        Alert? Evaluate(MonitoredServer server, MetricSample sample, DateTime utcNow);
    }

    public class ThresholdAnomalyRule : IAnomalyRule
    {
        private readonly Func<MetricSample, double> _selector;
        private readonly double _warning;
        private readonly double _critical;
        private readonly string _unit;

        public ThresholdAnomalyRule(
            string metricName,
            Func<MetricSample, double> selector,
            double warning,
            double critical,
            string unit)
        {
            if (string.IsNullOrWhiteSpace(metricName))
            {
                throw new DomainException("Metric name is required.");
            }

            if (warning <= 0 || critical <= 0)
            {
                throw new DomainException("Thresholds must be positive.");
            }

            if (warning > critical)
            {
                throw new DomainException("Warning threshold cannot exceed the critical threshold.");
            }

            MetricName = metricName;
            _selector = selector;
            _warning = warning;
            _critical = critical;
            _unit = unit;
        }

        public string MetricName { get; }

        public Alert? Evaluate(MonitoredServer server, MetricSample sample, DateTime utcNow)
        {
            var value = _selector(sample);
            if (value < _warning)
            {
                return null;
            }

            var severity = value >= _critical ? AlertSeverity.Critical : AlertSeverity.Warning;
            var threshold = severity == AlertSeverity.Critical ? _critical : _warning;

            return new Alert
            {
                ServerId = server.Id,
                MetricName = MetricName,
                Value = Math.Round(value, 2),
                Threshold = threshold,
                Severity = severity,
                Message = $"{server.Name} {MetricName} is {value:0.##}{_unit}, above the {severity.ToString().ToLowerInvariant()} threshold of {threshold:0.##}{_unit}.",
                CreatedAtUtc = utcNow
            };
        }
    }

    public sealed class CpuAnomalyRule : ThresholdAnomalyRule
    {
        public CpuAnomalyRule(double warning, double critical)
            : base("CPU", sample => sample.CpuPercent, warning, critical, "%")
        {
        }
    }

    public sealed class MemoryAnomalyRule : ThresholdAnomalyRule
    {
        public MemoryAnomalyRule(double warning, double critical)
            : base("Memory", sample => sample.MemoryPercent, warning, critical, "%")
        {
        }
    }

    public sealed class DiskAnomalyRule : ThresholdAnomalyRule
    {
        public DiskAnomalyRule(double warning, double critical)
            : base("Disk", sample => sample.DiskPercent, warning, critical, "%")
        {
        }
    }

    public sealed class ResponseTimeAnomalyRule : ThresholdAnomalyRule
    {
        public ResponseTimeAnomalyRule(double warning, double critical)
            : base("ResponseTime", sample => sample.ResponseTimeMs, warning, critical, " ms")
        {
        }
    }

    public sealed class AnomalyDetectionService
    {
        private readonly IReadOnlyList<IAnomalyRule> _rules;

        public AnomalyDetectionService(IEnumerable<IAnomalyRule> rules)
        {
            _rules = rules.ToList();
        }

        public IReadOnlyList<Alert> Evaluate(MonitoredServer server, MetricSample sample, DateTime utcNow) =>
            _rules
                .Select(rule => rule.Evaluate(server, sample, utcNow))
                .Where(alert => alert is not null)
                .Cast<Alert>()
                .ToList();
    }

    public static class HealthScoreCalculator
    {
        public static int Calculate(double? cpu, double? memory, double? disk, double? responseTimeMs)
        {
            var penalty = 0d;
            if (cpu is > 0)
            {
                penalty += Math.Max(0, cpu.Value - 50) * 0.45;
            }

            if (memory is > 0)
            {
                penalty += Math.Max(0, memory.Value - 50) * 0.35;
            }

            if (disk is > 0)
            {
                penalty += Math.Max(0, disk.Value - 60) * 0.25;
            }

            if (responseTimeMs is > 0)
            {
                penalty += Math.Max(0, (responseTimeMs.Value - 120) / 25d);
            }

            return (int)Math.Clamp(Math.Round(100 - penalty), 0, 100);
        }
    }
}

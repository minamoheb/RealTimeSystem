using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Domain.Enums;
using RealTimeSystem.Domain.Exceptions;
using RealTimeSystem.Domain.Services;

namespace RealTimeSystem.Domain.Tests;

public class AnomalyDetectionTests
{
    private static MonitoredServer Server() => new() { Name = "api-core-01" };

    private static MetricSample Sample(double cpu = 10, double memory = 10, double disk = 10, double response = 50) =>
        new()
        {
            CpuPercent = cpu,
            MemoryPercent = memory,
            DiskPercent = disk,
            ResponseTimeMs = response
        };

    [Fact]
    public void Evaluate_ReturnsNoAlert_WhenValueIsBelowWarning()
    {
        var rule = new CpuAnomalyRule(80, 90);

        var alert = rule.Evaluate(Server(), Sample(cpu: 79.9), DateTime.UtcNow);

        Assert.Null(alert);
    }

    [Fact]
    public void Evaluate_ReturnsWarning_WhenValueMeetsWarningThreshold()
    {
        var rule = new MemoryAnomalyRule(80, 90);

        var alert = rule.Evaluate(Server(), Sample(memory: 80), DateTime.UtcNow);

        Assert.NotNull(alert);
        Assert.Equal(AlertSeverity.Warning, alert.Severity);
        Assert.Equal(80, alert.Threshold);
        Assert.Equal("Memory", alert.MetricName);
    }

    [Fact]
    public void Evaluate_ReturnsCritical_WhenValueMeetsCriticalThreshold()
    {
        var rule = new ResponseTimeAnomalyRule(400, 800);

        var alert = rule.Evaluate(Server(), Sample(response: 800), DateTime.UtcNow);

        Assert.NotNull(alert);
        Assert.Equal(AlertSeverity.Critical, alert.Severity);
        Assert.Contains("api-core-01", alert.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_RejectsWarningAboveCritical()
    {
        var exception = Assert.Throws<DomainException>(() => new DiskAnomalyRule(99, 90));

        Assert.Contains("Warning threshold", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Service_ReturnsEveryBreachedRule()
    {
        var service = new AnomalyDetectionService(
        [
            new CpuAnomalyRule(80, 90),
            new MemoryAnomalyRule(80, 90),
            new DiskAnomalyRule(85, 95)
        ]);

        var alerts = service.Evaluate(Server(), Sample(cpu: 91, memory: 10, disk: 86), DateTime.UtcNow);

        Assert.Equal(2, alerts.Count);
        Assert.Contains(alerts, alert => alert.MetricName == "CPU" && alert.Severity == AlertSeverity.Critical);
        Assert.Contains(alerts, alert => alert.MetricName == "Disk" && alert.Severity == AlertSeverity.Warning);
    }

    [Fact]
    public void HealthScore_DropsAsUtilizationRises()
    {
        var calm = HealthScoreCalculator.Calculate(10, 10, 10, 20);
        var stressed = HealthScoreCalculator.Calculate(95, 95, 95, 1000);

        Assert.Equal(100, calm);
        Assert.InRange(stressed, 0, 40);
    }
}

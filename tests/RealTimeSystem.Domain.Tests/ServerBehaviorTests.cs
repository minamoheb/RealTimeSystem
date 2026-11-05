using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Domain.Enums;
using RealTimeSystem.Domain.Events;

namespace RealTimeSystem.Domain.Tests;

public class ServerBehaviorTests
{
    [Fact]
    public void RaiseAlert_MarksServerDegraded_AndRecordsEvent()
    {
        var server = new MonitoredServer { Name = "edge-gateway-01", Status = ServerStatus.Healthy };
        var alert = new Alert
        {
            MetricName = "CPU",
            Value = 91,
            Threshold = 90,
            Severity = AlertSeverity.Critical,
            Message = "CPU high",
            CreatedAtUtc = DateTime.UtcNow
        };

        server.RaiseAlert(alert);

        Assert.Equal(ServerStatus.Degraded, server.Status);
        Assert.Contains(server.Alerts, candidate => candidate.Id == alert.Id);
        Assert.Contains(server.DomainEvents, domainEvent => domainEvent is ThresholdBreachedDomainEvent);
    }

    [Fact]
    public void Acknowledge_IsIdempotent()
    {
        var alert = new Alert { CreatedAtUtc = DateTime.UtcNow };
        const long userId = 1;
        var first = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);

        alert.Acknowledge(userId, first);
        alert.Acknowledge(2, first.AddMinutes(5));

        Assert.True(alert.IsAcknowledged);
        Assert.Equal(userId, alert.AcknowledgedByUserId);
        Assert.Equal(first, alert.AcknowledgedAtUtc);
    }

    [Fact]
    public void Deactivate_TakesServerOffline()
    {
        var server = new MonitoredServer { IsActive = true, Status = ServerStatus.Healthy };

        server.Deactivate();

        Assert.False(server.IsActive);
        Assert.Equal(ServerStatus.Offline, server.Status);
    }

    [Fact]
    public void Report_MarkReady_RaisesStatusEvent()
    {
        var report = new Report { Title = "Daily", RequestedByEmail = "admin@monitor.local", RequestedAtUtc = DateTime.UtcNow };

        report.MarkReady("All clear", DateTime.UtcNow);

        Assert.Equal(ReportStatus.Ready, report.Status);
        Assert.Equal("All clear", report.Summary);
        var domainEvent = Assert.IsType<ReportStatusChangedDomainEvent>(Assert.Single(report.DomainEvents));
        Assert.Equal("Ready", domainEvent.Status);
    }
}

namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface IMonitoringNotifier
    {
        Task MetricUpdatedAsync(LiveMetricDto metric, CancellationToken cancellationToken);

        Task AlertRaisedAsync(AlertDto alert, CancellationToken cancellationToken);

        Task ReportUpdatedAsync(ReportDto report, CancellationToken cancellationToken);

        Task MaintenanceScheduledAsync(MaintenanceNoticeDto notice, CancellationToken cancellationToken);
    }
}


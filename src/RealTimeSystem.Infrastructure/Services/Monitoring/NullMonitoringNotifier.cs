using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.Services.Monitoring
{
    public sealed class NullMonitoringNotifier : IMonitoringNotifier
    {
        public Task MetricUpdatedAsync(LiveMetricDto metric, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task AlertRaisedAsync(AlertDto alert, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ReportUpdatedAsync(ReportDto report, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task MaintenanceScheduledAsync(MaintenanceNoticeDto notice, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

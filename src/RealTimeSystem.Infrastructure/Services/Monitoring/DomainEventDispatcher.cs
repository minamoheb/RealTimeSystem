using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Common;
using RealTimeSystem.Domain.Events;

namespace RealTimeSystem.Infrastructure.Services.Monitoring
{
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMonitoringNotifier _notifier;

        public DomainEventDispatcher(IMonitoringNotifier notifier)
        {
            _notifier = notifier;
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            foreach (var domainEvent in domainEvents)
            {
                switch (domainEvent)
                {
                    case MetricCollectedDomainEvent metric:
                        await _notifier.MetricUpdatedAsync(
                            new LiveMetricDto(
                                metric.ServerId,
                                metric.ServerName,
                                metric.Status,
                                metric.CpuPercent,
                                metric.MemoryPercent,
                                metric.DiskPercent,
                                metric.NetworkMbps,
                                metric.ResponseTimeMs,
                                metric.CollectedAtUtc),
                            cancellationToken);
                        break;
                    case ThresholdBreachedDomainEvent alert:
                        await _notifier.AlertRaisedAsync(
                            new AlertDto(
                                alert.AlertId,
                                alert.ServerId,
                                alert.ServerName,
                                alert.MetricName,
                                alert.Value,
                                alert.Threshold,
                                alert.Severity,
                                alert.Message,
                                false,
                                alert.CreatedAtUtc,
                                null),
                            cancellationToken);
                        break;
                    case ReportStatusChangedDomainEvent report:
                        await _notifier.ReportUpdatedAsync(
                            new ReportDto(
                                report.ReportId,
                                report.Title,
                                DateTime.MinValue,
                                DateTime.MinValue,
                                report.Status,
                                report.Summary,
                                null,
                                report.RequestedAtUtc,
                                report.CompletedAtUtc,
                                report.RequestedByEmail),
                            cancellationToken);
                        break;
                    case MaintenanceScheduledDomainEvent maintenance:
                        await _notifier.MaintenanceScheduledAsync(
                            new MaintenanceNoticeDto(
                                maintenance.MaintenanceWindowId,
                                maintenance.ServerId,
                                maintenance.ServerName,
                                maintenance.Reason,
                                maintenance.ScheduledForUtc,
                                maintenance.Status),
                            cancellationToken);
                        break;
                }
            }
        }
    }
}

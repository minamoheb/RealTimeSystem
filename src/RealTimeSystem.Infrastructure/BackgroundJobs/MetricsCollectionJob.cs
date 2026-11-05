using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Services;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class MetricsCollectionJob
    {
        private readonly IApplicationDbContext _db;
        private readonly IMetricSampleFactory _factory;
        private readonly AnomalyDetectionService _detector;
        private readonly IDateTimeProvider _clock;
        private readonly ILogger<MetricsCollectionJob> _logger;

        public MetricsCollectionJob(
            IApplicationDbContext db,
            IMetricSampleFactory factory,
            AnomalyDetectionService detector,
            IDateTimeProvider clock,
            ILogger<MetricsCollectionJob> logger)
        {
            _db = db;
            _factory = factory;
            _detector = detector;
            _clock = clock;
            _logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 120)]
        public async Task Execute()
        {
            var startDate = _clock.UtcNow;
            var now = startDate;
            var servers = await _db.Servers.Where(server => server.IsActive).ToListAsync();
            foreach (var server in servers)
            {
                var previous = await _db.Metrics.AsNoTracking()
                    .Where(metric => metric.ServerId == server.Id)
                    .OrderByDescending(metric => metric.CollectedAtUtc)
                    .FirstOrDefaultAsync();
                var sample = _factory.Create(server.Id, previous, now);
                var detected = _detector.Evaluate(server, sample, now);
                var recent = await _db.Alerts.AsNoTracking()
                    .Where(alert => alert.ServerId == server.Id && !alert.IsAcknowledged && alert.CreatedAtUtc > now.AddMinutes(-15))
                    .Select(alert => alert.MetricName)
                    .ToListAsync();
                var freshAlerts = detected.Where(alert => !recent.Contains(alert.MetricName)).ToList();

                if (freshAlerts.Count > 0)
                {
                    foreach (var alert in freshAlerts)
                    {
                        _db.Alerts.Add(alert);
                        server.NoteAlert(alert);
                    }
                }
                else if (detected.Count == 0)
                {
                    server.MarkHealthy();
                }

                _db.Metrics.Add(sample);
                server.PublishSample(sample);
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "Collected metrics for {ServerCount} servers. StartDate {StartDate:o} EndDate {EndDate:o}",
                servers.Count,
                startDate,
                _clock.UtcNow);
        }
    }
}

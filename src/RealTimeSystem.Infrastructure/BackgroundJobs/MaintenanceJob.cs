using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Domain.Enums;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class MaintenanceJob
    {
        private readonly IApplicationDbContext _db;
        private readonly IDateTimeProvider _clock;
        private readonly ILogger<MaintenanceJob> _logger;

        public MaintenanceJob(IApplicationDbContext db, IDateTimeProvider clock, ILogger<MaintenanceJob> logger)
        {
            _db = db;
            _clock = clock;
            _logger = logger;
        }

        public async Task Execute(long maintenanceWindowId)
        {
            var window = await _db.MaintenanceWindows.Include(candidate => candidate.Server)
                .FirstOrDefaultAsync(candidate => candidate.Id == maintenanceWindowId);
            if (window is null || window.Status == MaintenanceStatus.Completed)
            {
                return;
            }

            var now = _clock.UtcNow;
            window.Server.MarkMaintenance();
            var alert = new Alert
            {
                ServerId = window.ServerId,
                MetricName = "Maintenance",
                Value = 0,
                Threshold = 0,
                Severity = AlertSeverity.Info,
                Message = $"Scheduled maintenance executed for {window.Server.Name}: {window.Reason}",
                CreatedAtUtc = now
            };
            _db.Alerts.Add(alert);
            window.Server.NoteAlert(alert);
            window.Server.ResumeFromMaintenance();
            window.Complete(now, window.Server.Name);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Completed maintenance window {MaintenanceWindowId}", maintenanceWindowId);
        }
    }
}

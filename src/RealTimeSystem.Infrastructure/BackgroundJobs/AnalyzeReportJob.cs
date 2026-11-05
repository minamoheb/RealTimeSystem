using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Enums;
using System.Text.Json;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class AnalyzeReportJob
    {
        private readonly IApplicationDbContext _db;
        private readonly IDateTimeProvider _clock;
        private readonly ILogger<AnalyzeReportJob> _logger;

        public AnalyzeReportJob(IApplicationDbContext db, IDateTimeProvider clock, ILogger<AnalyzeReportJob> logger)
        {
            _db = db;
            _clock = clock;
            _logger = logger;
        }

        public async Task Execute(long reportId)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(candidate => candidate.Id == reportId);
            if (report is null || report.Status == ReportStatus.Failed)
            {
                return;
            }

            report.MarkAnalyzing();
            await _db.SaveChangesAsync();

            var alertCount = await _db.Alerts.CountAsync(alert => alert.CreatedAtUtc >= report.FromUtc && alert.CreatedAtUtc <= report.ToUtc);
            string summary;
            if (string.IsNullOrWhiteSpace(report.StoragePath) || !File.Exists(report.StoragePath))
            {
                summary = $"Analysis completed with {alertCount} alerts in the selected window. The generated file was not available.";
            }
            else
            {
                using var document = JsonDocument.Parse(await File.ReadAllTextAsync(report.StoragePath));
                var servers = document.RootElement.GetProperty("servers");
                var busiest = servers.EnumerateArray()
                    .Select(server => new
                    {
                        Name = server.GetProperty("server").GetString(),
                        Cpu = server.GetProperty("avgCpu").GetDouble()
                    })
                    .OrderByDescending(server => server.Cpu)
                    .FirstOrDefault();
                summary = busiest is null
                    ? $"No samples were found between {report.FromUtc:u} and {report.ToUtc:u}. Alerts recorded: {alertCount}."
                    : $"Busiest server was {busiest.Name} at {busiest.Cpu:0.##}% average CPU. {servers.GetArrayLength()} servers were included and {alertCount} alerts fell inside the window.";
            }

            report.MarkReady(summary, _clock.UtcNow);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Finished analysis continuation for {ReportId}", reportId);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealTimeSystem.Application.Common.Interfaces;
using System.Text.Json;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class GenerateReportJob
    {
        private readonly IApplicationDbContext _db;
        private readonly IHostEnvironment _environment;
        private readonly IDateTimeProvider _clock;
        private readonly ILogger<GenerateReportJob> _logger;

        public GenerateReportJob(
            IApplicationDbContext db,
            IHostEnvironment environment,
            IDateTimeProvider clock,
            ILogger<GenerateReportJob> logger)
        {
            _db = db;
            _environment = environment;
            _clock = clock;
            _logger = logger;
        }

        public async Task Execute(long reportId)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(candidate => candidate.Id == reportId);
            if (report is null)
            {
                return;
            }

            try
            {
                report.MarkGenerating();
                await _db.SaveChangesAsync();

                var rows = await _db.Metrics.AsNoTracking()
                    .Where(metric => metric.CollectedAtUtc >= report.FromUtc && metric.CollectedAtUtc <= report.ToUtc)
                    .Join(
                        _db.Servers.AsNoTracking(),
                        metric => metric.ServerId,
                        server => server.Id,
                        (metric, server) => new
                        {
                            server.Name,
                            metric.CpuPercent,
                            metric.MemoryPercent,
                            metric.DiskPercent,
                            metric.NetworkMbps,
                            metric.ResponseTimeMs
                        })
                    .ToListAsync();

                var servers = rows
                    .GroupBy(row => row.Name)
                    .Select(group => new
                    {
                        server = group.Key,
                        samples = group.Count(),
                        avgCpu = Math.Round(group.Average(row => row.CpuPercent), 2),
                        maxCpu = Math.Round(group.Max(row => row.CpuPercent), 2),
                        avgMemory = Math.Round(group.Average(row => row.MemoryPercent), 2),
                        avgDisk = Math.Round(group.Average(row => row.DiskPercent), 2),
                        avgNetwork = Math.Round(group.Average(row => row.NetworkMbps), 2),
                        avgResponseMs = Math.Round(group.Average(row => row.ResponseTimeMs), 2)
                    })
                    .OrderBy(row => row.server)
                    .ToList();

                var directory = Path.Combine(_environment.ContentRootPath, "App_Data", "reports");
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, $"{report.Id}.json");
                var payload = new
                {
                    report.Title,
                    report.FromUtc,
                    report.ToUtc,
                    generatedAtUtc = _clock.UtcNow,
                    servers
                };
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
                report.StoragePath = path;
                await _db.SaveChangesAsync();
                _logger.LogInformation("Generated report file for {ReportId}", reportId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Report generation failed for {ReportId}", reportId);
                report.MarkFailed(exception.Message, _clock.UtcNow);
                await _db.SaveChangesAsync();
                throw;
            }
        }
    }
}

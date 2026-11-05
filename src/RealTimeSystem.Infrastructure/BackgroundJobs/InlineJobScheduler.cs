using Microsoft.Extensions.DependencyInjection;
using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class InlineJobScheduler : IBackgroundJobScheduler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public InlineJobScheduler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void EnqueueReportPipeline(long reportId)
        {
            using var scope = _scopeFactory.CreateScope();
            var generate = scope.ServiceProvider.GetRequiredService<GenerateReportJob>();
            var analyze = scope.ServiceProvider.GetRequiredService<AnalyzeReportJob>();
            generate.Execute(reportId).GetAwaiter().GetResult();
            analyze.Execute(reportId).GetAwaiter().GetResult();
        }

        public void ScheduleMaintenance(long maintenanceWindowId, TimeSpan delay)
        {
            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<MaintenanceJob>();
            job.Execute(maintenanceWindowId).GetAwaiter().GetResult();
        }

        public void EnqueueMetricCollection()
        {
            using var scope = _scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<MetricsCollectionJob>();
            job.Execute().GetAwaiter().GetResult();
        }

        public bool Requeue(string jobId) => false;
    }
}

using Hangfire;
using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class HangfireJobScheduler : IBackgroundJobScheduler
    {
        public void EnqueueReportPipeline(long reportId)
        {
            var parentId = BackgroundJob.Enqueue<GenerateReportJob>(job => job.Execute(reportId));
            BackgroundJob.ContinueJobWith<AnalyzeReportJob>(parentId, job => job.Execute(reportId));
        }

        public void ScheduleMaintenance(long maintenanceWindowId, TimeSpan delay)
        {
            BackgroundJob.Schedule<MaintenanceJob>(job => job.Execute(maintenanceWindowId), delay);
        }

        public void EnqueueMetricCollection() =>
            BackgroundJob.Enqueue<MetricsCollectionJob>(job => job.Execute());

        public bool Requeue(string jobId)
        {
            try
            {
                return BackgroundJob.Requeue(jobId);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

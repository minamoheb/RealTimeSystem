namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface IBackgroundJobScheduler
    {
        void EnqueueReportPipeline(long reportId);

        void ScheduleMaintenance(long maintenanceWindowId, TimeSpan delay);

        void EnqueueMetricCollection();

        bool Requeue(string jobId);
    }
}

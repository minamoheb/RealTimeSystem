using Hangfire;
using Hangfire.Storage.Monitoring;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class HangfireJobMonitoringReader : IJobMonitoringReader
    {
        public JobOverviewDto GetOverview()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var statistics = monitor.GetStatistics();
            var recent = new List<JobListItemDto>();
            recent.AddRange(Read(monitor.ProcessingJobs(0, 10), "Processing"));
            recent.AddRange(Read(monitor.ScheduledJobs(0, 10), "Scheduled"));
            recent.AddRange(Read(monitor.SucceededJobs(0, 10), "Succeeded"));
            recent.AddRange(Read(monitor.FailedJobs(0, 10), "Failed"));

            return new JobOverviewDto(
                statistics.Enqueued,
                statistics.Processing,
                statistics.Scheduled,
                statistics.Succeeded,
                statistics.Failed,
                recent.Take(20).ToList());
        }

        private static IEnumerable<JobListItemDto> Read<T>(JobList<T> jobs, string state)
            where T : class
        {
            foreach (var job in jobs)
            {
                var name = job.Value switch
                {
                    ProcessingJobDto processing => processing.Job?.Type.Name ?? "Job",
                    ScheduledJobDto scheduled => scheduled.Job?.Type.Name ?? "Job",
                    SucceededJobDto succeeded => succeeded.Job?.Type.Name ?? "Job",
                    FailedJobDto failed => failed.Job?.Type.Name ?? "Job",
                    _ => "Job"
                };
                DateTime? created = job.Value switch
                {
                    ProcessingJobDto processing => processing.StartedAt,
                    ScheduledJobDto scheduled => scheduled.EnqueueAt,
                    SucceededJobDto succeeded => succeeded.SucceededAt,
                    FailedJobDto failed => failed.FailedAt,
                    _ => null
                };
                yield return new JobListItemDto(job.Key, name, state, created);
            }
        }
    }
}

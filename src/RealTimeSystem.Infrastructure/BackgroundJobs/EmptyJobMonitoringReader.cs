using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
    public sealed class EmptyJobMonitoringReader : IJobMonitoringReader
    {
        public JobOverviewDto GetOverview() => new(0, 0, 0, 0, 0, []);
    }
}

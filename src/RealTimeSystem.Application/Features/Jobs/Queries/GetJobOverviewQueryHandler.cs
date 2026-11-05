namespace RealTimeSystem.Application.Features.Jobs
{
    public sealed record GetJobOverviewQuery : IRequest<JobOverviewDto>;
    public sealed class GetJobOverviewQueryHandler(IJobMonitoringReader reader) : IRequestHandler<GetJobOverviewQuery, JobOverviewDto>
    {
        public Task<JobOverviewDto> Handle(GetJobOverviewQuery request, CancellationToken cancellationToken) =>
            Task.FromResult(reader.GetOverview());
    }
}


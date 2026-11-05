namespace RealTimeSystem.Application.Features.Metrics
{
    public sealed record GetLatestMetricsQuery : Pagination, IRequest<PagedResult<LiveMetricDto>>
    {
        public GetLatestMetricsQuery()
        {
            PageSize = 20;
        }
    }
    public sealed class GetLatestMetricsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetLatestMetricsQuery, PagedResult<LiveMetricDto>>
    {
        public async Task<PagedResult<LiveMetricDto>> Handle(GetLatestMetricsQuery request, CancellationToken cancellationToken)
        {
            var serversRepository = unitOfWork.Repository<MonitoredServer>();
            var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize, maxPageSize: 1000);

            var total = await serversRepository.CountAsync(
                predicate: server => server.IsActive && server.Metrics.Any(),
                cancellationToken: cancellationToken);

            var items = await serversRepository.SelectListAsync(
                selector: server => new LiveMetricDto(
                    server.Id,
                    server.Name,
                    server.Status.ToString(),
                    server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => metric.CpuPercent).FirstOrDefault(),
                    server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => metric.MemoryPercent).FirstOrDefault(),
                    server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => metric.DiskPercent).FirstOrDefault(),
                    server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => metric.NetworkMbps).FirstOrDefault(),
                    server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => metric.ResponseTimeMs).FirstOrDefault(),
                    server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => metric.CollectedAtUtc).FirstOrDefault()),
                predicate: server => server.IsActive && server.Metrics.Any(),
                orderBy: query => query.OrderBy(server => server.Name),
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            return PagedResult<LiveMetricDto>.Create(items, page, pageSize, total);
        }
    }
}

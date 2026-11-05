using System.Linq.Expressions;

namespace RealTimeSystem.Application.Features.Metrics
{
    public sealed record GetServerMetricsQuery : Pagination, IRequest<PagedResult<MetricHistoryDto>>
    {
        public long ServerId { get; init; }

        public DateTime? FromUtc { get; init; }

        public DateTime? ToUtc { get; init; }

        public GetServerMetricsQuery()
        {
            PageSize = 200;
        }
    }
    public sealed class GetServerMetricsQueryHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IRequestHandler<GetServerMetricsQuery, PagedResult<MetricHistoryDto>>
    {
        public async Task<PagedResult<MetricHistoryDto>> Handle(GetServerMetricsQuery request, CancellationToken cancellationToken)
        {
            var servers = unitOfWork.Repository<MonitoredServer>();
            var metrics = unitOfWork.Repository<MetricSample>();
            var exists = await servers.AnyAsync(server => server.Id == request.ServerId, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException(nameof(MonitoredServer), request.ServerId);
            }

            var to = request.ToUtc ?? clock.UtcNow;
            var from = request.FromUtc ?? to.AddHours(-24);
            var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize, maxPageSize: 1000);

            Expression<Func<MetricSample, bool>> predicate = metric =>
                metric.ServerId == request.ServerId &&
                metric.CollectedAtUtc >= from &&
                metric.CollectedAtUtc <= to;

            var total = await metrics.CountAsync(predicate, cancellationToken);
            var items = await metrics.SelectListAsync(
                selector: metric => new MetricHistoryDto(
                    metric.Id,
                    metric.ServerId,
                    metric.CpuPercent,
                    metric.MemoryPercent,
                    metric.DiskPercent,
                    metric.NetworkMbps,
                    metric.ResponseTimeMs,
                    metric.CollectedAtUtc),
                predicate: predicate,
                orderBy: query => query.OrderBy(metric => metric.CollectedAtUtc),
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            return PagedResult<MetricHistoryDto>.Create(items, page, pageSize, total);
        }
    }
}

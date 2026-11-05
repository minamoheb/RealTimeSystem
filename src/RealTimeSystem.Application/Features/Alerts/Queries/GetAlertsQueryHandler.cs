using RealTimeSystem.Application.Common.Mapping;
using RealTimeSystem.Domain.Enums;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Features.Alerts
{
    public sealed record GetAlertsQuery : Pagination, IRequest<PagedResult<AlertDto>>
    {
        public long? ServerId { get; init; }

        public string? Severity { get; init; }

        public bool UnacknowledgedOnly { get; init; }

        public GetAlertsQuery()
        {
            PageSize = 20;
        }
    }
    public sealed class GetAlertsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAlertsQuery, PagedResult<AlertDto>>
    {
        public async Task<PagedResult<AlertDto>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
        {
            var alertsRepository = unitOfWork.Repository<Alert>();
            var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
            var severity = default(AlertSeverity);
            var hasSeverity = !string.IsNullOrWhiteSpace(request.Severity) &&
                Enum.TryParse<AlertSeverity>(request.Severity, true, out severity);
            var hasServerId = request.ServerId.HasValue;
            var serverId = request.ServerId ?? 0;

            Expression<Func<Alert, bool>> predicate = alert =>
                (!hasServerId || alert.ServerId == serverId) &&
                (!request.UnacknowledgedOnly || !alert.IsAcknowledged) &&
                (!hasSeverity || alert.Severity == severity);

            var total = await alertsRepository.CountAsync(predicate, cancellationToken);
            var alerts = await alertsRepository.SelectListAsync(
                selector: AlertMappings.AlertDtoProjection,
                predicate: predicate,
                orderBy: query => query.OrderByDescending(alert => alert.CreatedAtUtc),
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            return PagedResult<AlertDto>.Create(alerts, page, pageSize, total);
        }
    }
}

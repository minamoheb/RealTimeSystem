using RealTimeSystem.Application.Common.Mapping;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Features.Reports
{
    public sealed record GetReportsQuery : Pagination, IRequest<PagedResult<ReportDto>>
    {
        public string? Status { get; init; }

        public GetReportsQuery()
        {
            PageSize = 20;
        }
    }
    public sealed class GetReportsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
    {
        public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
        {
            var reportsRepository = unitOfWork.Repository<Report>();
            var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
            var status = default(Domain.Enums.ReportStatus);
            var hasStatus = !string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<Domain.Enums.ReportStatus>(request.Status, true, out status);

            Expression<Func<Report, bool>> predicate = report => !hasStatus || report.Status == status;

            var total = await reportsRepository.CountAsync(predicate, cancellationToken);
            var reports = await reportsRepository.SelectListAsync(
                selector: ReportMappings.ReportDtoProjection,
                predicate: predicate,
                orderBy: query => query.OrderByDescending(report => report.RequestedAtUtc),
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            return PagedResult<ReportDto>.Create(reports, page, pageSize, total);
        }
    }
}

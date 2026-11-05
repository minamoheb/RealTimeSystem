using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Reports
{
    public sealed record GetReportByIdQuery(long Id) : IRequest<ReportDto>;
    public sealed class GetReportByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetReportByIdQuery, ReportDto>
    {
        public async Task<ReportDto> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
        {
            var reports = unitOfWork.Repository<Report>();
            var report = await reports.FirstOrDefaultAsync(
                predicate: candidate => candidate.Id == request.Id,
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException(nameof(Report), request.Id);
            return report.ToDto();
        }
    }
}

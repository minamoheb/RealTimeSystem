using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Reports
{
    public sealed record CreateReportCommand(string Title, DateTime FromUtc, DateTime ToUtc) : IRequest<ReportDto>;
    public sealed class CreateReportCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IBackgroundJobScheduler scheduler) : IRequestHandler<CreateReportCommand, ReportDto>
    {
        public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken cancellationToken)
        {
            if (currentUser.UserId is not long userId || string.IsNullOrWhiteSpace(currentUser.Email))
            {
                throw new UnauthorizedAppException("Authentication is required.");
            }

            var reports = unitOfWork.Repository<Report>();
            var report = new Report
            {
                Title = request.Title.Trim(),
                FromUtc = request.FromUtc,
                ToUtc = request.ToUtc,
                RequestedByUserId = userId,
                RequestedByEmail = currentUser.Email,
                RequestedAtUtc = clock.UtcNow
            };

            await reports.AddAsync(report, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            scheduler.EnqueueReportPipeline(report.Id);
            return report.ToDto();
        }
    }
}

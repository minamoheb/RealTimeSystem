using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Alerts
{
    public sealed record AcknowledgeAlertCommand(long AlertId) : IRequest<AlertDto>;
    public sealed class AcknowledgeAlertCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser,
        IDateTimeProvider clock) : IRequestHandler<AcknowledgeAlertCommand, AlertDto>
    {
        public async Task<AlertDto> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
        {
            if (currentUser.UserId is not long userId)
            {
                throw new UnauthorizedAppException("Authentication is required.");
            }

            var alerts = unitOfWork.Repository<Alert>();
            var alert = await alerts.Query(asNoTracking: false).Include(candidate => candidate.Server)
                .FirstOrDefaultAsync(candidate => candidate.Id == request.AlertId, cancellationToken)
                ?? throw new NotFoundException(nameof(Alert), request.AlertId);

            alert.Acknowledge(userId, clock.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return alert.ToDto();
        }
    }
}

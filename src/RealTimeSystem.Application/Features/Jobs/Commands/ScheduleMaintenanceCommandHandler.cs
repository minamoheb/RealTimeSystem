namespace RealTimeSystem.Application.Features.Jobs
{
    public sealed record ScheduleMaintenanceCommand(long ServerId, string Reason, int DelayMinutes) : IRequest<MaintenanceNoticeDto>;
    public sealed class ScheduleMaintenanceCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IBackgroundJobScheduler scheduler) : IRequestHandler<ScheduleMaintenanceCommand, MaintenanceNoticeDto>
    {
        public async Task<MaintenanceNoticeDto> Handle(ScheduleMaintenanceCommand request, CancellationToken cancellationToken)
        {
            var servers = unitOfWork.Repository<MonitoredServer>();
            var maintenanceWindows = unitOfWork.Repository<MaintenanceWindow>();
            var server = await servers.Query(asNoTracking: false)
                .FirstOrDefaultAsync(candidate => candidate.Id == request.ServerId && candidate.IsActive, cancellationToken)
                ?? throw new NotFoundException(nameof(MonitoredServer), request.ServerId);

            var scheduledFor = clock.UtcNow.AddMinutes(request.DelayMinutes);
            var window = new MaintenanceWindow
            {
                ServerId = server.Id,
                Reason = request.Reason.Trim(),
                ScheduledForUtc = scheduledFor,
                CreatedAtUtc = clock.UtcNow
            };
            window.NotifyScheduled(server.Name);
            await maintenanceWindows.AddAsync(window, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            scheduler.ScheduleMaintenance(window.Id, TimeSpan.FromMinutes(request.DelayMinutes));

            return new MaintenanceNoticeDto(window.Id, server.Id, server.Name, window.Reason, window.ScheduledForUtc, window.Status.ToString());
        }
    }
}

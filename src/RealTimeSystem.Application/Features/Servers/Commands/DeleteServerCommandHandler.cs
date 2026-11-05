namespace RealTimeSystem.Application.Features.Servers
{
    public sealed record DeleteServerCommand(long Id) : IRequest<bool>;
    public sealed class DeleteServerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteServerCommand, bool>
    {
        public async Task<bool> Handle(DeleteServerCommand request, CancellationToken cancellationToken)
        {
            var servers = unitOfWork.Repository<MonitoredServer>();
            var server = await servers.Query(asNoTracking: false).FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(MonitoredServer), request.Id);
            server.Deactivate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

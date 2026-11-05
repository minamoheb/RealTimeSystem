using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Servers
{
    public sealed record UpdateServerCommand(long Id, string Name, string HostName, string EnvironmentName, string? Description) : IRequest<ServerDto>;

    public sealed class UpdateServerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateServerCommand, ServerDto>
    {
        public async Task<ServerDto> Handle(UpdateServerCommand request, CancellationToken cancellationToken)
        {
            var servers = unitOfWork.Repository<MonitoredServer>();
            var server = await servers.Query(asNoTracking: false).FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(MonitoredServer), request.Id);

            var name = request.Name.Trim();
            var duplicate = await servers.Query().AnyAsync(candidate => candidate.Id != request.Id && candidate.Name == name, cancellationToken);
            if (duplicate)
            {
                throw new ConflictException($"A server named '{name}' already exists.");
            }

            server.Name = name;
            server.HostName = request.HostName.Trim();
            server.EnvironmentName = request.EnvironmentName.Trim();
            server.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await servers.Query().Where(candidate => candidate.Id == server.Id).ProjectServer().FirstAsync(cancellationToken);
        }
    }
}

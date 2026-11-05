using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Servers
{

    public sealed record CreateServerCommand(string Name, string HostName, string EnvironmentName, string? Description) : IRequest<ServerDto>;
    public sealed class CreateServerCommandHandler(
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock) : IRequestHandler<CreateServerCommand, ServerDto>
    {
        public async Task<ServerDto> Handle(CreateServerCommand request, CancellationToken cancellationToken)
        {
            var servers = unitOfWork.Repository<MonitoredServer>();
            var name = request.Name.Trim();
            var exists = await servers.Query().AnyAsync(server => server.Name == name, cancellationToken);
            if (exists)
            {
                throw new ConflictException($"A server named '{name}' already exists.");
            }

            var server = new MonitoredServer
            {
                Name = name,
                HostName = request.HostName.Trim(),
                EnvironmentName = request.EnvironmentName.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CreatedAtUtc = clock.UtcNow
            };

            await servers.AddAsync(server, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await servers.Query().Where(candidate => candidate.Id == server.Id).ProjectServer().FirstAsync(cancellationToken);
        }
    }
}

using RealTimeSystem.Application.Common.Mapping;

namespace RealTimeSystem.Application.Features.Servers
{
    public sealed record GetServerByIdQuery(long Id) : IRequest<ServerDto>;

    public sealed class GetServerByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetServerByIdQuery, ServerDto>
    {
        public async Task<ServerDto> Handle(GetServerByIdQuery request, CancellationToken cancellationToken)
        {
            var servers = unitOfWork.Repository<MonitoredServer>();
            var server = await servers.FirstOrDefaultAsync(
                selector: ServerMappings.ServerDtoProjection,
                predicate: candidate => candidate.Id == request.Id,
                cancellationToken: cancellationToken);
            return server ?? throw new NotFoundException(nameof(MonitoredServer), request.Id);
        }
    }
}

using RealTimeSystem.Application.Common.Mapping;
using RealTimeSystem.Domain.Enums;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Features.Servers
{
    public sealed record GetServersQuery : Pagination, IRequest<PagedResult<ServerDto>>
    {
        public string? Search { get; init; }

        public string? Environment { get; init; }

        public string? Status { get; init; }

        public string? SortBy { get; init; }

        public bool SortDescending { get; init; }

        public bool IncludeInactive { get; init; }

        public GetServersQuery()
        {
            PageSize = 10;
        }
    }

    public sealed class GetServersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetServersQuery, PagedResult<ServerDto>>
    {
        public async Task<PagedResult<ServerDto>> Handle(GetServersQuery request, CancellationToken cancellationToken)
        {
            var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);
            var servers = unitOfWork.Repository<MonitoredServer>();
            var predicate = BuildPredicate(request);
            var total = await servers.CountAsync(predicate, cancellationToken);
            var items = await servers.SelectListAsync(
                selector: ServerMappings.ServerDtoProjection,
                predicate: predicate,
                orderBy: BuildOrderBy(request),
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);
            return PagedResult<ServerDto>.Create(items, page, pageSize, total);
        }

        private static Expression<Func<MonitoredServer, bool>> BuildPredicate(GetServersQuery request)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(request.Search);
            var term = request.Search?.Trim() ?? string.Empty;
            var hasEnvironment = !string.IsNullOrWhiteSpace(request.Environment);
            var environment = request.Environment?.Trim() ?? string.Empty;
            var status = default(ServerStatus);
            var hasStatus = !string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<ServerStatus>(request.Status, true, out status);

            return server =>
                (request.IncludeInactive || server.IsActive) &&
                (!hasSearch || server.Name.Contains(term) || server.HostName.Contains(term)) &&
                (!hasEnvironment || server.EnvironmentName == environment) &&
                (!hasStatus || server.Status == status);
        }

        private static Func<IQueryable<MonitoredServer>, IOrderedQueryable<MonitoredServer>> BuildOrderBy(GetServersQuery request)
        {
            return (request.SortBy?.Trim().ToLowerInvariant(), request.SortDescending) switch
            {
                ("host", true) => query => query.OrderByDescending(server => server.HostName),
                ("host", false) => query => query.OrderBy(server => server.HostName),
                ("environment", true) => query => query.OrderByDescending(server => server.EnvironmentName),
                ("environment", false) => query => query.OrderBy(server => server.EnvironmentName),
                ("status", true) => query => query.OrderByDescending(server => server.Status),
                ("status", false) => query => query.OrderBy(server => server.Status),
                ("created", true) => query => query.OrderByDescending(server => server.CreatedAtUtc),
                ("created", false) => query => query.OrderBy(server => server.CreatedAtUtc),
                (_, true) => query => query.OrderByDescending(server => server.Name),
                _ => query => query.OrderBy(server => server.Name)
            };
        }
    }
}

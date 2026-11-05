using RealTimeSystem.Domain.Services;

namespace RealTimeSystem.Application.Features.Servers
{

    public sealed record GetServersV2Query : Pagination, IRequest<PagedResult<ServerV2Dto>>
    {
        public string? Search { get; init; }

        public string? Environment { get; init; }

        public string? Status { get; init; }

        public string? SortBy { get; init; }

        public bool SortDescending { get; init; }

        public GetServersV2Query()
        {
            PageSize = 10;
        }
    }

    public sealed class GetServersV2QueryHandler(ISender sender) : IRequestHandler<GetServersV2Query, PagedResult<ServerV2Dto>>
    {
        public async Task<PagedResult<ServerV2Dto>> Handle(GetServersV2Query request, CancellationToken cancellationToken)
        {
            var page = await sender.Send(
                new GetServersQuery
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Search = request.Search,
                    Environment = request.Environment,
                    Status = request.Status,
                    SortBy = request.SortBy,
                    SortDescending = request.SortDescending
                },
                cancellationToken);

            var items = page.Items.Select(server => new ServerV2Dto(
                server.Id,
                server.Name,
                server.HostName,
                server.EnvironmentName,
                server.Status,
                HealthScoreCalculator.Calculate(server.CpuPercent, server.MemoryPercent, server.DiskPercent, server.ResponseTimeMs),
                server.CpuPercent,
                server.MemoryPercent,
                server.DiskPercent,
                server.ResponseTimeMs,
                server.OpenAlertCount)).ToList();

            return PagedResult<ServerV2Dto>.Create(items, page.Page, page.PageSize, page.TotalCount);
        }
    }
}

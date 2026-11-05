using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Metrics;
using RealTimeSystem.Application.Features.Servers;
using RealTimeSystem.Application.Features.Servers.Dtos;

namespace RealTimeSystem.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/servers")]
    public sealed class ServersController : BaseApiController
    {
        public ServersController(ISender sender) : base(sender) { }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ServerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ServerDto>>> Get([FromQuery] GetServersQuery query, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(query, cancellationToken));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ServerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServerDto>> GetById(long id, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(new GetServerByIdQuery(id), cancellationToken));

        [HttpGet("{id:long}/metrics")]
        [ProducesResponseType(typeof(PagedResult<MetricHistoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<MetricHistoryDto>>> Metrics(
            long id,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 200,
            CancellationToken cancellationToken = default) =>
            Ok(await Sender.Send(new GetServerMetricsQuery
            {
                ServerId = id,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Page = page,
                PageSize = pageSize
            }, cancellationToken));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ServerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ServerDto>> Create([FromBody] CreateServerRequestDto request, CancellationToken cancellationToken)
        {
            var created = await Sender.Send(new CreateServerCommand(request.Name, request.HostName, request.EnvironmentName, request.Description), cancellationToken);
            return Created($"/api/v1/servers/{created.Id}", created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(ServerDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServerDto>> Update(long id, [FromBody] UpdateServerRequestDto body, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(new UpdateServerCommand(id, body.Name, body.HostName, body.EnvironmentName, body.Description), cancellationToken));

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await Sender.Send(new DeleteServerCommand(id), cancellationToken);
            return NoContent();
        }
    }
}

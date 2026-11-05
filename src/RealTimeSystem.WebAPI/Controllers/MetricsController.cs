using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Metrics;

namespace RealTimeSystem.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/metrics")]
    public sealed class MetricsController : BaseApiController
    {
        public MetricsController(ISender sender) : base(sender) { }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(PagedResult<LiveMetricDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<LiveMetricDto>>> Latest(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default) =>
            Ok(await Sender.Send(new GetLatestMetricsQuery
            {
                Page = page,
                PageSize = pageSize
            }, cancellationToken));
    }
}

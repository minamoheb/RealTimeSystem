using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Alerts;

namespace RealTimeSystem.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/alerts")]
    public sealed class AlertsController : BaseApiController
    {
        public AlertsController(ISender sender) : base(sender) { }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<AlertDto>>> Get([FromQuery] GetAlertsQuery query, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(query, cancellationToken));

        [HttpPost("{id:long}/acknowledge")]
        [ProducesResponseType(typeof(AlertDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlertDto>> Acknowledge(long id, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(new AcknowledgeAlertCommand(id), cancellationToken));
    }
}

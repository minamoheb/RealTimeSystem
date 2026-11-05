using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Servers;

namespace RealTimeSystem.WebAPI.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/servers")]
    public sealed class ServersV2Controller : BaseApiController
    {
        public ServersV2Controller(ISender sender) : base(sender) { }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ServerV2Dto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ServerV2Dto>>> Get([FromQuery] GetServersV2Query query, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(query, cancellationToken));
    }
}


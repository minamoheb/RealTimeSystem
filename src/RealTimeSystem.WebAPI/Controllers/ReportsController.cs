using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Reports;

namespace RealTimeSystem.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reports")]
    public sealed class ReportsController : BaseApiController
    {
        public ReportsController(ISender sender) : base(sender) { }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ReportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ReportDto>>> Get([FromQuery] GetReportsQuery query, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(query, cancellationToken));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReportDto>> GetById(long id, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(new GetReportByIdQuery(id), cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(ReportDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReportDto>> Create([FromBody] CreateReportRequestDto request, CancellationToken cancellationToken)
        {
            var report = await Sender.Send(new CreateReportCommand(request.Title, request.FromUtc, request.ToUtc), cancellationToken);
            return Accepted($"/api/v1/reports/{report.Id}", report);
        }
    }
}
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Jobs;

namespace RealTimeSystem.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/jobs")]
    public sealed class JobsController : BaseApiController
    {
        public JobsController(ISender sender) : base(sender) { }

        [HttpGet]
        [ProducesResponseType(typeof(JobOverviewDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<JobOverviewDto>> Get(CancellationToken cancellationToken) =>
            Ok(await Sender.Send(new GetJobOverviewQuery(), cancellationToken));

        [Authorize(Roles = "Admin")]
        [HttpPost("hangfire-session")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult HangfireSession()
        {
            var header = Request.Headers.Authorization.ToString();
            const string prefix = "Bearer ";
            if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized();
            }

            Response.Cookies.Append("rts.hangfire", header[prefix.Length..].Trim(), new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                Path = "/hangfire",
                MaxAge = TimeSpan.FromMinutes(30)
            });
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("collect")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Collect(CancellationToken cancellationToken)
        {
            await Sender.Send(new CollectMetricsCommand(), cancellationToken);
            return Accepted();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{jobId}/retry")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Retry(string jobId, CancellationToken cancellationToken)
        {
            await Sender.Send(new RetryJobCommand(jobId), cancellationToken);
            return Accepted();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("maintenance")]
        [ProducesResponseType(typeof(MaintenanceNoticeDto), StatusCodes.Status202Accepted)]
        public async Task<ActionResult<MaintenanceNoticeDto>> ScheduleMaintenance(
            [FromBody] ScheduleMaintenanceRequestDto request,
            CancellationToken cancellationToken)
        {
            var notice = await Sender.Send(new ScheduleMaintenanceCommand(request.ServerId, request.Reason, request.DelayMinutes), cancellationToken);
            return Accepted($"/api/v1/jobs/maintenance/{notice.Id}", notice);
        }
    }
}


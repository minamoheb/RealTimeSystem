using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealTimeSystem.Application.Common.Mediator;

namespace RealTimeSystem.WebAPI.Controllers
{
[ApiController]
[Authorize]
[EnableRateLimiting("api")]
public abstract class BaseApiController : ControllerBase
{
    protected BaseApiController(ISender sender) => Sender = sender;

    protected ISender Sender { get; }
}
}
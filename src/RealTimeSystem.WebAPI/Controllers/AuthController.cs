using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Features.Auth;

namespace RealTimeSystem.WebAPI.Controllers
{
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : BaseApiController
{
    public AuthController(ISender sender) : base(sender) { }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken) =>
        Ok(await Sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken) =>
        Ok(await Sender.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken));

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request, CancellationToken cancellationToken)
    {
        await Sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken cancellationToken) =>
        Ok(await Sender.Send(new GetCurrentUserQuery(), cancellationToken));
}
}

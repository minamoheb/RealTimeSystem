using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Features.Users;

namespace RealTimeSystem.WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public sealed class UsersController : BaseApiController
    {
        public UsersController(ISender sender) : base(sender) { }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<UserDto>>> Get([FromQuery] GetUsersQuery query, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(query, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
        {
            var user = await Sender.Send(new CreateUserCommand(request.Email, request.FullName, request.Password, request.Roles), cancellationToken);
            return Created($"/api/v1/users/{user.Id}", user);
        }

        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDto>> Update(long id, [FromBody] UpdateUserRequestDto body, CancellationToken cancellationToken) =>
            Ok(await Sender.Send(new UpdateUserCommand(id, body.FullName, body.IsActive, body.Roles), cancellationToken));

        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await Sender.Send(new DeleteUserCommand(id), cancellationToken);
            return NoContent();
        }
    }
}

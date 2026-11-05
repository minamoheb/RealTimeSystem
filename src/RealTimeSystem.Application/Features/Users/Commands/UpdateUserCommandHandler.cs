using RealTimeSystem.Application.Common.Mapping;
using RealTimeSystem.Application.Features.Auth;

namespace RealTimeSystem.Application.Features.Users
{
    public sealed record UpdateUserCommand(long Id, string FullName, bool IsActive, IReadOnlyList<string> Roles) : IRequest<UserDto>;
    public sealed class UpdateUserCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, UserDto>
    {
        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var users = unitOfWork.Repository<User>();
            var userRoles = unitOfWork.Repository<UserRole>();
            var user = await users.Query(asNoTracking: false)
                .Include(candidate => candidate.UserRoles)
                .FirstOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.Id);

            var roles = await RoleGuard.ResolveRolesAsync(unitOfWork, request.Roles, cancellationToken);
            user.FullName = request.FullName.Trim();
            user.IsActive = request.IsActive;
            userRoles.RemoveRange(user.UserRoles);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            foreach (var role in roles)
            {
                await userRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id }, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await users.Query()
                .Include(candidate => candidate.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .FirstAsync(candidate => candidate.Id == user.Id, cancellationToken);
            return updated.ToDto();
        }
    }
}

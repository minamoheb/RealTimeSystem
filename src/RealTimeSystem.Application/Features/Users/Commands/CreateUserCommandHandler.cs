using RealTimeSystem.Application.Common.Mapping;
using RealTimeSystem.Application.Features.Auth;

namespace RealTimeSystem.Application.Features.Users
{
    public sealed record CreateUserCommand(string Email, string FullName, string Password, IReadOnlyList<string> Roles) : IRequest<UserDto>;
    public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IDateTimeProvider clock) : IRequestHandler<CreateUserCommand, UserDto>
    {
        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var users = unitOfWork.Repository<User>();
            var email = request.Email.Trim().ToLowerInvariant();
            if (await users.Query().AnyAsync(user => user.Email == email, cancellationToken))
            {
                throw new ConflictException($"A user with email '{email}' already exists.");
            }

            var roles = await RoleGuard.ResolveRolesAsync(unitOfWork, request.Roles, cancellationToken);
            var user = new User
            {
                Email = email,
                FullName = request.FullName.Trim(),
                PasswordHash = passwordHasher.Hash(request.Password),
                CreatedAtUtc = clock.UtcNow,
                UserRoles = roles.Select(role => new UserRole { RoleId = role.Id }).ToList()
            };

            await users.AddAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await users.Query()
                .Include(candidate => candidate.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .FirstAsync(candidate => candidate.Id == user.Id, cancellationToken);
            return created.ToDto();
        }
    }
}


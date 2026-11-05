namespace RealTimeSystem.Application.Features.Auth
{
    public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
    public sealed class LoginCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher,
        IAuthTokenIssuer tokenIssuer) : IRequestHandler<LoginCommand, AuthResponse>
    {
        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var users = unitOfWork.Repository<User>();
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await users.Query()
                .Include(candidate => candidate.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .ThenInclude(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
                .FirstOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

            if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAppException("Invalid email or password.");
            }

            return await tokenIssuer.IssueAsync(user, cancellationToken);
        }
    }
}


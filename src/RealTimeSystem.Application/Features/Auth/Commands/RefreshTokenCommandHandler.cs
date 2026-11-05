namespace RealTimeSystem.Application.Features.Auth
{
    public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
    public sealed class RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IDateTimeProvider clock,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshTokens = unitOfWork.Repository<RefreshToken>();
            var hash = tokenService.HashRefreshToken(request.RefreshToken);
            var existing = await refreshTokens.Query(asNoTracking: false)
                .Include(token => token.User)
                .ThenInclude(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .ThenInclude(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
                .FirstOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);

            if (existing is null || !existing.IsValid(clock.UtcNow) || !existing.User.IsActive)
            {
                throw new UnauthorizedAppException("Refresh token is invalid or expired.");
            }

            existing.Revoke(clock.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await tokenIssuer.IssueAsync(existing.User, cancellationToken);
        }
    }
}

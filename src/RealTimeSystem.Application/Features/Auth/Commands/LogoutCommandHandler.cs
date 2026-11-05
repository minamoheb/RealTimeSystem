namespace RealTimeSystem.Application.Features.Auth
{
    public sealed record LogoutCommand(string? RefreshToken) : IRequest<bool>;
    public sealed class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IDateTimeProvider clock,
    ICurrentUser currentUser) : IRequestHandler<LogoutCommand, bool>
    {
        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var refreshTokens = unitOfWork.Repository<RefreshToken>();
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var hash = tokenService.HashRefreshToken(request.RefreshToken);
                var token = await refreshTokens.Query(asNoTracking: false)
                    .FirstOrDefaultAsync(candidate => candidate.TokenHash == hash, cancellationToken);
                token?.Revoke(clock.UtcNow);
            }
            else if (currentUser.UserId is long userId)
            {
                var tokens = await refreshTokens.Query(asNoTracking: false)
                    .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
                    .ToListAsync(cancellationToken);
                foreach (var token in tokens)
                {
                    token.Revoke(clock.UtcNow);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

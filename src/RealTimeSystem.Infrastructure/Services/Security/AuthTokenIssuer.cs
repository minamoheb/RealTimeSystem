using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Application.Common.Mapping;
using RealTimeSystem.Domain.Entities;

namespace RealTimeSystem.Infrastructure.Services.Security
{
    public sealed class AuthTokenIssuer(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IDateTimeProvider clock,
        IRefreshTokenLifetime refreshTokenLifetime) : IAuthTokenIssuer
    {
        public async Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken)
        {
            var access = tokenService.CreateAccessToken(user);
            var refreshValue = tokenService.CreateRefreshTokenValue();
            var refreshExpires = clock.UtcNow.Add(refreshTokenLifetime.Lifetime);

            var refreshTokens = unitOfWork.Repository<RefreshToken>();

            await refreshTokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokenService.HashRefreshToken(refreshValue),
                CreatedAtUtc = clock.UtcNow,
                ExpiresAtUtc = refreshExpires
            }, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                access.Token,
                access.ExpiresAtUtc,
                refreshValue,
                refreshExpires,
                user.ToProfile());
        }
    }
}

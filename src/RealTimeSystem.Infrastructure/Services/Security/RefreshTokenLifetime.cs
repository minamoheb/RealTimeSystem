using Microsoft.Extensions.Options;
using RealTimeSystem.Application.Common.Interfaces;

namespace RealTimeSystem.Infrastructure.Services.Security
{
    public sealed class RefreshTokenLifetime : IRefreshTokenLifetime
    {
        public RefreshTokenLifetime(IOptions<JwtOptions> options)
        {
            Lifetime = TimeSpan.FromDays(Math.Max(1, options.Value.RefreshTokenDays));
        }

        public TimeSpan Lifetime { get; }
    }
}

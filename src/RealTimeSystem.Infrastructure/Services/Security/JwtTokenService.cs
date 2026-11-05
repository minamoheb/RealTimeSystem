using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealTimeSystem.Infrastructure.Services.Security
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly IDateTimeProvider _clock;

        public JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider clock)
        {
            _options = options.Value;
            _clock = clock;
        }

        public AccessTokenResult CreateAccessToken(User user)
        {
            if (string.IsNullOrWhiteSpace(_options.Key) || _options.Key.Length < 32)
            {
                throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
            }

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            foreach (var role in user.UserRoles.Select(userRole => userRole.Role.Name).Distinct())
            {
                claims.Add(new Claim("role", role));
            }

            var permissions = user.UserRoles
                .SelectMany(userRole => userRole.Role.RolePermissions)
                .Select(rolePermission => rolePermission.Permission.Name)
                .Distinct();
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var expires = _clock.UtcNow.AddMinutes(_options.AccessTokenMinutes);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claims,
                notBefore: _clock.UtcNow,
                expires: expires,
                signingCredentials: credentials);

            return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public string CreateRefreshTokenValue() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public string HashRefreshToken(string refreshToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}

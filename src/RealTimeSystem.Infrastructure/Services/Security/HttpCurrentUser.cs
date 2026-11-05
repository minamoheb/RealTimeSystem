using Microsoft.AspNetCore.Http;
using RealTimeSystem.Application.Common.Interfaces;
using System.Security.Claims;

namespace RealTimeSystem.Infrastructure.Services.Security
{
    public sealed class HttpCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? UserId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
                return long.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("email");

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }
}

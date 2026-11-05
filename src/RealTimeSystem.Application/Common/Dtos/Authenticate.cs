namespace RealTimeSystem.Application.Common.Dtos
{

    public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
    public sealed record RefreshTokenRequestDto(string RefreshToken);
    public sealed record LoginRequestDto(string Email, string Password);
    public sealed record LogoutRequestDto(string? RefreshToken);
    public sealed record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc, UserProfileDto User);
}

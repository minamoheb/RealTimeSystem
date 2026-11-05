namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface ITokenService
    {
        AccessTokenResult CreateAccessToken(User user);

        string CreateRefreshTokenValue();

        string HashRefreshToken(string refreshToken);
    }
}


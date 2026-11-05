namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface IRefreshTokenLifetime
    {
        TimeSpan Lifetime { get; }
    }
}


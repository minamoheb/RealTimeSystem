namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface IAuthTokenIssuer
    {
        Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken);
    }
}

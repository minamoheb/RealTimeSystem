namespace RealTimeSystem.Application.Features.Servers.Dtos
{
    public sealed record CreateServerRequestDto(string Name, string HostName, string EnvironmentName, string? Description);
    public sealed record UpdateServerRequestDto(string Name, string HostName, string EnvironmentName, string? Description);
}


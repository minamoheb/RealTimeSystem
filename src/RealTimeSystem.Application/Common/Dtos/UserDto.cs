namespace RealTimeSystem.Application.Common.Dtos
{
    public sealed record UserDto(long Id, string Email, string FullName, bool IsActive, DateTime CreatedAtUtc, IReadOnlyList<string> Roles);
    public sealed record UserProfileDto(long Id, string Email, string FullName, IReadOnlyList<string> Roles);
    public sealed record CreateUserRequestDto(string Email, string FullName, string Password, IReadOnlyList<string> Roles);
    public sealed record UpdateUserRequestDto(string FullName, bool IsActive, IReadOnlyList<string> Roles);
}

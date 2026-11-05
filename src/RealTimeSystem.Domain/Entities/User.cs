namespace RealTimeSystem.Domain.Entities
{
    public sealed class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<Report> Reports { get; set; } = new List<Report>();

        public void Deactivate() => IsActive = false;
    }
}

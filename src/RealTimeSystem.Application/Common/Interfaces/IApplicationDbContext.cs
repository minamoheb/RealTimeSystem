namespace RealTimeSystem.Application.Common.Interfaces
{
public interface IApplicationDbContext
{
    DbSet<MonitoredServer> Servers { get; }

    DbSet<MetricSample> Metrics { get; }

    DbSet<Alert> Alerts { get; }

    DbSet<Report> Reports { get; }

    DbSet<User> Users { get; }

    DbSet<Role> Roles { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<UserRole> UserRoles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<MaintenanceWindow> MaintenanceWindows { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
}


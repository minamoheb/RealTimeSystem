using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Domain.Enums;

namespace RealTimeSystem.Infrastructure.Persistence.Configurations
{
public sealed class MonitoredServerConfiguration : IEntityTypeConfiguration<MonitoredServer>
{
    public void Configure(EntityTypeBuilder<MonitoredServer> builder)
    {
        builder.ToTable("Servers");
        builder.HasKey(server => server.Id);
        builder.Property(server => server.Name).HasMaxLength(120).IsRequired();
        builder.Property(server => server.HostName).HasMaxLength(200).IsRequired();
        builder.Property(server => server.EnvironmentName).HasMaxLength(40).IsRequired();
        builder.Property(server => server.Description).HasMaxLength(500);
        builder.Property(server => server.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(server => server.Name).IsUnique();
        builder.Ignore(server => server.DomainEvents);
        builder.HasMany(server => server.Metrics).WithOne(metric => metric.Server).HasForeignKey(metric => metric.ServerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(server => server.Alerts).WithOne(alert => alert.Server).HasForeignKey(alert => alert.ServerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(server => server.MaintenanceWindows).WithOne(window => window.Server).HasForeignKey(window => window.ServerId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MetricSampleConfiguration : IEntityTypeConfiguration<MetricSample>
{
    public void Configure(EntityTypeBuilder<MetricSample> builder)
    {
        builder.ToTable("MetricSamples");
        builder.HasKey(metric => metric.Id);
        builder.Property(metric => metric.Id).ValueGeneratedOnAdd();
        builder.HasIndex(metric => new { metric.ServerId, metric.CollectedAtUtc });
    }
}

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");
        builder.HasKey(alert => alert.Id);
        builder.Property(alert => alert.MetricName).HasMaxLength(40).IsRequired();
        builder.Property(alert => alert.Message).HasMaxLength(500).IsRequired();
        builder.Property(alert => alert.Severity).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(alert => new { alert.ServerId, alert.CreatedAtUtc });
    }
}

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(report => report.Id);
        builder.Property(report => report.Title).HasMaxLength(160).IsRequired();
        builder.Property(report => report.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(report => report.Summary).HasMaxLength(4000);
        builder.Property(report => report.FailureReason).HasMaxLength(1000);
        builder.Property(report => report.StoragePath).HasMaxLength(500);
        builder.Property(report => report.RequestedByEmail).HasMaxLength(256).IsRequired();
        builder.Ignore(report => report.DomainEvents);
        builder.HasOne(report => report.RequestedBy).WithMany(user => user.Reports).HasForeignKey(report => report.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.Property(user => user.FullName).HasMaxLength(160).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Name).HasMaxLength(64).IsRequired();
        builder.HasIndex(role => role.Name).IsUnique();
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Name).HasMaxLength(80).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(200);
        builder.HasIndex(permission => permission.Name).IsUnique();
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
        builder.HasOne(userRole => userRole.User).WithMany(user => user.UserRoles).HasForeignKey(userRole => userRole.UserId);
        builder.HasOne(userRole => userRole.Role).WithMany(role => role.UserRoles).HasForeignKey(userRole => userRole.RoleId);
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });
        builder.HasOne(rolePermission => rolePermission.Role).WithMany(role => role.RolePermissions).HasForeignKey(rolePermission => rolePermission.RoleId);
        builder.HasOne(rolePermission => rolePermission.Permission).WithMany(permission => permission.RolePermissions).HasForeignKey(rolePermission => rolePermission.PermissionId);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasOne(token => token.User).WithMany(user => user.RefreshTokens).HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MaintenanceWindowConfiguration : IEntityTypeConfiguration<MaintenanceWindow>
{
    public void Configure(EntityTypeBuilder<MaintenanceWindow> builder)
    {
        builder.ToTable("MaintenanceWindows");
        builder.HasKey(window => window.Id);
        builder.Property(window => window.Reason).HasMaxLength(300).IsRequired();
        builder.Property(window => window.Status).HasConversion<string>().HasMaxLength(32);
        builder.Ignore(window => window.DomainEvents);
    }
}
}


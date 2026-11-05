using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Domain.Constants;
using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Domain.Enums;

namespace RealTimeSystem.Infrastructure.Persistence.DBContext
{
public static class DatabaseInitializer
{

    public static async Task InitializeDatabaseAsync(this IHost app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var startup = scope.ServiceProvider.GetRequiredService<DatabaseStartup>();
        await startup.InitializeAsync(cancellationToken);
    }
}

public sealed class DatabaseStartup
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseStartup> _logger;
    private readonly bool _applyMigrations;
    private readonly bool _seed;

    public DatabaseStartup(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IHostEnvironment environment,
        ILogger<DatabaseStartup> logger,
        IConfiguration configuration)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _environment = environment;
        _logger = logger;
        _applyMigrations = configuration.GetValue("Database:ApplyMigrations", true);
        _seed = configuration.GetValue("Seed:Enabled", true);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.Combine(_environment.ContentRootPath, "App_Data"));
        if (_applyMigrations)
        {
            await BaselineExistingSchemaAsync(cancellationToken);
            await _db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _db.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (_seed)
        {
            await SeedAsync(cancellationToken);
        }
    }

    private async Task BaselineExistingSchemaAsync(CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var shape = connection.CreateCommand();
        shape.CommandText = "SELECT CASE WHEN COL_LENGTH(N'dbo.Servers', N'CreatedBy') IS NULL THEN 0 ELSE 1 END";
        var currentShape = Convert.ToInt32(await shape.ExecuteScalarAsync(cancellationToken) ?? 0) == 1;
        if (!currentShape)
        {
            return;
        }

        await using var history = connection.CreateCommand();
        history.CommandText = """
            IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END
            """;
        await history.ExecuteNonQueryAsync(cancellationToken);

        foreach (var migrationId in new[]
        {
            "20261104112147_InitialSqlServer",
            "20261104115011_ConvertGuidKeysToLong",
            "20261105091857_SyncPendingModel"
        })
        {
            await using var mark = connection.CreateCommand();
            mark.CommandText = """
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @id)
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (@id, N'10.0.10');
                """;
            var id = mark.CreateParameter();
            id.ParameterName = "@id";
            id.Value = migrationId;
            mark.Parameters.Add(id);
            await mark.ExecuteNonQueryAsync(cancellationToken);
        }

        _logger.LogInformation("Existing SQL Server schema matches the current model. Pending duplicate migrations were skipped.");
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var permissions = new[]
        {
            Permission("servers.read", "View monitored servers"),
            Permission("servers.write", "Create and update servers"),
            Permission("metrics.read", "View metric history"),
            Permission("alerts.acknowledge", "Acknowledge alerts"),
            Permission("reports.generate", "Generate monitoring reports"),
            Permission("users.manage", "Manage user accounts"),
            Permission("jobs.manage", "Schedule maintenance and inspect jobs")
        };

        var admin = new Role { Name = RoleNames.Admin };
        var user = new Role { Name = RoleNames.User };
        var userPermissionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "servers.read",
            "metrics.read",
            "alerts.acknowledge",
            "reports.generate"
        };

        foreach (var permission in permissions)
        {
            admin.RolePermissions.Add(new RolePermission { Role = admin, Permission = permission });
            if (userPermissionNames.Contains(permission.Name))
            {
                user.RolePermissions.Add(new RolePermission { Role = user, Permission = permission });
            }
        }

        var adminUser = new User
        {
            Email = "admin@monitor.local",
            FullName = "Amina Hassan",
            PasswordHash = _passwordHasher.Hash("Admin123!"),
            CreatedAtUtc = now.AddDays(-30)
        };
        adminUser.UserRoles.Add(new UserRole { User = adminUser, Role = admin });

        var analyst = new User
        {
            Email = "analyst@monitor.local",
            FullName = "Omar Farouk",
            PasswordHash = _passwordHasher.Hash("User123!"),
            CreatedAtUtc = now.AddDays(-12)
        };
        analyst.UserRoles.Add(new UserRole { User = analyst, Role = user });

        var servers = new[]
        {
            Server("edge-gateway-01", "10.0.1.11", "Production", "Public ingress and TLS termination"),
            Server("api-core-01", "10.0.1.21", "Production", "Primary application API"),
            Server("worker-analytics-01", "10.0.2.15", "Production", "Background analysis workers"),
            Server("db-primary-01", "10.0.3.8", "Production", "Primary relational database"),
            Server("staging-web-01", "10.0.8.4", "Staging", "Pre-production web tier")
        };

        _db.Permissions.AddRange(permissions);
        _db.Roles.AddRange(admin, user);
        _db.Users.AddRange(adminUser, analyst);
        _db.Servers.AddRange(servers);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var server in servers)
        {
            var samples = BuildHistory(server, now);
            _db.Metrics.AddRange(samples);
            var latest = samples[^1];
            if (latest.CpuPercent >= 90 || latest.MemoryPercent >= 90 || latest.DiskPercent >= 95 || latest.ResponseTimeMs >= 800)
            {
                server.Status = ServerStatus.Degraded;
            }
        }

        var hot = servers[2];
        _db.Alerts.Add(new Alert
        {
            ServerId = hot.Id,
            MetricName = "CPU",
            Value = 93.4,
            Threshold = 90,
            Severity = AlertSeverity.Critical,
            Message = $"{hot.Name} CPU is 93.4%, above the critical threshold of 90%.",
            CreatedAtUtc = now.AddMinutes(-8)
        });

        _db.Reports.Add(new Report
        {
            Title = "Overnight production digest",
            FromUtc = now.AddHours(-12),
            ToUtc = now.AddHours(-1),
            Status = ReportStatus.Ready,
            Summary = "Seeded digest. api-core-01 and worker-analytics-01 carried the highest CPU. No failed jobs were recorded in this sample window.",
            RequestedByUserId = adminUser.Id,
            RequestedByEmail = adminUser.Email,
            RequestedAtUtc = now.AddHours(-1),
            CompletedAtUtc = now.AddHours(-1).AddMinutes(1)
        });

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded monitoring demonstration data");
    }

    private static Permission Permission(string name, string description) =>
        new() { Name = name, Description = description };

    private static MonitoredServer Server(string name, string host, string environment, string description) =>
        new()
        {
            Name = name,
            HostName = host,
            EnvironmentName = environment,
            Description = description,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-20),
            Status = ServerStatus.Healthy
        };

    private static List<MetricSample> BuildHistory(MonitoredServer server, DateTime now)
    {
        var baselines = server.Name switch
        {
            "edge-gateway-01" => (28d, 46d, 61d, 180d, 90d),
            "api-core-01" => (72d, 68d, 54d, 240d, 160d),
            "worker-analytics-01" => (84d, 77d, 63d, 90d, 360d),
            "db-primary-01" => (34d, 81d, 88d, 40d, 25d),
            _ => (22d, 41d, 48d, 30d, 110d)
        };

        var samples = new List<MetricSample>();
        for (var index = 36; index >= 0; index--)
        {
            var wave = Math.Sin(index / 3.5d);
            var cpu = Clamp(baselines.Item1 + wave * 8 + index % 5, 1, 99);
            var memory = Clamp(baselines.Item2 + Math.Cos(index / 4d) * 4, 1, 99);
            var disk = Clamp(baselines.Item3 + index * 0.02, 1, 99);
            var network = Clamp(baselines.Item4 + wave * 20, 1, 1000);
            var response = Clamp(baselines.Item5 + wave * 30, 10, 1500);
            if (server.Name == "worker-analytics-01" && index == 0)
            {
                cpu = 93.4;
                response = 860;
            }

            if (server.Name == "db-primary-01" && index < 3)
            {
                disk = 91.2;
            }

            samples.Add(new MetricSample
            {
                ServerId = server.Id,
                CpuPercent = Math.Round(cpu, 2),
                MemoryPercent = Math.Round(memory, 2),
                DiskPercent = Math.Round(disk, 2),
                NetworkMbps = Math.Round(network, 2),
                ResponseTimeMs = Math.Round(response, 2),
                CollectedAtUtc = now.AddMinutes(-30 * index)
            });
        }

        return samples;
    }

    private static double Clamp(double value, double min, double max) => Math.Min(max, Math.Max(min, value));
}
}

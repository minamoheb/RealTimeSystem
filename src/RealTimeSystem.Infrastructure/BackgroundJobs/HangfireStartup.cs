using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Data.SqlClient;
using RealTimeSystem.Infrastructure.Persistence.DBContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RealTimeSystem.Infrastructure.BackgroundJobs
{
public static class HangfireStartup
{
    public static async Task EnsureDatabaseAsync(IHost app, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled(app))
        {
            return;
        }

        await SqlServerDatabase.EnsureExistsAsync(GetConnectionString(app), cancellationToken);
    }

    public static async Task EnsureSchemaAsync(IHost app, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled(app))
        {
            return;
        }

        var connectionString = GetConnectionString(app);
        _ = new SqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true
        });

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT OBJECT_ID(N'[HangFire].[Hash]', N'U');";
        var hashTable = await command.ExecuteScalarAsync(cancellationToken);
        if (hashTable is null or DBNull)
        {
            throw new InvalidOperationException(
                "Hangfire did not create [HangFire].[Hash]. The SQL login needs permission to create tables in the application database.");
        }
    }

    public static void RegisterJobs(IHost app)
    {
        if (!IsEnabled(app))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobs.AddOrUpdate<MetricsCollectionJob>(
            "collect-system-metrics",
            job => job.Execute(),
            "*/1 * * * *");
    }

    private static bool IsEnabled(IHost app) =>
        app.Services.GetRequiredService<IConfiguration>().GetValue("Hangfire:Enabled", true);

    private static string GetConnectionString(IHost app)
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        return configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is required when Hangfire is enabled.");
    }
}
}

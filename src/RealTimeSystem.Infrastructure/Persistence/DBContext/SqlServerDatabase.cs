using Microsoft.Data.SqlClient;

namespace RealTimeSystem.Infrastructure.Persistence.DBContext
{
public static class SqlServerDatabase
{
    public static async Task EnsureExistsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        builder.InitialCatalog = "master";
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF DB_ID(@databaseName) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@databaseName);
                EXEC (@sql);
            END
            """;
        command.Parameters.Add(new SqlParameter("@databaseName", databaseName));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
}

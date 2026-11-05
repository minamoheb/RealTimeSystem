using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using RealTimeSystem.Application.Common.Dtos;

namespace RealTimeSystem.WebAPI.Tests;

public class MonitoringApiTests : IClassFixture<MonitoringApiFactory>
{
    private readonly MonitoringApiFactory _factory;

    public MonitoringApiTests(MonitoringApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_IsPublic()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Servers_RequireAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/servers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_AndListServers_ReturnsSeededFleet()
    {
        var client = await CreateAuthenticatedClientAsync("admin@monitor.local", "Admin123!");

        var page = await client.GetFromJsonAsync<PagedResult<ServerDto>>("/api/v1/servers?pageSize=10");

        Assert.NotNull(page);
        Assert.True(page.TotalCount >= 5);
        Assert.Contains(page.Items, server => server.Name == "api-core-01");
    }

    [Fact]
    public async Task Analyst_CannotCreateServers()
    {
        var client = await CreateAuthenticatedClientAsync("analyst@monitor.local", "User123!");

        var response = await client.PostAsJsonAsync("/api/v1/servers", new
        {
            name = "extra-01",
            hostName = "10.9.9.9",
            environmentName = "Test"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateServer_AndGenerateReport()
    {
        var client = await CreateAuthenticatedClientAsync("admin@monitor.local", "Admin123!");
        var create = await client.PostAsJsonAsync("/api/v1/servers", new
        {
            name = $"lab-{Guid.NewGuid():N}"[..12],
            hostName = "10.8.8.8",
            environmentName = "Lab",
            description = "Created by integration test"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var reportResponse = await client.PostAsJsonAsync("/api/v1/reports", new
        {
            title = "Integration window",
            fromUtc = DateTime.UtcNow.AddHours(-6),
            toUtc = DateTime.UtcNow.AddMinutes(1)
        });
        Assert.Equal(HttpStatusCode.Accepted, reportResponse.StatusCode);
        var created = await reportResponse.Content.ReadFromJsonAsync<ReportDto>();
        Assert.NotNull(created);

        var stored = await client.GetFromJsonAsync<ReportDto>($"/api/v1/reports/{created.Id}");
        Assert.NotNull(stored);
        Assert.Equal("Ready", stored.Status);
        Assert.False(string.IsNullOrWhiteSpace(stored.Summary));
    }

    [Fact]
    public async Task VersionTwo_IncludesHealthScore()
    {
        var client = await CreateAuthenticatedClientAsync("admin@monitor.local", "Admin123!");

        var page = await client.GetFromJsonAsync<PagedResult<ServerV2Dto>>("/api/v2/servers?pageSize=10");

        Assert.NotNull(page);
        Assert.All(page.Items, server => Assert.InRange(server.HealthScore, 0, 100));
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        if (!login.IsSuccessStatusCode)
        {
            var body = await login.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed {(int)login.StatusCode}: {body}");
        }
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}

public sealed class MonitoringApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"RealTimeSystemWebApiTests_{Guid.NewGuid():N}";
    private readonly string _sqlServerConnectionString;

    public MonitoringApiFactory()
    {
        _sqlServerConnectionString = SqlTestConnection.ForDatabase(_databaseName);

        Environment.SetEnvironmentVariable("Database__Provider", "SqlServer");
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", "false");
        Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", _sqlServerConnectionString);
        Environment.SetEnvironmentVariable("Hangfire__Enabled", "false");
        Environment.SetEnvironmentVariable("Seed__Enabled", "true");
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-test-signing-key-32-characters");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "RealTimeSystem");
        Environment.SetEnvironmentVariable("Jwt__Audience", "RealTimeSystem.Client");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable("Database__Provider", null);
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", null);
        Environment.SetEnvironmentVariable("Hangfire__Enabled", null);
        Environment.SetEnvironmentVariable("Seed__Enabled", null);
        Environment.SetEnvironmentVariable("Jwt__Key", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        base.Dispose(disposing);
    }

    private static class SqlTestConnection
    {
        public static string ForDatabase(string databaseName)
        {
            var configured = Environment.GetEnvironmentVariable("TEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(configured))
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new InvalidOperationException(
                        "Set TEST_SQLSERVER to a SQL Server connection string. LocalDB is only available on Windows.");
                }

                return $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
            }

            var builder = new SqlConnectionStringBuilder(configured)
            {
                InitialCatalog = databaseName
            };
            return builder.ConnectionString;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealTimeSystem.Application;
using RealTimeSystem.Application.Common.Exceptions;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Application.Common.Mediator;
using RealTimeSystem.Application.Common.Dtos;
using RealTimeSystem.Application.Features.Auth;
using RealTimeSystem.Application.Features.Servers;
using RealTimeSystem.Domain.Constants;
using RealTimeSystem.Domain.Entities;
using RealTimeSystem.Infrastructure.Persistence.DBContext;
using RealTimeSystem.Infrastructure.Persistence.UnitOfWork;
using RealTimeSystem.Infrastructure.Services.Security;

namespace RealTimeSystem.Application.Tests;

public class MonitoringHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly string _connectionString;
    private readonly Pbkdf2PasswordHasher _hasher = new();

    public MonitoringHandlerTests()
    {
        var databaseName = $"RealTimeSystemAppTests_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(_connectionString).Options;
        _db = new ApplicationDbContext(options);
        _db.Database.EnsureDeleted();
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Login_ReturnsToken_ForValidPassword()
    {
        await SeedUserAsync("admin@monitor.local", "Admin123!", RoleNames.Admin);
        var handler = new LoginCommandHandler(CreateUnitOfWork(), _hasher, new StubIssuer());

        var response = await handler.Handle(new LoginCommand("admin@monitor.local", "Admin123!"), CancellationToken.None);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Contains(RoleNames.Admin, response.User.Roles);
    }

    [Fact]
    public async Task Login_RejectsUnknownPassword()
    {
        await SeedUserAsync("admin@monitor.local", "Admin123!", RoleNames.Admin);
        var handler = new LoginCommandHandler(CreateUnitOfWork(), _hasher, new StubIssuer());

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            handler.Handle(new LoginCommand("admin@monitor.local", "WrongPass1"), CancellationToken.None));
    }

    [Fact]
    public async Task GetServers_FiltersSortsAndPages()
    {
        _db.Servers.AddRange(
            new MonitoredServer { Name = "zeta", HostName = "10.0.0.3", EnvironmentName = "Production", CreatedAtUtc = DateTime.UtcNow },
            new MonitoredServer { Name = "alpha", HostName = "10.0.0.1", EnvironmentName = "Staging", CreatedAtUtc = DateTime.UtcNow },
            new MonitoredServer { Name = "middle", HostName = "10.1.0.2", EnvironmentName = "Production", CreatedAtUtc = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        var handler = new GetServersQueryHandler(CreateUnitOfWork());

        var page = await handler.Handle(new GetServersQuery
        {
            Page = 1,
            PageSize = 1,
            Environment = "Production",
            SortBy = "name"
        }, CancellationToken.None);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal("middle", page.Items[0].Name);
    }

    [Fact]
    public async Task PasswordHasher_VerifiesOnlyTheOriginalPassword()
    {
        var hash = _hasher.Hash("Admin123!");

        Assert.True(_hasher.Verify("Admin123!", hash));
        Assert.False(_hasher.Verify("Admin123?", hash));
    }

    [Fact]
    public async Task ValidationBehavior_StopsInvalidLogin()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        await using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var exception = await Assert.ThrowsAsync<ValidationAppException>(() =>
            sender.Send(new LoginCommand("not-an-email", "short"), CancellationToken.None));

        Assert.True(exception.Errors.ContainsKey("Email"));
        Assert.True(exception.Errors.ContainsKey("Password"));
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private async Task SeedUserAsync(string email, string password, string roleName)
    {
        var role = new Role { Name = roleName };
        var user = new User
        {
            Email = email,
            FullName = "Test User",
            PasswordHash = _hasher.Hash(password),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.UserRoles.Add(new UserRole { Role = role });
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    private IUnitOfWork CreateUnitOfWork() => new EfUnitOfWork(_db);

    private sealed class StubIssuer : IAuthTokenIssuer
    {
        public Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken)
        {
            var roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
            return Task.FromResult(new AuthResponse(
                "access-token",
                DateTime.UtcNow.AddMinutes(5),
                "refresh-token",
                DateTime.UtcNow.AddDays(1),
                new UserProfileDto(user.Id, user.Email, user.FullName, roles)));
        }
    }
}

using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeSystem.Application.Common.Interfaces;
using RealTimeSystem.Application.Common.Dtos;

namespace RealTimeSystem.WebAPI.Hubs
{
public interface IPresenceTracker
{
    IReadOnlyList<PresenceUser> Connect(string connectionId, long userId, string name);

    IReadOnlyList<PresenceUser> Disconnect(string connectionId);
}

public sealed record PresenceUser(long UserId, string Name, int Connections);

public sealed class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<string, (long UserId, string Name)> _connections = new();

    public IReadOnlyList<PresenceUser> Connect(string connectionId, long userId, string name)
    {
        _connections[connectionId] = (userId, name);
        return Snapshot();
    }

    public IReadOnlyList<PresenceUser> Disconnect(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
        return Snapshot();
    }

    private IReadOnlyList<PresenceUser> Snapshot() =>
        _connections.Values
            .GroupBy(connection => connection.UserId)
            .Select(group => new PresenceUser(group.Key, group.First().Name, group.Count()))
            .OrderBy(user => user.Name)
            .ToList();
}

[Authorize]
public sealed class MonitoringHub : Hub
{
    public const string DashboardGroup = "dashboard";
    private readonly IPresenceTracker _presence;
    private readonly ILogger<MonitoringHub> _logger;

    public MonitoringHub(IPresenceTracker presence, ILogger<MonitoringHub> logger)
    {
        _presence = presence;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var startDate = DateTime.UtcNow;
        Context.Items["StartDate"] = startDate;
        await Groups.AddToGroupAsync(Context.ConnectionId, DashboardGroup);
        var userId = long.TryParse(Context.User?.FindFirstValue("sub"), out var parsed) ? parsed : 0;
        var name = Context.User?.FindFirstValue("name") ?? "Operator";
        var snapshot = _presence.Connect(Context.ConnectionId, userId, name);
        await Clients.Group(DashboardGroup).SendAsync("PresenceUpdated", snapshot);
        _logger.LogInformation(
            "SignalR connected {ConnectionId} for {UserName}. StartDate {StartDate:o}",
            Context.ConnectionId,
            name,
            startDate);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var endDate = DateTime.UtcNow;
        var startDate = Context.Items.TryGetValue("StartDate", out var value) && value is DateTime started
            ? started
            : endDate;
        var snapshot = _presence.Disconnect(Context.ConnectionId);
        await Clients.Group(DashboardGroup).SendAsync("PresenceUpdated", snapshot);
        _logger.LogInformation(
            "SignalR disconnected {ConnectionId}. StartDate {StartDate:o} EndDate {EndDate:o}",
            Context.ConnectionId,
            startDate,
            endDate);
        await base.OnDisconnectedAsync(exception);
    }

    public Task WatchServer(long serverId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"server:{serverId}");
}

public sealed class SignalRMonitoringNotifier : IMonitoringNotifier
{
    private readonly IHubContext<MonitoringHub> _hubContext;

    public SignalRMonitoringNotifier(IHubContext<MonitoringHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task MetricUpdatedAsync(LiveMetricDto metric, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(MonitoringHub.DashboardGroup).SendAsync("MetricUpdated", metric, cancellationToken);

    public Task AlertRaisedAsync(AlertDto alert, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(MonitoringHub.DashboardGroup).SendAsync("AlertRaised", alert, cancellationToken);

    public Task ReportUpdatedAsync(ReportDto report, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(MonitoringHub.DashboardGroup).SendAsync("ReportUpdated", report, cancellationToken);

    public Task MaintenanceScheduledAsync(MaintenanceNoticeDto notice, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(MonitoringHub.DashboardGroup).SendAsync("MaintenanceScheduled", notice, cancellationToken);
}
}

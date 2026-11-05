using Mapster;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Common.Mapping
{

    public static class ServerMappings
    {
        private static readonly TypeAdapterConfig Config = BuildConfig();

        public static readonly Expression<Func<MonitoredServer, ServerDto>> ServerDtoProjection = server =>
            new ServerDto(
                server.Id,
                server.Name,
                server.HostName,
                server.EnvironmentName,
                server.Description,
                server.Status.ToString(),
                server.IsActive,
                server.CreatedAtUtc,
                server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.CpuPercent).FirstOrDefault(),
                server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.MemoryPercent).FirstOrDefault(),
                server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.DiskPercent).FirstOrDefault(),
                server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.NetworkMbps).FirstOrDefault(),
                server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.ResponseTimeMs).FirstOrDefault(),
                server.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (DateTime?)metric.CollectedAtUtc).FirstOrDefault(),
                server.Alerts.Count(alert => !alert.IsAcknowledged));

        public static IQueryable<ServerDto> ProjectServer(this IQueryable<MonitoredServer> query) =>
            query.ProjectToType<ServerDto>(Config);

        private static TypeAdapterConfig BuildConfig()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<MonitoredServer, ServerDto>()
                .Map(destination => destination.Status, source => source.Status.ToString())
                .Map(destination => destination.CpuPercent, source => source.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.CpuPercent).FirstOrDefault())
                .Map(destination => destination.MemoryPercent, source => source.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.MemoryPercent).FirstOrDefault())
                .Map(destination => destination.DiskPercent, source => source.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.DiskPercent).FirstOrDefault())
                .Map(destination => destination.NetworkMbps, source => source.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.NetworkMbps).FirstOrDefault())
                .Map(destination => destination.ResponseTimeMs, source => source.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (double?)metric.ResponseTimeMs).FirstOrDefault())
                .Map(destination => destination.CollectedAtUtc, source => source.Metrics.OrderByDescending(metric => metric.CollectedAtUtc).Select(metric => (DateTime?)metric.CollectedAtUtc).FirstOrDefault())
                .Map(destination => destination.OpenAlertCount, source => source.Alerts.Count(alert => !alert.IsAcknowledged));

            return config;
        }
    }
}

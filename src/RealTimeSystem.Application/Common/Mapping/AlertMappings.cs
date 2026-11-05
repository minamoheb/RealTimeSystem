using Mapster;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Common.Mapping
{
    public static class AlertMappings
    {
        private static readonly TypeAdapterConfig Config = BuildConfig();

        public static readonly Expression<Func<Alert, AlertDto>> AlertDtoProjection = alert =>
            new AlertDto(
                alert.Id,
                alert.ServerId,
                alert.Server.Name,
                alert.MetricName,
                alert.Value,
                alert.Threshold,
                alert.Severity.ToString(),
                alert.Message,
                alert.IsAcknowledged,
                alert.CreatedAtUtc,
                alert.AcknowledgedAtUtc);

        public static AlertDto ToDto(this Alert alert) => alert.Adapt<AlertDto>(Config);

        private static TypeAdapterConfig BuildConfig()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<Alert, AlertDto>()
                .Map(destination => destination.ServerName, source => source.Server.Name)
                .Map(destination => destination.Severity, source => source.Severity.ToString());

            return config;
        }
    }
}

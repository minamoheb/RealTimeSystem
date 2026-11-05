using Mapster;
using System.Linq.Expressions;

namespace RealTimeSystem.Application.Common.Mapping
{
    public static class ReportMappings
    {
        private static readonly TypeAdapterConfig Config = BuildConfig();

        public static readonly Expression<Func<Report, ReportDto>> ReportDtoProjection = report =>
            new ReportDto(
                report.Id,
                report.Title,
                report.FromUtc,
                report.ToUtc,
                report.Status.ToString(),
                report.Summary,
                report.FailureReason,
                report.RequestedAtUtc,
                report.CompletedAtUtc,
                report.RequestedByEmail);

        public static ReportDto ToDto(this Report report) => report.Adapt<ReportDto>(Config);

        private static TypeAdapterConfig BuildConfig()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<Report, ReportDto>()
                .Map(destination => destination.Status, source => source.Status.ToString());

            return config;
        }
    }
}

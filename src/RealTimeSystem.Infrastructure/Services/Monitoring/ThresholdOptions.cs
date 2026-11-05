namespace RealTimeSystem.Infrastructure.Services.Monitoring
{
    public sealed class ThresholdOptions
    {
        public const string SectionName = "Thresholds";

        public double CpuWarning { get; set; } = 80;

        public double CpuCritical { get; set; } = 90;

        public double MemoryWarning { get; set; } = 80;

        public double MemoryCritical { get; set; } = 90;

        public double DiskWarning { get; set; } = 85;

        public double DiskCritical { get; set; } = 95;

        public double ResponseWarning { get; set; } = 400;

        public double ResponseCritical { get; set; } = 800;
    }
}

namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface IMetricSampleFactory
    {
        MetricSample Create(long serverId, MetricSample? previous, DateTime utcNow);
    }
}
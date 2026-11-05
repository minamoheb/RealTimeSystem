namespace RealTimeSystem.Application.Features.Jobs
{
    public sealed record CollectMetricsCommand : IRequest<bool>;
    public sealed class CollectMetricsCommandHandler(IBackgroundJobScheduler scheduler) : IRequestHandler<CollectMetricsCommand, bool>
    {
        public Task<bool> Handle(CollectMetricsCommand request, CancellationToken cancellationToken)
        {
            scheduler.EnqueueMetricCollection();
            return Task.FromResult(true);
        }
    }
}


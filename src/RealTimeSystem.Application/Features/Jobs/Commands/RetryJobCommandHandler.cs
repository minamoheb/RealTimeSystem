namespace RealTimeSystem.Application.Features.Jobs
{
    public sealed record RetryJobCommand(string JobId) : IRequest<bool>;

    public sealed class RetryJobCommandHandler(IBackgroundJobScheduler scheduler) : IRequestHandler<RetryJobCommand, bool>
    {
        public Task<bool> Handle(RetryJobCommand request, CancellationToken cancellationToken)
        {
            if (!scheduler.Requeue(request.JobId))
            {
                throw new ConflictException("Only a failed Hangfire job can be retried.");
            }

            return Task.FromResult(true);
        }
    }
}

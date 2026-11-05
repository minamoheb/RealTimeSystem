namespace RealTimeSystem.Application.Common.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
    }
}


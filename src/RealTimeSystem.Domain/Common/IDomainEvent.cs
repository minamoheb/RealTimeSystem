namespace RealTimeSystem.Domain.Common
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }

    public interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        void ClearDomainEvents();
    }
}


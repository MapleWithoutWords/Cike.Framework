namespace Cike.Data.Domain.AggregateRoots;

public abstract record DomainEvent : Event, IDomainEvent
{
}

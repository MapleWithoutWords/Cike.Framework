namespace Cike.Data.Domain.AggregateRoots;

public interface IAggregateRoot : IEntity
{
    IEnumerable<IDomainEvent> DomainEvents { get; }

    void AddDomainEvent(IDomainEvent domainEvent);

    void ClearDomainEvents();
}

public interface IAggregateRoot<TKey> : IEntity<TKey>, IAggregateRoot
{

}

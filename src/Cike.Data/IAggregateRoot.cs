namespace Cike.Data;

public interface IAggregateRoot : IEntity
{
    IEnumerable<IDomainEvent> DomainEvents { get; }

    void AddDomainEvent(IDomainEvent domainEvent);

    void ClearDomainEvents();
}

public interface IAggregateRoot<TKey> : IEntity<TKey>, IAggregateRoot
{

}

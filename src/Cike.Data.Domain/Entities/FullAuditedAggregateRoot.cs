using Cike.Data.Domain.AggregateRoots;

namespace Cike.Data.Domain.Entities;

public class FullAuditedAggregateRoot<TKey> : FullAuditedAggregateRoot<TKey, long>
{
}

public class FullAuditedAggregateRoot<TKey, TUserId> : FullAuditedEntity<TKey, TUserId>, IFullAuditedAggregateRoot<TKey, TUserId>, IHasConcurrencyStamp where TUserId : struct
{
    private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

    public IEnumerable<IDomainEvent> DomainEvents => _events;

    public virtual string ConcurrencyStamp { get; set; } = default!;

    public FullAuditedAggregateRoot()
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _events.Clear();
    }
}

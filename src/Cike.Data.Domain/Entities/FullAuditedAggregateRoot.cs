namespace Cike.Data.Domain.Entities;

public class FullAuditedAggregateRoot<TKey> : FullAuditedAggregateRoot<TKey, long>
{
}

public class FullAuditedAggregateRoot<TKey, TUserId> : FullAuditedEntity<TKey, TUserId>, IHasConcurrencyStamp where TUserId : struct
{
    public virtual string ConcurrencyStamp { get; set; } = default!;

    public FullAuditedAggregateRoot()
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}

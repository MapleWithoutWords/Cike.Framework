namespace Cike.Domain.Entities;

public class AuditedEntity<TKey, TUserId> : Entity<TKey>, IAuditedEntity<TUserId>
{
    public DateTime CreateTime { get; set; }

    public TUserId CreateUserId { get; set; } = default!;

    public DateTime UpdateTime { get; set; }

    public TUserId UpdateUserId { get; set; } = default!;
}

public class AuditedEntity<TKey> : AuditedEntity<TKey, long>, IAuditedEntity<long>
{
}

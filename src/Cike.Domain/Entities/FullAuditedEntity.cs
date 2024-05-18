using Cike.Data;

namespace Cike.Domain.Entities;

public class FullAuditedEntity<TKey, TUserId> : AuditedEntity<TKey, TUserId>, ISoftDelete
{
    public bool IsDeleted { get; set; }
}

public class FullAuditedEntity<TKey> : AuditedEntity<TKey>, ISoftDelete
{
    public bool IsDeleted { get; set; }
}
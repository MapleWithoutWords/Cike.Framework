using Cike.Data;

namespace Cike.Domain.Entities;

public class FullAuditedEntity<TKey, TUserId> : AuditedEntity<TKey, TUserId>, IFullAuditedEntity<TUserId>
{
    public bool IsDeleted { get; set; } = false;
}

public class FullAuditedEntity<TKey> : FullAuditedEntity<TKey, long>
{
}
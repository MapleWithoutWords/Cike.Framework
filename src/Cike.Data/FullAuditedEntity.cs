namespace Cike.Data;

public class FullAuditedEntity<TKey, TUserId> : AuditedEntity<TKey, TUserId>
{
    public bool IsDeleted { get; set; }
}

public class FullAuditedEntity<TKey> : AuditedEntity<TKey>
{
    public bool IsDeleted { get; set; }
}
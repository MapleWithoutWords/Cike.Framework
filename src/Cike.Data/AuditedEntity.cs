namespace Cike.Data;

public class AuditedEntity<TKey, TUserId> : Entity<TKey>, IAuditedEntity<TUserId>
{
    public DateTime CreateTime { get; set; }
    public TUserId CreateUserId { get; set; } = default!;
    public DateTime UpdateTime { get; set; }
    public TUserId UpdateUserId { get; set; } = default!;
}
public class AuditedEntity<TKey> : Entity<TKey>, IAuditedEntity<Guid>
{
    public DateTime CreateTime { get; set; }
    public Guid CreateUserId { get; set; } = default!;
    public DateTime UpdateTime { get; set; }
    public Guid UpdateUserId { get; set; } = default!;
}

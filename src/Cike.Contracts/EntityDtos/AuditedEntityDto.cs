namespace Cike.Contracts.EntityDtos;

public class AuditedEntityDto<TKey, TUserId> : EntityDto<TKey>
{
    public DateTime CreatedAt { get; set; }
    public TUserId CreatedBy { get; set; } = default!;
    public DateTime UpdatedAt { get; set; }
    public TUserId UpdatedBy { get; set; } = default!;
}

public class AuditedEntityDto<TKey> : AuditedEntityDto<TKey, long>
{
}

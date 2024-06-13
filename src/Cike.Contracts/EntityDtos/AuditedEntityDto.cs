namespace Cike.Contracts.EntityDtos;

public class AuditedEntityDto<TKey, TUserId> : EntityDto<TKey>
{
    public DateTime CreateTime { get; set; }
    public TUserId CreateUserId { get; set; } = default!;
    public DateTime UpdateTime { get; set; }
    public TUserId UpdateUserId { get; set; } = default!;
}

public class AuditedEntityDto<TKey> : AuditedEntityDto<TKey, long>
{
}

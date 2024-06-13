namespace Cike.Contracts.EntityDtos;

public class FullAuditedEntityDto<TKey, TUserId> : AuditedEntityDto<TKey, TUserId>
{
    public bool IsDeleted { get; set; }
}

public class FullAuditedEntityDto<TKey> : FullAuditedEntityDto<TKey, long>
{
}

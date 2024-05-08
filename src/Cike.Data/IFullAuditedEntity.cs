namespace Cike.Data;

public interface IFullAuditedEntity<TKey, TUserId> : IAuditedEntity<TUserId>, ISoftDelete
{
}

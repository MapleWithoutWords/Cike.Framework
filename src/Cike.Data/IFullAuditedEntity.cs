namespace Cike.Data;

public interface IFullAuditedEntity<TKey, TUserId> : IAuditedEntity<TKey, TUserId>, ISoftDelete
{
}

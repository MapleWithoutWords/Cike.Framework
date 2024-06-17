namespace Cike.Data;

public interface IFullAuditedEntity<TUserId> : IAuditedEntity<TUserId>, ISoftDelete
{
}

namespace Cike.Data;

public interface IAuditedEntity<TKey,TUserId>:IEntity<TKey>
{
    DateTime CreateTime { get; set; }
    TUserId CreateUserId { get; set; }
    DateTime UpdateTime { get; set; }
    TUserId UpdateUserId { get; set; }
}

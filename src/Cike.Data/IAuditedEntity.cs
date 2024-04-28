namespace Cike.Data;

public interface IAuditedEntity<TUserId>
{
    DateTime CreateTime { get; set; }
    TUserId CreateUserId { get; set; }
    DateTime UpdateTime { get; set; }
    TUserId UpdateUserId { get; set; }
}

namespace Cike.Data;

public interface ICreateAuditedEntity<TUserId> where TUserId : struct
{
    DateTime CreateTime { get; set; }

    TUserId CreateUserId { get; set; }
}

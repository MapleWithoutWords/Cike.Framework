namespace Cike.Data;

public interface IAuditedEntity<TUserId> : ICreateAuditedEntity<TUserId> where TUserId : struct
{
    DateTime UpdateTime { get; set; }

    TUserId UpdateUserId { get; set; }
}

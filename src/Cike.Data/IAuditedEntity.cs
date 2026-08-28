namespace Cike.Data;

public interface IAuditedEntity<TUserId> : ICreateAuditedEntity<TUserId> where TUserId : struct
{
    DateTime UpdatedAt { get; set; }

    TUserId UpdatedBy { get; set; }
}

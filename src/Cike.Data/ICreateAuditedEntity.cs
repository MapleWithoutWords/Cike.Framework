namespace Cike.Data;

public interface ICreateAuditedEntity<TUserId> where TUserId : struct
{
    DateTime CreatedAt { get; set; }

    TUserId CreatedBy { get; set; }
}

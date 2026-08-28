namespace Cike.Data.Domain.AggregateRoots;

public interface IFullAuditedAggregateRoot<TKey, TUserId> : IFullAuditedEntity<TUserId>, IEntity<TKey>, IAggregateRoot<TKey> where TUserId : struct
{
}

public interface IFullAuditedAggregateRoot<TKey> : IFullAuditedAggregateRoot<TKey, long>
{
}

namespace Cike.Domain.Repositories;

/// <summary>
/// 只读仓储：查询侧最小契约。CQRS 查询侧注入本接口可在编译期保证无写操作。
/// </summary>
public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// 按主键查询单个实体，不存在时抛出 <see cref="Cike.Core.Exceptions.UserFriendlyException"/>（"必须存在"场景）。
    /// </summary>
    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按主键查询单个实体，不存在时返回 null（"可能不存在"场景）。
    /// </summary>
    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);
}

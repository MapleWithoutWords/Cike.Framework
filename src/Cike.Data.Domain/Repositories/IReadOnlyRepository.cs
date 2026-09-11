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

    /// <summary>
    /// 查询全部实体列表。
    /// </summary>
    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 按条件查询实体列表。
    /// </summary>
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页排序查询，一次返回总数与当页数据。Sorting 为 System.Linq.Dynamic 语法的排序字符串。
    /// </summary>
    Task<(long Total, List<TEntity> Items)> GetPagedListAsync(IPagedAndSortedRequest request, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 计数。
    /// </summary>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 按条件计数。
    /// </summary>
    Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 存在性判断（无谓词时等价于"任意一行存在"）。
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

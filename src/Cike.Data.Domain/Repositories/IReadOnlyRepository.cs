namespace Cike.Domain.Repositories;

/// <summary>
/// 只读仓储：查询侧完整契约。CQRS 查询侧注入本接口可在编译期保证无写操作。
/// 方法面与 ABP 对齐：按 Id / 谓词查询、includeDetails 导航加载、双形态分页、计数存在性、IQueryable 出口。
/// </summary>
public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// 按主键查询单个实体，不存在时抛出 <see cref="Cike.Core.Exceptions.UserFriendlyException"/>（"必须存在"场景）。
    /// includeDetails 默认 true：加载全部一级导航属性。
    /// </summary>
    Task<TEntity> GetAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按主键查询单个实体，不存在时返回 null（"可能不存在"场景）。
    /// </summary>
    Task<TEntity?> FindAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询全部实体列表（默认不加载导航属性）。
    /// </summary>
    Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按条件查询实体列表。
    /// </summary>
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页排序查询（请求对象形态），一次返回总数与当页数据。Sorting 为 System.Linq.Dynamic 语法的排序字符串。
    /// </summary>
    Task<(long Total, List<TEntity> Items)> GetPagedListAsync(IPagedAndSortedRequest request, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询（skip/take 形态，ABP 风格）：跳过 skipCount 条、取 maxResultCount 条，按 sorting 排序（可空）。
    /// </summary>
    Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// 获取可查询源：仓储方法不够用时做任意 LINQ / Include 组合。
    /// 挂在仓储背后的 scoped DbContext 上，软删/多租户全局过滤器自动生效；scope 释放后不可再物化。
    /// </summary>
    Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取已加载全部一级导航属性的可查询源。
    /// </summary>
    Task<IQueryable<TEntity>> WithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取按指定导航属性路径加载的可查询源。
    /// </summary>
    Task<IQueryable<TEntity>> WithDetailsAsync(params Expression<Func<TEntity, object?>>[] propertyPaths);
}

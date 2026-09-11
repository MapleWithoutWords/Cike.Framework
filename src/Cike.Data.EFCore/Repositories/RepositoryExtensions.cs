namespace Cike.Data.EFCore.Repositories;

/// <summary>
/// EF Core 仓储的出口扩展：仓储方法不够用时，经 IQueryable 做导航属性加载与任意复杂查询，
/// 或取强类型 DbContext 触达仓储未覆盖的 EF 能力（原生 SQL 等）。全局过滤器（软删/多租户）依然生效。
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// 从仓储获取可查询源（同步完成，无需异步等待）。返回的 IQueryable 挂在仓储背后的 scoped DbContext 上：
    /// 软删/多租户全局查询过滤器自动生效；scope 释放后不可再物化。取消令牌请在物化处
    /// （ToListAsync / FirstOrDefaultAsync 等）传入。
    /// </summary>
    public static Task<IQueryable<TEntity>> GetQueryableAsync<TEntity, TKey>(this IReadOnlyRepository<TEntity, TKey> repository)
        where TEntity : class, IEntity<TKey>
    {
        return Task.FromResult<IQueryable<TEntity>>(GetDbContext(repository).Set<TEntity>());
    }

    /// <summary>
    /// 获取仓储背后的强类型 DbContext。
    /// 类型参数需全部显式（C# 不支持只显式前缀、其余推断）：await repository.GetDbContextAsync&lt;TDbContext, TEntity, TKey&gt;()
    /// </summary>
    public static Task<TDbContext> GetDbContextAsync<TDbContext, TEntity, TKey>(this IReadOnlyRepository<TEntity, TKey> repository)
        where TDbContext : DbContext
        where TEntity : class, IEntity<TKey>
    {
        var dbContext = GetDbContext(repository);
        if (dbContext is not TDbContext typed)
        {
            throw new InvalidCastException(
                $"仓储背后的 DbContext 是 {dbContext.GetType().Name}，无法转换为 {typeof(TDbContext).Name}。" +
                "请确认仓储来自正确的 TDbContext 注册。");
        }
        return Task.FromResult(typed);
    }

    private static DbContext GetDbContext(object repository)
    {
        if (repository is not IEfCoreRepositoryAccessor accessor)
        {
            throw new NotSupportedException(
                $"仓储实现 {repository.GetType().Name} 未实现 {nameof(IEfCoreRepositoryAccessor)}，" +
                "无法获取底层 DbContext。默认 EfCoreRepository 与其子类均支持。");
        }
        return accessor.DbContext;
    }
}

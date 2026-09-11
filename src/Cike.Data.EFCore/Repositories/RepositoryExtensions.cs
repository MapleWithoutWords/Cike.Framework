namespace Cike.Data.EFCore.Repositories;

/// <summary>
/// EF Core 仓储的出口扩展。IQueryable 相关出口（GetQueryableAsync / WithDetailsAsync）已收敛为
/// IReadOnlyRepository 接口成员，见 Cike.Data.Domain；此处仅保留依赖 EF 类型的扩展。
/// </summary>
public static class RepositoryExtensions
{
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

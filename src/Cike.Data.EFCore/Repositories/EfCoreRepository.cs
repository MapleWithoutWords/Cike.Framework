namespace Cike.Data.EFCore.Repositories;

/// <summary>
/// EF Core 默认仓储实现。不实现任何 DI 标记接口——默认仓储由 AddCikeDbContext 显式注册，
/// 自定义仓储由业务方继承本类并标注 IScopedDependency 走约定注册。
/// </summary>
public class EfCoreRepository<TDbContext, TEntity, TKey>(TDbContext dbContext) : IReadOnlyRepository<TEntity, TKey>, IEfCoreRepositoryAccessor
    where TDbContext : CikeDbContext<TDbContext>
    where TEntity : class, IEntity<TKey>
{
    public TDbContext DbContext { get; } = dbContext;

    DbContext IEfCoreRepositoryAccessor.DbContext => DbContext;

    public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new UserFriendlyException($"Id {id} is NotFound.");
        }
        return entity;
    }

    public async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<(long Total, List<TEntity> Items)> GetPagedListAsync(IPagedAndSortedRequest request, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbContext.Set<TEntity>();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        return await query.ToPaginationAsync(request, cancellationToken);
    }

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().LongCountAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>().LongCountAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await DbContext.Set<TEntity>().AnyAsync(cancellationToken);
        }
        return await DbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }
}

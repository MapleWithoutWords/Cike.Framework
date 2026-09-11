namespace Cike.Data.EFCore.Repositories;

/// <summary>
/// EF Core 默认仓储实现。不实现任何 DI 标记接口——默认仓储由 AddCikeDbContext 显式注册，
/// 自定义仓储由业务方继承本类并标注 IScopedDependency 走约定注册。
/// </summary>
public class EfCoreRepository<TDbContext, TEntity, TKey>(TDbContext dbContext) : IRepository<TEntity, TKey>, IEfCoreRepositoryAccessor
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

    public async Task<TEntity> InsertAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        await SaveChangesIfAsync(autoSave, cancellationToken);
        return entity;
    }

    public async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        await SaveChangesIfAsync(autoSave, cancellationToken);
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().Update(entity);
        await SaveChangesIfAsync(autoSave, cancellationToken);
        return entity;
    }

    public async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().UpdateRange(entities);
        await SaveChangesIfAsync(autoSave, cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        // 软删转换由 CikeDbContext 的 ChangeTracker 钩子完成，仓储只负责 Remove
        DbContext.Set<TEntity>().Remove(entity);
        await SaveChangesIfAsync(autoSave, cancellationToken);
    }

    public async Task DeleteAsync(TKey id, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity == null)
        {
            // 幂等：实体不存在时静默返回
            return;
        }
        await DeleteAsync(entity, autoSave, cancellationToken);
    }

    public async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbContext.Set<TEntity>().RemoveRange(entities);
        await SaveChangesIfAsync(autoSave, cancellationToken);
    }

    /// <summary>
    /// autoSave 为 true 时立即保存；为 false 时变更挂起，由工作单元提交时统一保存。
    /// </summary>
    protected virtual async Task SaveChangesIfAsync(bool autoSave, CancellationToken cancellationToken = default)
    {
        if (autoSave)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

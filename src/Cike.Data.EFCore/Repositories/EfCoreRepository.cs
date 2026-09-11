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

    public async Task<TEntity> GetAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, includeDetails, cancellationToken);
        if (entity == null)
        {
            throw new UserFriendlyException($"Id {id} is NotFound.");
        }
        return entity;
    }

    public async Task<TEntity?> FindAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludeDetails(DbContext.Set<TEntity>(), includeDetails)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludeDetails(DbContext.Set<TEntity>(), includeDetails)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await ApplyIncludeDetails(DbContext.Set<TEntity>(), includeDetails)
            .Where(predicate)
            .ToListAsync(cancellationToken);
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

    public async Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbContext.Set<TEntity>();
        if (predicate != null)
        {
            query = query.Where(predicate);
        }
        if (!string.IsNullOrWhiteSpace(sorting))
        {
            query = query.OrderBy(sorting);
        }
        return await query
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
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

    public Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IQueryable<TEntity>>(DbContext.Set<TEntity>());
    }

    public Task<IQueryable<TEntity>> WithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApplyIncludeDetails(DbContext.Set<TEntity>(), includeDetails: true));
    }

    public Task<IQueryable<TEntity>> WithDetailsAsync(params Expression<Func<TEntity, object?>>[] propertyPaths)
    {
        IQueryable<TEntity> query = DbContext.Set<TEntity>();
        foreach (var propertyPath in propertyPaths)
        {
            query = query.Include(propertyPath);
        }
        return Task.FromResult(query);
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
        var entity = await FindAsync(id, includeDetails: false, cancellationToken);
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
    /// includeDetails 为 true 时加载全部一级导航属性（ABP 的 IncludeDetails 语义）。
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyIncludeDetails(IQueryable<TEntity> query, bool includeDetails)
    {
        if (!includeDetails)
        {
            return query;
        }

        var entityType = DbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType == null)
        {
            return query;
        }

        foreach (var navigation in entityType.GetNavigations())
        {
            query = query.Include(navigation.Name);
        }
        return query;
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

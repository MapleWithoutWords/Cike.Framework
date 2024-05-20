using Cike.Core.DependencyInjection;
using Cike.Core.Exceptions;
using Cike.Data;
using Cike.Data.EFCore;
using Cike.Domain.Entities;
using Cike.Domain.Repositories;
using Cike.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Cike.Data.EFCore.Repositories;

public class EFCoreRepository<TDbContext, TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TDbContext : CikeDbContext<TDbContext>
{
    protected TDbContext DbContext { get; set; }
    protected IUnitOfWork UnitOfWork { get; set; }
    public EFCoreRepository(IServiceProvider serviceProvider)
    {
        DbContext = serviceProvider.GetRequiredService<TDbContext>();
        UnitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    }
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var result = await DbContext.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbContext.AddRangeAsync(entities, cancellationToken);
    }

    public async Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var entity = await DbContext.Set<TEntity>().FindAsync(key);
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        await DeleteRangeAsync([entity], cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DeleteRangeAsync([entity], cancellationToken);
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
    {
        var entities = await DbContext.Set<TEntity>().Where(expression).ToListAsync();
        await DeleteRangeAsync(entities, cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            if (item is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                await UpdateAsync(item);
            }
            else
            {
                DbContext.Remove(item);
            }
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(e => keys.Contains(e.Id), cancellationToken);
    }

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await GetQueryable().FirstOrDefaultAsync(filter, cancellationToken);
    }

    public async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await FindAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var result = await FindAsync(filter);
        if (result == null)
        {
            throw new FriendlyException($"Entity Not Found.");
        }
        return result;
    }

    public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await GetAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable().ToListAsync();
    }

    public async Task<IReadOnlyCollection<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await GetQueryable().Where(filter).ToListAsync();
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return DbContext.Set<TEntity>();
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var result = DbContext.Update(entity);
        return await Task.FromResult(result.Entity);
    }

    public Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbContext.UpdateRange(entities, cancellationToken);
        return Task.CompletedTask;
    }
}

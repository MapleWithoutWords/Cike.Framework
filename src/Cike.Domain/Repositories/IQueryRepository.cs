using Cike.Data;
using System.Linq.Expressions;

namespace Cike.Domain.Repositories;

public interface IQueryRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    IQueryable<TEntity> GetQueryable();

    Task<IReadOnlyCollection<TEntity>> GetListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);
}

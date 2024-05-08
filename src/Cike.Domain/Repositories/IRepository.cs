using Cike.Data;
using System.Linq.Expressions;

namespace Cike.Domain.Repositories;

public interface IRepository<TEntity, TKey> : IQueryRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey key, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<TEntity> entitie, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default);
}

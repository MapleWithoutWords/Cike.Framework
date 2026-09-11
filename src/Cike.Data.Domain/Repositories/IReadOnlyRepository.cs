using System.Linq.Expressions;

namespace Cike.Domain.Repositories;

public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    IDisposable BeginAsNoTracking();

    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<(long Total, List<TEntity> Items)> GetPagedListAsync(IPagedAndSortedRequest request, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    IQueryable<TEntity> GetQueryable();

    Task<IQueryable<TEntity>> WithDetailsAsync(params Expression<Func<TEntity, object?>>[] propertyPaths);
}

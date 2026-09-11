namespace Cike.Contracts.Extensions;

public static class IQueryablePaginationExtensions
{
    public static async Task<(long Total, List<TEntity> Items)> ToPaginationAsync<TEntity>(this IQueryable<TEntity> query, IPagedAndSortedRequest pageAndSorted, CancellationToken cancellationToken = default)
    {
        var total = await query.LongCountAsync();
        var items = new List<TEntity>();
        if (total > 0)
        {
            if (!pageAndSorted.Sorting.IsNullOrEmpty())
            {
                query = query.OrderBy(pageAndSorted.Sorting);
            }
            if (pageAndSorted.PageSize > 0)
            {
                items = await query.Skip((pageAndSorted.Page - 1) * pageAndSorted.PageSize).Take(pageAndSorted.PageSize).ToListAsync();
            }
            else
            {
                items = await query.ToListAsync(cancellationToken);
            }
        }
        return (total, items);
    }

    public static async Task<TEntity> GetAsync<TEntity, TKey>(this IQueryable<TEntity> query, TKey id, CancellationToken cancellationToken = default) where TEntity : IEntity<TKey> where TKey : struct
    {
        var data = await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        if (data == null)
        {
            throw new UserFriendlyException($"Id {id} is NotFound.");
        }
        return data;
    }

    public static IQueryable<TEntity> WhereIf<TEntity>(this IQueryable<TEntity> query, bool ifExpression, Expression<Func<TEntity, bool>> whereExpression) where TEntity : class
    {
        if (ifExpression)
        {
            return query.Where(whereExpression);
        }
        return query;
    }

    /// <summary>
    /// 按开关应用跟踪行为。注意：不可带默认参数、不可裸调用同名的 EF 扩展——
    /// 内部裸调用 query.AsNoTracking() 会重载决议到本方法自身（默认参数 false），形成自递归后落到 AsTracking。
    /// </summary>
    public static IQueryable<TEntity> AsNoTracking<TEntity>(this IQueryable<TEntity> query, bool asNoTracking) where TEntity : class
    {
        if (asNoTracking)
        {
            return EntityFrameworkQueryableExtensions.AsNoTracking(query);
        }
        return EntityFrameworkQueryableExtensions.AsTracking(query);
    }
}

﻿namespace Cike.Contracts.Extensions;

public static class IQueryablePaginationExtensions
{
    public static async Task<(long Total, List<TEntity> Items)> ToPaginationAsync<TEntity>(this IQueryable<TEntity> query, IPagedAndSortedRequest pageAndSorted)
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
                items = await query.ToListAsync();
            }
        }
        return (total, items);
    }

    public static async Task<TEntity> GetAsync<TEntity, TKey>(this IQueryable<TEntity> query, TKey id) where TEntity : IEntity<TKey> where TKey : struct
    {
        var data = await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        if (data == null)
        {
            throw new UserFriendlyException($"Id {id} is NotFound.");
        }
        return data;
    }

    public static IQueryable<TEntity> WhereIf<TEntity>(this IQueryable<TEntity> query, Func<bool> ifExpression, Expression<Func<TEntity, bool>> whereExpression) where TEntity : class
    {
        if (ifExpression())
        {
            return query.Where(whereExpression);
        }
        return query;
    }
}

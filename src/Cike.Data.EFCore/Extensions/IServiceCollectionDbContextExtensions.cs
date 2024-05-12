using Cike.Data.EFCore;
using Cike.Data.EFCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cike.Data.Extensions;

public static class IServiceCollectionDbContextExtensions
{
    public static IServiceCollection AddCikeDbContext<TDbContext>(this IServiceCollection services) where TDbContext : CikeDbContext<TDbContext>
    {
        services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);

        typeof(TDbContext).GetProperties(System.Reflection.BindingFlags.Public).Where(e => e.PropertyType.IsGenericType && typeof(DbSet<>).IsAssignableFrom(e.PropertyType)).Select(e => e.PropertyType.GetGenericArguments().First()).ToList().ForEach(e =>
        {
            //TODO:添加仓储
        });

        return services;
    }
}

namespace Cike.Data.Extensions;

public static class IServiceCollectionDbContextExtensions
{
    public static IServiceCollection AddCikeDbContext<TDbContext>(this IServiceCollection services, bool addDefaultRepositories = true) where TDbContext : CikeDbContext<TDbContext>
    {
        services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);
        services.AddScoped<IUnitOfWork, EFCoreUnitOfWork<TDbContext>>();

        if (addDefaultRepositories)
        {
            AddDefaultRepositories<TDbContext>(services);
        }

        return services;
    }

    /// <summary>
    /// 为 DbContext 中每个实现了 IEntity&lt;TKey&gt; 的 DbSet 实体注册默认仓储（Scoped）。
    /// 多个 DbContext 含同一实体类型时，后注册者覆盖。
    /// </summary>
    private static void AddDefaultRepositories<TDbContext>(IServiceCollection services) where TDbContext : CikeDbContext<TDbContext>
    {
        foreach (var entityType in GetEntityTypes(typeof(TDbContext)))
        {
            // 领域层静态 EntityHelper（Cike.Domain），与本项目 internal EntityHelper 同名，全限定消歧
            var primaryKeyType = Cike.Domain.EntityHelper.FindPrimaryKeyType(entityType);
            if (primaryKeyType == null)
            {
                continue;
            }

            var implementationType = typeof(EfCoreRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, primaryKeyType);
            services.AddScoped(typeof(IReadOnlyRepository<,>).MakeGenericType(entityType, primaryKeyType), implementationType);
        }
    }

    public static IEnumerable<Type> GetEntityTypes(Type dbContextType)
    {
        return
            from property in dbContextType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            where
                ReflectionHelper.IsAssignableToGenericType(property.PropertyType, typeof(DbSet<>)) &&
                typeof(IEntity).IsAssignableFrom(property.PropertyType.GenericTypeArguments[0])
            select property.PropertyType.GenericTypeArguments[0];
    }
}

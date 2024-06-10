namespace Cike.Data.Extensions;

public static class IServiceCollectionDbContextExtensions
{
    public static IServiceCollection AddCikeDbContext<TDbContext>(this IServiceCollection services) where TDbContext : CikeDbContext<TDbContext>
    {
        services.TryAddTransient(DbContextOptionsFactory.Create<TDbContext>);

        GetEntityTypes(typeof(TDbContext)).ToList().ForEach(e =>
        {
            //添加仓储
            services.AddDefaultRepository(e, typeof(EFCoreRepository<,,>).MakeGenericType(typeof(TDbContext), e, EntityHelper.FindPrimaryKeyType(e)!));
        });

        services.AddScoped<IUnitOfWork, EFCoreUnitOfWork<TDbContext>>();

        return services;
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

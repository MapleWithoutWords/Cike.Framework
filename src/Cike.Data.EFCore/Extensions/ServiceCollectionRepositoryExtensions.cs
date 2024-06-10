namespace Cike.Data.EFCore.Extensions;

public static class ServiceCollectionRepositoryExtensions
{
    public static IServiceCollection AddDefaultRepository(
        this IServiceCollection services,
        Type entityType,
        Type repositoryImplementationType,
        bool replaceExisting = false)
    {

        var primaryKeyType = EntityHelper.FindPrimaryKeyType(entityType);
        if (primaryKeyType != null)
        {
            //IQueryRepository<TEntity, TKey>
            var readOnlyBasicRepositoryInterfaceWithPk = typeof(IQueryRepository<,>).MakeGenericType(entityType, primaryKeyType);
            if (readOnlyBasicRepositoryInterfaceWithPk.IsAssignableFrom(repositoryImplementationType))
            {
                RegisterService(services, readOnlyBasicRepositoryInterfaceWithPk, repositoryImplementationType, replaceExisting);

                //IRepository<TEntity, TKey>
                var repositoryInterfaceWithPk = typeof(IRepository<,>).MakeGenericType(entityType, primaryKeyType);
                if (repositoryInterfaceWithPk.IsAssignableFrom(repositoryImplementationType))
                {
                    RegisterService(services, repositoryInterfaceWithPk, repositoryImplementationType, replaceExisting);
                }
            }
        }

        return services;
    }

    private static void RegisterService(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        bool replaceExisting)
    {
        var descriptor = ServiceDescriptor.Transient(serviceType, implementationType);

        if (replaceExisting)
        {
            services.Replace(descriptor);
        }
        else
        {
            services.TryAdd(descriptor);
        }
    }
}

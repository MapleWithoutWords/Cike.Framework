using Cike.Core.DependencyInjection;
using Cike.Core.Modularity;
using Microsoft.Extensions.Hosting;

namespace Cike.Core.Extensions;

public static class IHostApplicationBuilderExtensions
{
    public static void AddApplication<TStartupModule>(
        this IHostApplicationBuilder builder)
        where TStartupModule : CikeModule
    {
        var services = builder.Services;
        services.AddSingleton<IModuleLoader>(new ModuleLoader());

        var moduleLoader = services.GetRequireServiceInstance<IModuleLoader>();
        var moduleContainer = moduleLoader.LoadCikeModules(typeof(TStartupModule));
        services.AddSingleton(moduleContainer);

        foreach (var item in moduleContainer.CikeModules.Reverse<CikeModule>())
        {
            //Add Service Register
            foreach (var typeItem in item.GetType().Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsClass && typeof(IDependencyInjection).IsAssignableFrom(t)))
            {
                var interfaceList = typeItem.GetInterfaces();
                ServiceLifetime serviceLifetime = ServiceLifetime.Transient;
                if (interfaceList.Any(x => x == typeof(ITransientDependency)))
                {
                    serviceLifetime = ServiceLifetime.Transient;
                }
                else if (interfaceList.Any(x => x == typeof(IScopedDependency)))
                {
                    serviceLifetime = ServiceLifetime.Scoped;
                }
                else if (interfaceList.Any(x => x == typeof(ISingletonDependency)))
                {
                    serviceLifetime = ServiceLifetime.Singleton;
                }

                foreach (var interfaceType in typeItem.GetInterfaces())
                {
                    switch (serviceLifetime)
                    {
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(typeItem, typeItem);
                            services.AddSingleton(interfaceType, typeItem);
                            break;
                        case ServiceLifetime.Scoped:
                            services.AddScoped(typeItem, typeItem);
                            services.AddScoped(interfaceType, typeItem);
                            break;
                        case ServiceLifetime.Transient:
                            services.AddTransient(typeItem, typeItem);
                            services.AddTransient(interfaceType, typeItem);
                            break;
                        default:
                            break;
                    }
                }
            }

            item.ConfigureServices(new ServiceConfigurationContext(services, builder.Configuration));
        }
    }
}

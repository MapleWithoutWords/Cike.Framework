namespace Cike.Core.Modularity;

public class ModularityFactory
{

    public static async Task AddApplicationAsync<TStartupModule>(IServiceCollection services)
    {
        services.AddSingleton<IModuleLoader>(new ModuleLoader());

        var moduleLoader = services.GetSingletonInstance<IModuleLoader>();
        var moduleContainer = moduleLoader.LoadCikeModules(typeof(TStartupModule));
        services.AddSingleton(moduleContainer);

        foreach (var item in moduleContainer.CikeModules)
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

                //添加自身
                services.TryAdd(ServiceDescriptor.Describe(typeItem, typeItem, serviceLifetime));

                //添加接口
                var dependAttr = typeItem.GetCustomAttribute<DependencyInjection.DependencyAttribute>();
                foreach (var interfaceType in typeItem.GetInterfaces().Concat(typeItem.GetBaseClasses()))
                {
                    var descriptor = string.IsNullOrEmpty(dependAttr?.Key) ? ServiceDescriptor.Describe(interfaceType, sp => sp.GetRequiredService(typeItem), serviceLifetime) : ServiceDescriptor.DescribeKeyed(interfaceType, dependAttr.Key, (sp, key) => sp.GetRequiredService(typeItem), serviceLifetime);


                    if (dependAttr?.ReplaceServices == true)
                        services.Replace(descriptor);
                    else
                        if (dependAttr?.TryRegister == true)
                            services.TryAdd(descriptor);
                        else
                            services.Add(descriptor);
                }
            }

            await item.ConfigureServicesAsync(new ServiceConfigurationContext(services));
        }
        ModuleLoader.Services = services;
    }
}

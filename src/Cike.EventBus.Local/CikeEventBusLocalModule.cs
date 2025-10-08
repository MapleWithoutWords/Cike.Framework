namespace Cike.EventBus.Local;

[DependsOn(typeof(CikeEventBusModule))]
public class CikeEventBusLocalModule : CikeModule
{

    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        if (!context.Services.Any(e => e.ServiceType == typeof(CikeEventBusLocalModuleDenpency)))
        {
            var cikeModuleContainer = context.Services.GetSingletonInstance<CikeModuleContainer>();
            var eventHandlerRelationContainer = new LocalEventHandlerRelationContainer(cikeModuleContainer);
            context.Services.AddSingleton(eventHandlerRelationContainer);
            foreach (var item in eventHandlerRelationContainer.EventHanlderClassTypeList)
            {
                context.Services.AddScoped(item);
            }
            context.Services.AddSingleton(typeof(CikeEventBusLocalModuleDenpency));

            context.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ILocalEventMiddleware<>), typeof(DbTransactionLocalEventMiddleware<>), ServiceLifetime.Transient));
            context.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ILocalEventMiddleware<>), typeof(ExceptionLocalEventMiddleware<>), ServiceLifetime.Transient));
            context.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ILocalEventMiddleware<>), typeof(PreventRecursiveMiddleware<>), ServiceLifetime.Transient));
        }

        await base.ConfigureServicesAsync(context);
    }
}

internal class CikeEventBusLocalModuleDenpency
{
}

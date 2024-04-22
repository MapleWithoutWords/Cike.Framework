using Cike.Core.Modularity;
using Cike.EventBus.LocalEvent;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Cike.EventBus.Local;

public class CikeEventBusLocalModule : CikeModule
{

    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);
    }
}

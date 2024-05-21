using Cike.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Localization;

[DependsOn([])]
public class CikeLocalizationModule:CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ResourceManagerStringLocalizerFactory>();
        await base.ConfigureServicesAsync(context);
    }
}

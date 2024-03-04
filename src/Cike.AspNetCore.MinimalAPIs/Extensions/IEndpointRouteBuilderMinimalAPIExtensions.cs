using Cike.AspNetCore.MinimalAPIs.Options;
using Cike.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cike.AspNetCore.MinimalAPIs.Extensions;

public static class IEndpointRouteBuilderMinimalAPIExtensions
{

    public static IEndpointRouteBuilder AddCikeMinimalAPIs(this IEndpointRouteBuilder builder)
    {
        var globalRouteOptions = builder.ServiceProvider.GetRequiredService<IOptions<GlobalMinimalApiRouteOptions>>().Value;

        var cikeModuleContainer = builder.ServiceProvider.GetRequiredService<CikeModuleContainer>();

        var minimalApiServiceTypeList = cikeModuleContainer.ModuleTypes.SelectMany(e => e.Assembly.GetTypes().Where(x => !x.IsAbstract && x.IsClass && typeof(MinimalApiServiceBase).IsAssignableFrom(x)).Select(x => x)).ToList();
        foreach (var item in minimalApiServiceTypeList)
        {
            var instance = (MinimalApiServiceBase)builder.ServiceProvider.GetRequiredService(item);
            foreach (var methodInfo in item.GetMethods(System.Reflection.BindingFlags.Public))
            {
                //builder.Map()
            }
        }

        return builder;
    }
}

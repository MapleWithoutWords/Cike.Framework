using Cike.Core.DependencyInjection;
using Cike.Core.Modularity;
using Microsoft.Extensions.Hosting;

namespace Cike.Core.Extensions;

public static class IServiceCollectionModularityExtensions
{
    public static async Task AddApplicationAsync<TStartupModule>(
        this IServiceCollection services)
        where TStartupModule : CikeModule
    {
        services.AddSingleton<IApplicationWithExternalServiceProvider>(new ApplicationWithExternalServiceProvider());
        await ModularityFactory.AddApplicationAsync<TStartupModule>(services);
    }
}

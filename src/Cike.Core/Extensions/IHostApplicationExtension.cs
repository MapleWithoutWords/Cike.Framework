using Cike.Core.Modularity;
using Microsoft.Extensions.Hosting;

namespace Cike.Core.Extensions;

public static class IHostApplicationExtension
{
    public static void AddApplication<TStartupModule>(
               this IHost app)
        where TStartupModule : CikeModule
    {
        var cikeModuleContainer = app.Services.GetRequiredService<CikeModuleContainer>();
        cikeModuleContainer.CikeModules.Reverse<CikeModule>().ToList().ForEach(module =>
        {
            module.Initialize(new ApplicationInitializationContext(app.Services));
        });
    }
}

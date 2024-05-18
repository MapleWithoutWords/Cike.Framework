using Cike.Core.Modularity;
using Cike.Data.EFCore.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Data.EFCore.MySql;

[DependsOn([typeof(CikeDataEFCoreModule)])]
public class CikeDataEFCoreMySqlModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);
    }
}

using Cike.Core.Modularity;
using Cike.EFCore;

namespace Cike.Data.EFCore.MySql;

[DependsOn([typeof(CikeEFCoreModule)])]
public class CikeDataEFCoreMySqlModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);
    }
}

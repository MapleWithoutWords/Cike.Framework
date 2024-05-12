using Cike.Core.Modularity;

namespace Cike.Data.EFCore.MySql;

[DependsOn([typeof(CikeDataEFCoreModule)])]
public class CikeDataEFCoreMySqlModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);
    }
}

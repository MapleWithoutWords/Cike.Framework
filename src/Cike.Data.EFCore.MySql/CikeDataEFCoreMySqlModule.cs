using Cike.Core.Modularity;
using Cike.UniversalId.Guids;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Data.EFCore.MySql;

[DependsOn([typeof(CikeDataEFCoreModule)])]
public class CikeDataEFCoreMySqlModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeSequentialGuidGeneratorOptions>(options =>
         {
             if (options.DefaultSequentialGuidType == null)
             {
                 options.DefaultSequentialGuidType = SequentialGuidType.SequentialAsString;
             }
         });
        await base.ConfigureServicesAsync(context);
    }
}

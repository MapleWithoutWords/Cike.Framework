using Cike.Core.Modularity;
using Cike.UniversalId.Guids;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Data.EFCore.SqlServer;

[DependsOn([typeof(CikeDataEFCoreModule)])]
public class CikeDataEFCoreSqlServerModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeSequentialGuidGeneratorOptions>(options =>
        {
            if (options.DefaultSequentialGuidType == null)
            {
                options.DefaultSequentialGuidType = SequentialGuidType.SequentialAtEnd;
            }
        });
        context.Services.Configure<CikeDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
        return base.ConfigureServicesAsync(context);
    }
}

using Cike.Core.Modularity;
using Cike.Data.EFCore;
using Cike.Data.EFCore.Extensions;
using Cike.Data.EFCore.MySql;
using Cike.Data.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Data;

[DependsOn([typeof(CikeDataEFCoreMySqlModule)])]
public class CQRSDataModule:CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeDbContextOptions>(options =>
        {
            options.UseMySQL();
        });
        context.Services.AddCikeDbContext<CQRSDbContext>();
        return base.ConfigureServicesAsync(context);
    }
}

using Cike.Core.Modularity;
using Cike.Data.EFCore;
using Cike.Data.Extensions;
using CQRS.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.EntityFrameworkCore;

[DependsOn(typeof(CQRSDomainModule), typeof(CikeDataEFCoreModule))]
public class CQRSEntityFrameworkCoreModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeDbContextOptions>(options =>
        {
            options.Configure(ctx => ctx.DbContextOptionsBuilder.UseSqlite(ctx.ConnectionString));
        });
        context.Services.AddCikeDbContext<CqrsDbContext>();

        return base.ConfigureServicesAsync(context);
    }
}

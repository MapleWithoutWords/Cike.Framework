using Cike.AspNetCore.MinimalAPIs;
using Cike.AspNetCore.MinimalAPIs.Options;
using Cike.AspNetCore.Swagger;
using Cike.Core.Modularity;
using CQRS.Application;
using CQRS.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Service.Open;

[DependsOn(typeof(CQRSApplicationModule),
    typeof(CQRSEntityFrameworkCoreModule),
    typeof(CikeAspNetCoreMinimalApiModule),
    typeof(CikeAspNetCoreSwaggerModule))]
public class CQRSServiceOpenModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddCikeSwagger("CQRS.Sample");

        context.Services.Configure<GlobalMinimalApiRouteOptions>(options =>
        {
            options.EnabledAuthorization = false;
        });

        return base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
#if DEBUG
        context.GetApplicationBuilder().UseCikeSwaggerUI("CQRS.Sample");
#endif

        using (var scope = context.ServiceProvider.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<CqrsDbContext>().Database.EnsureCreatedAsync();
        }

        await base.InitializeAsync(context);
    }
}

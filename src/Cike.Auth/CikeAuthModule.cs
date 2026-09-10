using Cike.Auth.MultiTenant;
using Cike.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Auth;

public class CikeAuthModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ICurrentTenantAccessor>(new CurrentTenantAccessor());
        return base.ConfigureServicesAsync(context);
    }

    public override Task InitializeAsync(ApplicationInitializationContext context)
    {
        return base.InitializeAsync(context);
    }
}

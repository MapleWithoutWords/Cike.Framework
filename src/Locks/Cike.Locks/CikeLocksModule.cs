using Cike.Core.Modularity;
using Cike.Locks.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Locks.Abstracts;

public class CikeLocksModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<LockOptions>(options =>
        {
            options.DefaultTimeout = TimeSpan.FromMinutes(10);
        });
        await base.ConfigureServicesAsync(context);
    }
}

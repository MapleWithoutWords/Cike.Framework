using Cike.Core.Modularity;
using Cike.Data.DataFilters;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Data;

public class CikeDataModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeDbConnectionOptions>(context.Services.GetConfiguration());
        context.Services.AddSingleton(typeof(IDataFilter<>), typeof(DataFilter<>));
        return Task.CompletedTask;
    }
}

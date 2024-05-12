using Cike.Data.Attributes;
using Cike.Data.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;

namespace Cike.Data.EFCore.Internal;

internal static class DbContextOptionsFactory
{

    public static DbContextOptions<TDbContext> Create<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : CikeDbContext<TDbContext>
    {
        var connectionConfigName = ConnectionStringNameAttribute.GetConnStringName<TDbContext>();
        var connectionStringResolver = serviceProvider.GetRequiredService<IConnectionStringResolver>();
        var connectionString = AsyncContext.Run(() => connectionStringResolver.ResolveAsync(connectionConfigName));

        var dbContextConfigurationContext = new CikeDbContextConfigurationContext<TDbContext>(serviceProvider, connectionString, connectionConfigName);

        var cikeDbContextOption = serviceProvider.GetRequiredService<IOptions<CikeDbContextOptions>>().Value;

        cikeDbContextOption.DefaultConfigureAction?.Invoke(dbContextConfigurationContext);

        return dbContextConfigurationContext.DbContextOptionsBuilder.Options;
    }
}

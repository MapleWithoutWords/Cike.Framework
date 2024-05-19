using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cike.Data.EFCore;

public class CikeDbContextConfigurationContext
{
    public string ConnectionString { get; }

    public string? ConnectionStringName { get; }
    public DbContextOptionsBuilder DbContextOptionsBuilder { get; protected set; }
    public CikeDbContextConfigurationContext(IServiceProvider serviceProvider, string connectionString, string? connectionStringName)
    {
        ConnectionString = connectionString;
        ConnectionStringName = connectionStringName;

        DbContextOptionsBuilder = new DbContextOptionsBuilder()
            .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
            .UseApplicationServiceProvider(serviceProvider);
    }
}
public class CikeDbContextConfigurationContext<TDbContext> : CikeDbContextConfigurationContext where TDbContext : CikeDbContext<TDbContext>
{
    public new DbContextOptionsBuilder<TDbContext> DbContextOptionsBuilder => (DbContextOptionsBuilder<TDbContext>)base.DbContextOptionsBuilder;
    public CikeDbContextConfigurationContext(IServiceProvider serviceProvider, string connectionString, string? connectionStringName) : base(serviceProvider, connectionString, connectionStringName)
    {
        base.DbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>()
            .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
            .UseApplicationServiceProvider(serviceProvider);
    }
}

namespace Cike.Data.Internal;

internal static class DbContextFactory
{

    public static DbContextOptions<TDbContext> Create<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : cikedbcbcon<TDbContext>
}

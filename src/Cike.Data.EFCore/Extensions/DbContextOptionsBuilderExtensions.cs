namespace Cike.Data.EFCore.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseUow(this DbContextOptionsBuilder dbContextOptionsBuilder)
    {
        ((IDbContextOptionsBuilderInfrastructure)dbContextOptionsBuilder).AddOrUpdateExtension(new UowDbContextOptionsExtension());
        return dbContextOptionsBuilder;
    }
}

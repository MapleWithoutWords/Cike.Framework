namespace Cike.Data.EFCore.Extensions;

public static class DbContextOptionsExtensions
{
    public static DbContextOptions UseUow(this DbContextOptions dbContextOptions)
    {
        return dbContextOptions.WithExtension(new UowDbContextOptionsExtension());
    }
}

namespace Cike.Data.EFCore.Extensions;

public static class CikeDbContextOptionsExtensions
{
    public static CikeDbContextOptions UseMySQL(this CikeDbContextOptions options, Action<MySqlDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        options.Configure(context =>
        {
            context.DbContextOptionsBuilder.UseMySql(context.ConnectionString, ServerVersion.AutoDetect(context.ConnectionString), optionsBuilder =>
            {
                optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                mySQLOptionsAction?.Invoke(optionsBuilder);
            });
        });
        return options;
    }
}

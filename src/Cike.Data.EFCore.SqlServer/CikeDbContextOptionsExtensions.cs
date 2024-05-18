using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cike.Data.EFCore.SqlServer;

public static class CikeDbContextOptionsExtensions
{
    public static CikeDbContextOptions UseSqlServer(this CikeDbContextOptions options, Action<SqlServerDbContextOptionsBuilder>? mySQLOptionsAction = null)
    {
        options.Configure(context =>
        {
            context.DbContextOptionsBuilder.UseSqlServer(context.ConnectionString, optionsBuilder =>
            {
                optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                mySQLOptionsAction?.Invoke(optionsBuilder);
            });
        });
        return options;
    }
}

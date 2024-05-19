using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Cike.Data.EFCore.Internal;

namespace Cike.Data.EFCore.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static void UseUow(this DbContextOptionsBuilder dbContextOptionsBuilder)
    {
        ((IDbContextOptionsBuilderInfrastructure)dbContextOptionsBuilder).AddOrUpdateExtension(new UowDbContextOptionsExtension());
    }
}

using Cike.Data.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Cike.Data.EFCore.Internal;

internal static class DbContextFactory
{

    public static DbContextOptions<TDbContext> Create<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : CikeDbContext<TDbContext>
    {

    }
}

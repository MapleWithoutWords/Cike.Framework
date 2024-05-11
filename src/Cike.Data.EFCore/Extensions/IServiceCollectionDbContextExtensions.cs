using Microsoft.Extensions.DependencyInjection;

namespace Cike.Data.Extensions;

public static class IServiceCollectionDbContextExtensions
{
    public async Task<IServiceCollection> AddCikeDbContext<TDbContext>(this IServiceCollection services)
    {


        return services;
    }
}

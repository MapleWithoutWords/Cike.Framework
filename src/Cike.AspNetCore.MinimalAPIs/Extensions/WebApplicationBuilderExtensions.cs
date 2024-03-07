using Cike.Core.Extensions;
using Cike.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.AspNetCore.MinimalAPIs.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static async Task AddApplicationAsync<TStartupModule>(WebApplicationBuilder builder)
        where TStartupModule : CikeModule
    {
        builder.Services.ReplaceConfiguration(builder.Configuration);
        await builder.Services.AddApplicationAsync<TStartupModule>();
    }
}

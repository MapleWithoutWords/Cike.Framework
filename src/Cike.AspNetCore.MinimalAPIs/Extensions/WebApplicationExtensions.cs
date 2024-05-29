using Cike.Auth.Middlewares;
using Cike.Core.Modularity;
using Cike.Core.ObjectAccessor;
using Cike.Core.Thireading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cike.AspNetCore.MinimalAPIs.Extensions;

public static class WebApplicationExtensions
{
    public async static Task InitializeApplicationAsync(this WebApplication app)
    {
        app.Services.GetRequiredService<ObjectAccessor<IApplicationBuilder>>().Value = app;
        app.Services.GetRequiredService<ObjectAccessor<IEndpointRouteBuilder>>().Value = app;
        var application = app.Services.GetRequiredService<IApplicationWithExternalServiceProvider>();
        var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        applicationLifetime.ApplicationStopping.Register(() =>
        {
            AsyncHelper.RunSync(() => application.ShutdownAsync(app.Services));
        });

        await application.InitializeAsync(app.Services);
    }

    public static void UseMultiTenant(this WebApplication app)
    {
        app.UseMiddleware<TenantMiddleware>();
    }
}
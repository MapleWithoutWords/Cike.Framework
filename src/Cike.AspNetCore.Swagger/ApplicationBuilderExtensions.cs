using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Cike.AspNetCore.Swagger;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCikeSwaggerUI(this IApplicationBuilder app, string projectName, Action<SwaggerUIOptions> configureOptions = null)
    {
        app.UseSwagger().UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", projectName);
            configureOptions?.Invoke(c);
        });
        return app;
    }
}

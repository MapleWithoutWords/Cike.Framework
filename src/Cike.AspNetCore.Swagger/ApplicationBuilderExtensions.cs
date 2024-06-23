using Microsoft.AspNetCore.Builder;

namespace Cike.AspNetCore.Swagger;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCikeSwaggerUI(this IApplicationBuilder app, string projectName)
    {
        app.UseSwagger().UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", projectName);
        });
        return app;
    }
}

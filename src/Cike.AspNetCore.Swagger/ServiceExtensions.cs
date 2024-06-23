using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cike.AspNetCore.Swagger;

public static class ServiceExtensions
{
    public static IServiceCollection AddCikeSwagger(IServiceCollection services, string projectName, Action<SwaggerGenOptions> configOptions = null)
    {
        return services.AddEndpointsApiExplorer()
                .AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo()
                    {
                        Title = projectName,
                        Version = "v1"
                    });
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        BearerFormat = "JWT",
                        Scheme = "Bearer",
                        Description = "Specify the authorization token",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                    foreach (var item in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
                    {
                        options.IncludeXmlComments(item);
                    }

                    // 支持多态
                    options.UseAllOfForInheritance();
                    options.UseOneOfForPolymorphism();
                    // 支持枚举
                    options.SchemaFilter<EnumDescriptionSupplementSchemaFilter>();
                    configOptions?.Invoke(options);
                });
    }

}

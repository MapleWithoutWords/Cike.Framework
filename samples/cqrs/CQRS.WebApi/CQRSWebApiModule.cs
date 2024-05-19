using Cike.AspNetCore.MinimalAPIs;
using Cike.Core.Modularity;
using CQRS.Application;
using CQRS.WebApi.Services;
using Microsoft.OpenApi.Models;

namespace CQRS.WebApi;

[DependsOn([typeof(CQRSApplicationModule), typeof(CikeAspNetCoreMinimalApiModule)])]
public class CQRSWebApiModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.AddSingleton(typeof(TodoService));
        services.AddEndpointsApiExplorer()
                .AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo()
                    {
                        Title = $"CQRS",
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
                    // 支持多态
                    options.UseAllOfForInheritance();
                    options.UseOneOfForPolymorphism();
                    // 支持枚举
                    //options.SchemaFilter<EnumDescriptionSupplementSchemaFilter>();
                });
    }

    public override Task InitializeAsync(ApplicationInitializationContext context)
    {
        context.GetApplicationBuilder().UseSwagger();
        context.GetApplicationBuilder().UseSwaggerUI(options => options.SwaggerEndpoint(
                $"/swagger/{"v1"}/swagger.json",
                $"CQRS HTTP API"
            )
        );
        return base.InitializeAsync(context);
    }
}

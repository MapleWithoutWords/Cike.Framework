using Cike.Core.Modularity;
using Microsoft.OpenApi.Models;

namespace Cike.AspNetCore.MinimalAPIs.Tests;

[DependsOn(typeof(CikeAspNetCoreMinimalApiModule))]
public class TestModule : CikeModule
{

    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddEndpointsApiExplorer();
        context.Services.AddSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "InchFan API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
                foreach (var item in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./"), "*.xml"))
                {
                    options.IncludeXmlComments(item, true);
                }
                //options.DocumentFilter<EnumDocumentFilter>();
            });

        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "InchFan API");
        });

       await  base.InitializeAsync(context);
    }
}

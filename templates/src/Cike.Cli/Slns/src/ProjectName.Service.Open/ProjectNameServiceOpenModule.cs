namespace ProjectName.Service.Open;

[DependsOn([
    typeof(ProjectNameApplicationModule),
    typeof(ProjectNameEntityFrameworkCoreModule),
    typeof(CikeAspNetCoreMinimalApiModule),
    ])]
public class ProjectNameServiceOpenModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {

        context.Services.AddCikeSwagger("ProjectName", options =>
        {
            options.SupportNonNullableReferenceTypes();
            //options.DocumentFilter<PolymorphismDocumentFilter<MessagePlatformBaseJsonConfig, MessagePlatformType>>();
        });
        context.Services.AddValidatorsFromAssembly(typeof(ProjectNameApplicationModule).Assembly);
        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var routeBuilder = context.GetEndpointRouteBuilder();
#if DEBUG
        app.UseCikeSwaggerUI("ProjectName");
#endif

        //routeBuilder.MapHub<ChatHub>("/chathub");
        //var jsonOptions = context.ServiceProvider.GetRequiredService<IOptions<JsonOptions>>().Value;
        await base.InitializeAsync(context);
    }
}

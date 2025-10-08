namespace Cike.FluentValidation;

public class CikeFluentValidationModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);

        var cikeModuleContainer = context.Services.GetSingletonInstance<CikeModuleContainer>();
        foreach (var item in cikeModuleContainer.CikeModules)
        {
            context.Services.AddValidatorsFromAssembly(item.GetType().Assembly);
        }
    }
}

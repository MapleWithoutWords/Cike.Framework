namespace Cike.Uow;

public class CikeUowModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<UnitOfWorkOptions>(options =>
        {
            options.Enable = true;
        });
        return base.ConfigureServicesAsync(context);
    }
}

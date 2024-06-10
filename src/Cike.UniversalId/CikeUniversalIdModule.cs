namespace Cike.UniversalId;

public class CikeUniversalIdModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton(typeof(ISnowflakeIdGenerator), new SnowflakeIdGenerator(1));
        return base.ConfigureServicesAsync(context);
    }
}

namespace Cike.Data.EFCore;

[DependsOn([typeof(CikeAuthModule),
    typeof(CikeDomainModule),
    typeof(CikeUowModule),
    typeof(CikeUniversalIdModule),
])]
public class CikeDataEFCoreModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeSnowflakeOptions>(options =>
        {
            options.IsEnable = true;
        });
        return base.ConfigureServicesAsync(context);
    }
}

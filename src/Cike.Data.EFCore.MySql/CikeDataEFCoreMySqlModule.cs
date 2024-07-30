namespace Cike.Data.EFCore.MySql;

[DependsOn([typeof(CikeDataEFCoreModule)])]
public class CikeDataEFCoreMySqlModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeSequentialGuidGeneratorOptions>(options =>
         {
             if (options.DefaultSequentialGuidType == null)
             {
                 options.DefaultSequentialGuidType = SequentialGuidType.SequentialAsString;
             }
         });
        context.Services.Configure<CikeDbContextOptions>(options =>
        {
            options.UseMySQL();
        });
        await base.ConfigureServicesAsync(context);
    }
}

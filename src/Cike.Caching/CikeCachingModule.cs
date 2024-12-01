namespace Cike.Caching;

public class CikeCachingModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<TypeAliasOptions>(options =>
        {
            options.RefreshTypeAliasInterval = 30;
            options.GetAllTypeAliasFunc = () => new Dictionary<string, string>();
        });

        context.Services.Configure<MultilevelCacheGlobalOptions>(context.Services.GetConfiguration().GetSection("MultilevelCache"));

        context.Services.Configure<RedisConfigurationOptions>(context.Services.GetConfiguration().GetSection(RedisConstant.DEFAULT_REDIS_SECTION_NAME));

        context.Services.AddMemoryCache();

        await base.ConfigureServicesAsync(context);
    }
}

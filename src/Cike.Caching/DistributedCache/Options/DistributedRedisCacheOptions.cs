namespace Cike.Caching.DistributedCache.Options;

public class DistributedRedisCacheOptions
{
    public RedisConfigurationOptions? Options { get; set; }

    public CacheEntryOptions? CacheEntryOptions { get; set; }
}

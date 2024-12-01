namespace Cike.Caching.Options;

public class CombinedCacheEntryOptions
{
    public CacheEntryOptions? MemoryCacheEntryOptions { get; set; }

    public CacheEntryOptions? DistributedCacheEntryOptions { get; set; }
}

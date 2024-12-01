namespace Cike.Caching.Options;

public class MultilevelCacheOptions : CacheOptions
{
    /// <summary>
    /// Memory default valid time configuration
    /// When the memory cache does not exist, the result obtained from the distributed cache is used when the result is newly written to the memory cache.
    /// If not specified, the global configuration is used by default
    /// </summary>
    public CacheEntryOptions? MemoryCacheEntryOptions { get; set; }
}

namespace Cike.Caching.MultilevelCache.Options;

/// <summary>
/// The MASA Multilevel cache options.
/// </summary>
public class MultilevelCacheGlobalOptions : MemoryCacheOptions
{
    public CacheOptions GlobalCacheOptions { get; set; } = new()
    {
        CacheKeyType = CacheKeyType.TypeName
    };

    /// <summary>
    /// Gets or sets the <see cref="SubscribeKeyType"/>.
    /// </summary>
    public SubscribeKeyType SubscribeKeyType { get; set; } = SubscribeKeyType.ValueTypeFullNameAndKey;

    /// <summary>
    /// Gets or sets the prefix of subscribe key.
    /// </summary>
    public string SubscribeKeyPrefix { get; set; } = string.Empty;

    public string? InstanceId { get; set; }

    /// <summary>
    /// Memory default valid time configuration
    /// </summary>
    public CacheEntryOptions? CacheEntryOptions { get; set; }
}

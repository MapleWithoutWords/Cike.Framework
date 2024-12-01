namespace Cike.Caching.MultilevelCache.Internal.Options;

internal class SetOptions<T>
{
    public string? FormattedKey { get; set; }

    public T? Value { get; set; }

    public CacheEntryOptions? MemoryCacheEntryOptions { get; set; }
}

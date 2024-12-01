namespace Cike.Caching.Formats;

public interface IFormatCacheKeyProvider
{
    string FormatCacheKey<T>(string? instanceId, string key, CacheKeyType cacheKeyType, Func<string, string>? typeAliasFunc = null);
}

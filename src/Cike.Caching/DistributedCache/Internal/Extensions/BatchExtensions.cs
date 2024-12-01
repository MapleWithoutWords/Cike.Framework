namespace Cike.Caching.DistributedCache.Internal.Extensions;

internal static class BatchExtensions
{
    public static Task<RedisValue[]> HashGetAsync(
        this IBatch batch,
        string key,
        params string[] hashFields)
        => batch.HashGetAsync(key, hashFields.ToRedisValueArray());
}

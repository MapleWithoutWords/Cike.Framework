namespace StackExchange.Redis;

internal static class DatabaseExtensions
{
    public static RedisValue[] HashGet(
        this IDatabase database,
        string key,
        params string[] hashFields)
        => database.HashGet(key, hashFields.ToRedisValueArray());

    public static Task<RedisValue[]> HashGetAsync(
        this IDatabase database,
        string key,
        params string[] hashFields)
        => database.HashGetAsync(key, hashFields.ToRedisValueArray());
}

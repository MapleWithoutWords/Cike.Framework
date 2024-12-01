namespace Cike.Caching.DistributedCache.Internal.Extensions;

internal static class RedisConfigurationOptionsExtensions
{
    /// <summary>
    /// Get the available redis configuration
    /// </summary>
    /// <param name="redisConfigurationOptions"></param>
    /// <returns></returns>
    public static RedisConfigurationOptions GetAvailableRedisOptions(this RedisConfigurationOptions redisConfigurationOptions)
    {
        if (redisConfigurationOptions.Servers.Any())
            return redisConfigurationOptions;

        return new RedisConfigurationOptions()
        {
            Servers = new List<RedisServerOptions>()
            {
                new()
            }
        };
    }
}

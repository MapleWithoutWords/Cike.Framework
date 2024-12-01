namespace Cike.Caching.DistributedCache.Internal.Extensions;

internal static class DateTimeOffsetExtensions
{
    public static long? GetExpirationInSeconds(
        DateTimeOffset creationTime,
        DateTimeOffset? absoluteExpiration,
        TimeSpan? slidingExpiration)
    {
        if (absoluteExpiration.HasValue && slidingExpiration.HasValue)
            return (long)Math.Min(
                (absoluteExpiration.Value - creationTime).TotalSeconds,
                slidingExpiration.Value.TotalSeconds);

        if (absoluteExpiration.HasValue)
            return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;

        if (slidingExpiration.HasValue)
            return (long)slidingExpiration.Value.TotalSeconds;

        return null;
    }
}

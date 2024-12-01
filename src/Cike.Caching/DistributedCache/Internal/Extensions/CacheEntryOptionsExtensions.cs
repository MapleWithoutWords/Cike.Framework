namespace Cike.Caching.DistributedCache.Internal.Extensions;

internal static class CacheEntryOptionsExtensions
{
    public static DateTimeOffset? GetAbsoluteExpiration(this CacheEntryOptions options, DateTimeOffset creationTime)
    {
        if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.AbsoluteExpiration.Value,
                "The absolute expiration value must be in the future.");

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            return creationTime.Add(options.AbsoluteExpirationRelativeToNow.Value);

        return options.AbsoluteExpiration;
    }
}

namespace Cike.Caching.DistributedCache.Internal.Model;

internal class CacheExpiredModel
{
    public long AbsoluteExpirationTicks { get; set; }

    public long SlidingExpirationTicks { get; set; }

    public long Expired { get; set; }

    public CacheExpiredModel(long absoluteExpirationTicks, long slidingExpirationTicks, long expired)
    {
        AbsoluteExpirationTicks = absoluteExpirationTicks;
        SlidingExpirationTicks = slidingExpirationTicks;
        Expired = expired;
    }
}

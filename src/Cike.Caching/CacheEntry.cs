namespace Cike.Caching;

public class CacheEntry<T> : CacheEntryOptionsBase
{
    public T? Value { get; }

    public CacheEntry(T? value)
    {
        Value = value;
    }

    public CacheEntry(T? value, DateTimeOffset absoluteExpiration) : this(value)
        => AbsoluteExpiration = absoluteExpiration;

    public CacheEntry(T? value, TimeSpan absoluteExpirationRelativeToNow) : this(value)
        => AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
}

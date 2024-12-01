namespace Cike.Caching.MultilevelCache.Internal.Options;

internal class SubscribeValueChangedOptions<T>
{
    public Action<T?>? ValueChanged { get; set; }
}

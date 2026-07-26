namespace Cike.Core;

public class LazyManualMemoryCache<TKey, TValue> : IDisposable
{
    private ConcurrentDictionary<TKey, Lazy<TValue>> _dicCache = new ConcurrentDictionary<TKey, Lazy<TValue>>();

    public TKey[] Keys => _dicCache.Keys.ToArray();

    public TValue[] Values => _dicCache.Values.Select((Lazy<TValue> value) => value.Value).ToArray();

    public bool Get(TKey key, out TValue value)
    {
        Lazy<TValue> value2;
        bool num = _dicCache.TryGetValue(key, out value2);
        if (num)
        {
            value = value2.Value;
            return num;
        }

        value = default(TValue);
        return num;
    }

    public bool TryAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        return _dicCache.TryAdd(key, new Lazy<TValue>(() => valueFactory(key), LazyThreadSafetyMode.ExecutionAndPublication));
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (!_dicCache.TryGetValue(key, out var value))
        {
            value = _dicCache.GetOrAdd(key, (TKey k) => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
        }

        return value.Value;
    }

    public bool TryUpdate(TKey key, Func<TKey, TValue> valueFactory, TValue comparisonValue)
    {
        return _dicCache.TryUpdate(key, new Lazy<TValue>(() => valueFactory(key), LazyThreadSafetyMode.ExecutionAndPublication), new Lazy<TValue>(() => comparisonValue, LazyThreadSafetyMode.ExecutionAndPublication));
    }

    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> valueFactory)
    {
        return _dicCache.AddOrUpdate(key, (TKey k) => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication), (TKey oldkey, Lazy<TValue> oldvalue) => new Lazy<TValue>(() => valueFactory(oldkey), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    public bool Remove(TKey key)
    {
        Lazy<TValue> value;
        return _dicCache.TryRemove(key, out value);
    }

    public bool ContainsKey(TKey key)
    {
        return _dicCache.ContainsKey(key);
    }

    public void Clear()
    {
        _dicCache.Clear();
    }

    public void Dispose()
    {
        _dicCache.Clear();
        _dicCache = null;
    }
}

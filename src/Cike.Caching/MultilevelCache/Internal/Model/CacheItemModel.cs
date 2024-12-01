namespace Cike.Caching.MultilevelCache.Internal.Model;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal class CacheItemModel<T>
{
    public string Key { get; set; }

    public string MemoryCacheKey { get; set; }

    public bool IsExist { get; set; }

    public T? Value { get; set; }

    public CacheItemModel(string key, string memoryCacheKey, bool isExist, T? value)
    {
        Key = key;
        MemoryCacheKey = memoryCacheKey;
        IsExist = isExist;
        Value = value;
    }
}

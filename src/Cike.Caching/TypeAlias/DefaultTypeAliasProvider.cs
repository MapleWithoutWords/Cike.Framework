namespace Cike.Caching.TypeAlias;

public class DefaultTypeAliasProvider : ITypeAliasProvider, ISingletonDependency
{
    private readonly object _lock = new();
    private ConcurrentDictionary<string, Lazy<string>>? _dicCache;
    private readonly IOptionsMonitor<TypeAliasOptions> _options;
    private DateTime? _lastDateTime;

    public DefaultTypeAliasProvider(IOptionsMonitor<TypeAliasOptions> options)
    {
        _options = options;
    }

    public string GetAliasName(string typeName)
    {
        if (_options.CurrentValue == null || _options.CurrentValue.GetAllTypeAliasFunc == null)
            throw new NotImplementedException();

        if (_dicCache == null || _dicCache.IsEmpty)
        {
            RefreshTypeAlias();
        }
        var aliasName = _dicCache?.GetOrAdd(typeName, key => new Lazy<string>(() =>
        {
            RefreshTypeAlias();

            if (_dicCache.TryGetValue(key, out var alias))
                return alias.Value;

            throw new ArgumentNullException(key, $"not found type alias by {typeName}");

        }, LazyThreadSafetyMode.ExecutionAndPublication));
        return aliasName?.Value ?? throw new ArgumentNullException(typeName, $"not found type alias by {typeName}");
    }

    private void RefreshTypeAlias()
    {
        if (_lastDateTime != null && (DateTime.UtcNow - _lastDateTime.Value).TotalSeconds < _options.CurrentValue!.RefreshTypeAliasInterval)
        {
            return;
        }

        lock (_lock)
        {
            _lastDateTime = DateTime.UtcNow;
            _dicCache?.Clear();
            _dicCache ??= new ConcurrentDictionary<string, Lazy<string>>();

            var typeAliases = _options.CurrentValue!.GetAllTypeAliasFunc!.Invoke();
            foreach (var typeAlias in typeAliases)
            {
                _dicCache[typeAlias.Key] = new Lazy<string>(typeAlias.Value);
            }
        }
    }
}

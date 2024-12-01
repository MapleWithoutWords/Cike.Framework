namespace Cike.Caching.TypeAlias.Options;

public class TypeAliasOptions
{
    /// <summary>
    /// Refresh TypeAlias minimum interval time
    /// default: 30s
    /// </summary>
    public long RefreshTypeAliasInterval { get; set; } = 30;

    public Func<Dictionary<string, string>>? GetAllTypeAliasFunc { get; set; }
}

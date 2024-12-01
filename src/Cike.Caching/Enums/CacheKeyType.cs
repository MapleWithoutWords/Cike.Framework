namespace Cike.Caching.Enums;

public enum CacheKeyType
{
    /// <summary>
    /// Keep it the same, use the key directly
    /// </summary>
    None = 1,

    /// <summary>
    /// Type's name(Type's full name with generic type name) and key combination
    /// </summary>
    TypeName,

    /// <summary>
    /// Type Alias and key combination, Format: ${TypeAliasName}{:}{key}
    /// </summary>
    TypeAlias
}

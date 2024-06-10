namespace Cike.Data.EFCore.Internal;

#pragma warning disable EF1001
internal class UowDbSetSource : IDbSetSource
{
    private static readonly MethodInfo GenericCreateSet
        = typeof(UowDbSetSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateSetFactory))!;

    private readonly ConcurrentDictionary<(Type Type, string? Name), Func<DbContext, string?, object>> _cache = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Create(DbContext context, Type type)
        => CreateCore(context, type, null, GenericCreateSet);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object Create(DbContext context, string name, Type type)
        => CreateCore(context, type, name, GenericCreateSet);

    private object CreateCore(DbContext context, Type type, string? name, MethodInfo createMethod)
        => _cache.GetOrAdd(
            (type, name),
            static (t, createMethod) => (Func<DbContext, string?, object>)createMethod
                .MakeGenericMethod(t.Type)
                .Invoke(null, null)!,
    createMethod)(context, name);

    private static Func<DbContext, string?, object> CreateSetFactory<TEntity>()
        where TEntity : class
        => (c, name) => new UowDbSet<TEntity>(c, name);
}
#pragma warning restore EF1001
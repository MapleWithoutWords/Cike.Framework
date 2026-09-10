using Cike.Core.Models;

namespace System;

public static class CikeTypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> SimpleAssemblyQualifiedTypeNameCache = new();

    /// <summary>
    /// Gets the assembly-qualified name of the type, without version, culture, and public key token information.
    /// </summary>
    public static string GetSimpleAssemblyQualifiedName(this Type type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));
        return SimpleAssemblyQualifiedTypeNameCache.GetOrAdd(type, BuildSimplifiedName);
    }

    /// <summary>
    /// Returns true of the type is generic, false otherwise.
    /// </summary>
    public static bool IsGenericType(this Type type, Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

    /// <summary>
    /// Returns true of the type is nullable, false otherwise.
    /// </summary>
    public static bool IsNullableType(this Type type) => type.IsGenericType(typeof(Nullable<>));

    /// <summary>
    /// Returns the wrapped type of the specified nullable type.
    /// </summary>
    public static Type GetTypeOfNullable(this Type type) => type.GenericTypeArguments[0];

    /// <summary>
    /// Returns true if the specified type is a collection type, false otherwise.
    /// </summary>
    public static bool IsCollectionType(this Type type)
    {
        if (!type.IsGenericType)
            return false;

        var elementType = type.GenericTypeArguments[0];
        var collectionType = typeof(ICollection<>).MakeGenericType(elementType);
        var listType = typeof(IList<>).MakeGenericType(elementType);
        return collectionType.IsAssignableFrom(type) || listType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Constructs a collection type from the specified type.
    /// </summary>
    public static Type MakeCollectionType(this Type type) => typeof(ICollection<>).MakeGenericType(type);

    /// <summary>
    /// Returns the element type of the specified collection type.
    /// </summary>
    public static Type GetCollectionElementType(this Type type) => type.GenericTypeArguments[0];

    /// <summary>
    /// Determines whether the specified type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the specified type is numeric, otherwise false.</returns>
    public static bool IsNumericType(this Type type)
    {
        return type.IsPrimitive || type == typeof(decimal) || type == typeof(float) || type == typeof(double) || type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte);
    }

    private static string BuildSimplifiedName(Type type)
    {
        var assemblyName = type.Assembly.GetName().Name;

        if (type.IsGenericType)
        {
            var genericTypeName = type.GetGenericTypeDefinition().FullName!;
            var backtickIndex = genericTypeName.IndexOf('`');
            var typeNameWithoutArity = genericTypeName[..backtickIndex];
            var arity = genericTypeName[backtickIndex..];
            var simplifiedGenericArguments = type.GetGenericArguments().Select(BuildSimplifiedName);

            return $"{typeNameWithoutArity}{arity}[[{string.Join("],[", simplifiedGenericArguments)}]], {assemblyName}";
        }

        return $"{type.FullName}, {assemblyName}";
    }

    public static string GetFullNameWithAssemblyName(this Type type)
    {
        return type.FullName + ", " + type.Assembly.GetName().Name;
    }

    /// <summary>
    /// Determines whether an instance of this type can be assigned to
    /// an instance of the <typeparamref name="TTarget"></typeparamref>.
    ///
    /// Internally uses <see cref="Type.IsAssignableFrom"/>.
    /// </summary>
    /// <typeparam name="TTarget">Target type</typeparam> (as reverse).
    public static bool IsAssignableTo<TTarget>([NotNull] this Type type)
    {
        return type.IsAssignableTo(typeof(TTarget));
    }

    /// <summary>
    /// Determines whether an instance of this type can be assigned to
    /// an instance of the <paramref name="targetType"></paramref>.
    ///
    /// Internally uses <see cref="Type.IsAssignableFrom"/> (as reverse).
    /// </summary>
    /// <param name="type">this type</param>
    /// <param name="targetType">Target type</param>
    public static bool IsAssignableTo([NotNull] this Type type, [NotNull] Type targetType)
    {
        return targetType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets all base classes of this type.
    /// </summary>
    /// <param name="type">The type to get its base classes.</param>
    /// <param name="includeObject">True, to include the standard <see cref="object"/> type in the returned array.</param>
    public static Type[] GetBaseClasses([NotNull] this Type type, bool includeObject = true)
    {
        var types = new List<Type>();
        AddTypeAndBaseTypesRecursively(types, type.BaseType, includeObject);
        return types.ToArray();
    }

    /// <summary>
    /// Gets all base classes of this type.
    /// </summary>
    /// <param name="type">The type to get its base classes.</param>
    /// <param name="stoppingType">A type to stop going to the deeper base classes. This type will be be included in the returned array</param>
    /// <param name="includeObject">True, to include the standard <see cref="object"/> type in the returned array.</param>
    public static Type[] GetBaseClasses([NotNull] this Type type, Type stoppingType, bool includeObject = true)
    {
        var types = new List<Type>();
        AddTypeAndBaseTypesRecursively(types, type.BaseType, includeObject, stoppingType);
        return types.ToArray();
    }

    private static void AddTypeAndBaseTypesRecursively(
        [NotNull] List<Type> types,
        Type? type,
        bool includeObject,
        Type? stoppingType = null)
    {
        if (type == null || type == stoppingType)
        {
            return;
        }

        if (!includeObject && type == typeof(object))
        {
            return;
        }

        AddTypeAndBaseTypesRecursively(types, type.BaseType, includeObject, stoppingType);
        types.Add(type);
    }

    /// <summary>
    /// Returns the default value for the specified type.
    /// </summary>
    public static object? GetDefaultValue(this Type type) => type.IsClass ? null : Activator.CreateInstance(type);

    /// <summary>
    /// Returns the element type of the specified type representing an array or generic enumerable.
    /// </summary>
    public static Type GetEnumerableElementType(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType()!;

        var elementType = FindIEnumerable(type);
        return elementType == null ? type : elementType.GetGenericArguments()[0];
    }

    /// <summary>
    /// Searches for the first implemented IEnumerable interface in the given type hierarchy, and returns the generic type argument of the interface. 
    /// </summary>
    /// <param name="sequenceType">The type to search for the IEnumerable interface.</param>
    /// <returns>The generic type argument of the first implemented IEnumerable interface found in the type hierarchy, or null if none is found.</returns>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    private static Type? FindIEnumerable([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type? sequenceType)
    {
        if (sequenceType == null || sequenceType == typeof(string))
            return null;

        if (sequenceType.IsArray)
            return typeof(IEnumerable<>).MakeGenericType(sequenceType.GetElementType()!);

        if (sequenceType.IsGenericType)
        {
            foreach (var arg in sequenceType.GetGenericArguments())
            {
                var enumerable = typeof(IEnumerable<>).MakeGenericType(arg);
                if (enumerable.IsAssignableFrom(sequenceType))
                    return enumerable;
            }
        }

        var interfaces = sequenceType.GetInterfaces();

        if (interfaces is { Length: > 0 })
        {
            foreach (var interfaceType in interfaces)
            {
                var enumerable = FindIEnumerable(interfaceType);
                if (enumerable != null) return enumerable;
            }
        }
        if (sequenceType.BaseType != null && sequenceType.BaseType != typeof(object))
            return FindIEnumerable(sequenceType.BaseType);

        return null;
    }

    public static string GetFriendlyTypeName(this Type type, Brackets brackets)
    {
        if (type.IsArray)
        {
            var elementTypeName = GetFriendlyTypeName(type.GetElementType()!, brackets);
            var rank = type.GetArrayRank();
            var commas = rank > 1 ? new string(',', rank - 1) : string.Empty;
            return elementTypeName + "[" + commas + "]";
        }

        if (!type.IsGenericType)
            return type.FullName!;

        var sb = new StringBuilder();
        sb.Append(type.Namespace);
        sb.Append('.');
        sb.Append(type.Name[..type.Name.IndexOf('`')]);
        sb.Append(brackets.Open);
        var genericArgs = type.GetGenericArguments();
        for (var i = 0; i < genericArgs.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(GetFriendlyTypeName(genericArgs[i], brackets));
        }

        sb.Append(brackets.Close);
        return sb.ToString();
    }
}

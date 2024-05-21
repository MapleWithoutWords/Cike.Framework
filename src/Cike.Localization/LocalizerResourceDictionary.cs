using Cike.Localization.Attributes;
using System.Data.Common;

namespace Cike.Localization;

public class LocalizerResourceDictionary : Dictionary<string, LocalizationResource>
{
    private readonly Dictionary<Type, LocalizationResource> _resourcesByTypes = new();
    public LocalizationResource Add(Type resourceType, string? defaultCultureName = null)
    {
        var resourceName = LocalizationResourceNameAttribute.GetName(resourceType);
        if (ContainsKey(resourceName))
        {
            throw new ArgumentException("This resource is already added before: " + resourceType.AssemblyQualifiedName);
        }

        var resource = new LocalizationResource(resourceType, defaultCultureName);

        this[resourceName] = resource;
        _resourcesByTypes[resourceType] = resource;

        return resource;
    }

    public LocalizationResource? GetOrNull(Type resourceType)
    {
        return _resourcesByTypes.GetOrDefault(resourceType);
    }

    public LocalizationResource GetOrAdd(Type resourceType, string? defaultCultureName = null)
    {
        return GetOrNull(resourceType) ?? Add(resourceType, defaultCultureName);
    }
}

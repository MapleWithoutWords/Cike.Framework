using Cike.Localization.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace Cike.Localization;

public class LocalizationResource
{
    public Type ResourceType { get; set; }
    public string ResourceName { get; }

    public List<string> BaseResourceNames { get; } = new();

    public string? DefaultCultureName { get; set; }

    public List<ILocalizationResourceContributor> Contributors { get; } = new();


    public LocalizationResource(
        Type resourceType,
        string? defaultCultureName = null,
        ILocalizationResourceContributor? initialContributor = null)
    {
        ResourceType = resourceType;
        ResourceName = LocalizationResourceNameAttribute.GetName(resourceType);
        DefaultCultureName = defaultCultureName;

        Contributors = new  List<ILocalizationResourceContributor>();
        BaseResourceNames = new();

        if (initialContributor != null)
        {
            Contributors.Add(initialContributor);
        }
    }
}

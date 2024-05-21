using System.Reflection;

namespace Cike.Localization.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class LocalizationResourceNameAttribute : Attribute
{
    public string Name { get; set; }

    public LocalizationResourceNameAttribute(string name)
    {
        Name = name;
    }

    public static string GetName(Type type)
    {
        return type.GetCustomAttribute<LocalizationResourceNameAttribute>()?.Name ?? type.FullName!;
    }
}

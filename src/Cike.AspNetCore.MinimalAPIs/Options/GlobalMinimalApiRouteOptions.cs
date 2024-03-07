using System.Reflection;

namespace Cike.AspNetCore.MinimalAPIs.Options;

public class GlobalMinimalApiRouteOptions : MinimalApiRouteOptions
{
    public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }
    protected EnglishPluralizationService PluralizationService { get; set; } = new EnglishPluralizationService();

    public GlobalMinimalApiRouteOptions()
    {

    }

    public string GetPluralizationName(string name)
    {
        return PluralizationService.Pluralize(name);
    }
}

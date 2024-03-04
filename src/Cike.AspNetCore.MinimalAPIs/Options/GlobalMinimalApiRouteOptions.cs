using System.Reflection;

namespace Cike.AspNetCore.MinimalAPIs.Options;

public class GlobalMinimalApiRouteOptions : MinimalApiRouteOptions
{
    public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }

    //public EnglishPluralizationService PluralizationService { get; set; } = new EnglishPluralizationService();
    public GlobalMinimalApiRouteOptions()
    {
        
    }
}

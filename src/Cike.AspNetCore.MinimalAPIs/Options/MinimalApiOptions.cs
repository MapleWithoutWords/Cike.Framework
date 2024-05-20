using System.Reflection;

namespace Cike.AspNetCore.MinimalAPIs.Options;

public class MinimalApiOptions
{
    public List<Assembly> MinimalApiAsseblies { get; set; }

    public MinimalApiOptions()
    {
        MinimalApiAsseblies = new List<Assembly>();
    }

    public void LoadMinimalApi(Assembly assembly)
    {
        MinimalApiAsseblies.Add(assembly);
    }
}

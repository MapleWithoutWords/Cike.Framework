namespace Cike.AspNetCore.MinimalAPIs.Options;

[Obsolete("已经不再需要了，直接 DependOn 引用CikeAspNetCoreMinimalApiModule 模块则自动扫描所有程序集引入")]
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

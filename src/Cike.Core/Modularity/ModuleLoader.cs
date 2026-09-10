namespace Cike.Core.Modularity;

public class ModuleLoader : IModuleLoader
{
    public static IServiceCollection Services { get; set; }

    public CikeModuleContainer LoadCikeModules(Type startupType)
    {
        var cikeModules = new List<Type>();
        var visitedModules = new HashSet<Type>();
        CollectModules(cikeModules, visitedModules, startupType);
        var moduleContainer = new CikeModuleContainer(cikeModules);
        return moduleContainer;
    }

    /// <summary>
    /// 后序遍历收集模块：一个模块只有在它依赖的所有模块都加入列表之后才加入，
    /// 保证依赖模块始终排在依赖它的模块之前（被多个模块依赖时，也会先于所有依赖方初始化）。
    /// </summary>
    private void CollectModules(List<Type> cikeModules, HashSet<Type> visitedModules, Type type)
    {
        if (!visitedModules.Add(type))
        {
            return;
        }

        foreach (var attr in type.GetCustomAttributes<DependsOnAttribute>())
        {
            foreach (var dependModuleType in attr.GetDependedTypes())
            {
                CollectModules(cikeModules, visitedModules, dependModuleType);
            }
        }

        cikeModules.Add(type);
    }
}

namespace Cike.Core.Modularity;

public class ModuleLoader : IModuleLoader
{
    public static IServiceCollection Services { get; set; }

    public CikeModuleContainer LoadCikeModules(Type startupType)
    {
        var cikeModules = new List<Type>();
        ForModuleTypeTree(cikeModules, startupType);
        cikeModules.Add(startupType);
        var moduleContainer = new CikeModuleContainer(cikeModules);
        return moduleContainer;
    }

    private void ForModuleTypeTree(List<Type> cikeModules, Type type)
    {
        var dependsOnAttries = type.GetCustomAttributes<DependsOnAttribute>();
        if (!dependsOnAttries.Any())
        {
            return;
        }

        foreach (var attr in dependsOnAttries)
        {
            foreach (var dependModuleType in attr.GetDependedTypes())
            {
                if (!cikeModules.Contains(dependModuleType))
                {
                    cikeModules.Insert(0, dependModuleType);
                    ForModuleTypeTree(cikeModules, dependModuleType);
                }
            }
        }
    }
}

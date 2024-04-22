namespace Cike.Core.Modularity;

public class CikeModuleContainer
{
    public CikeModuleContainer(List<Type> moduleTypes)
    {
        ModuleTypes = moduleTypes;
        CikeModules = new List<CikeModule>();

        foreach (var item in moduleTypes)
        {
            var cikeModule = (CikeModule)Activator.CreateInstance(item)!;
            CikeModules.Add(cikeModule);
        }
    }

    public List<Type> ModuleTypes { get; }
    public List<CikeModule> CikeModules { get; }
}

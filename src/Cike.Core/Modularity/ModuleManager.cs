namespace Cike.Core.Modularity;

public class ModuleManager
{
    private static List<Type> _moduleTypes = new List<Type>();
    public List<Type> GetModuleTypes()
    {
        if (_moduleTypes.Count <= 0)
        {

        }
        return _moduleTypes;
    }

    public void LoadModules(ApplicationInitializationContext context)
    {
        var moduleLoader = context.ServiceProvider.GetService<IModuleLoader>();
        var moduleTypes = moduleLoader.LoadCikeModules(context.GetType());
        _moduleTypes.AddRange(moduleTypes);
    }
}

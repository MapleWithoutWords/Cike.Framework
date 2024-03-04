namespace Cike.Core.Modularity;

public interface IModuleLoader
{
    CikeModuleContainer LoadCikeModules(Type startupType);
}

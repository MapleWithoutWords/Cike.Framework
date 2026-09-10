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

    /// <summary>
    /// 模块列表，已按依赖关系排序：被依赖的模块在前，启动模块在最后。
    /// 直接按列表顺序执行（ConfigureServices / Initialize），无需在外部反转。
    /// </summary>
    public List<CikeModule> CikeModules { get; }
}

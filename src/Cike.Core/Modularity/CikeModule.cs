namespace Cike.Core.Modularity;

public abstract class CikeModule
{

    public virtual void ConfigureServices(ServiceConfigurationContext context)
    {
    }


    public virtual void Initialize(ApplicationInitializationContext context)
    {
    }

    public virtual void Shutdown(ApplicationShutdownContext context)
    {
    }
}

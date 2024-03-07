namespace Cike.Core.Modularity;

public abstract class CikeModule
{

    public virtual async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
    }

    public virtual void Dispose()
    {

    }

    public virtual async Task InitializeAsync(ApplicationInitializationContext context)
    {
    }

    public virtual async Task ShutdownAsync(ApplicationShutdownContext context)
    {
    }
}

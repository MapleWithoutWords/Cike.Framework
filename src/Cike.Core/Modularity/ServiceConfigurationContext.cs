namespace Cike.Core.Modularity;

public class ServiceConfigurationContext
{
    public ServiceConfigurationContext(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}

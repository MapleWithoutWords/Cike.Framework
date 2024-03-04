namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static TService GetRequireServiceInstance<TService>(this IServiceCollection services)
    {
        return (TService)services.FirstOrDefault(s=>s.ServiceType==typeof(TService))!.ImplementationInstance!;
    }
}

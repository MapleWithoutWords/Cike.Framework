using Cike.Core.DependencyInjection;

namespace Cike.AspNetCore.MinimalAPIs;

public abstract class MinimalApiServiceBase : ISingletonDependency
{
    public MinimalApiRouteOptions RouteOptions { get; set; } = new MinimalApiRouteOptions();

    public string? ServiceName { get; set; }

    public MinimalApiServiceBase() { }
}

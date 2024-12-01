namespace Cike.AspNetCore.MinimalAPIs.EndpointFilters;

public interface IEndpointFilterProvider
{
    public ValueTask<object?> HandlerAsync(EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next);
}

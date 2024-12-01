namespace Cike.AspNetCore.MinimalAPIs.EndpointFilters;

public class AutoValidationEndpointFilterProvider : IEndpointFilterProvider
{
    public ValueTask<object?> HandlerAsync(EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next)
    {
        var endpointFilter = new AutoFluentValidationEndpointFilter(
            ModuleLoader.Services,
            invocationContext.HttpContext.RequestServices,
            invocationContext.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>());
        return endpointFilter.InvokeAsync(invocationContext, next);
    }
}

namespace Cike.AspNetCore.MinimalAPIs.EndpointFilters;

public class AutoValidationAttribute : EndpointFilterBaseAttribute<AutoValidationEndpointFilterProvider>
{
    public AutoValidationAttribute(int order = 999) : base(order)
    {
    }
}

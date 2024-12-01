namespace Cike.AspNetCore.MinimalAPIs.EndpointFilters;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class EndpointFilterBaseAttribute : Attribute
{
    public Type ServiceType { get; }

    public int Order { get; }

    protected EndpointFilterBaseAttribute(Type serviceType, int order)
    {
        ServiceType = serviceType;
        Order = order;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class EndpointFilterBaseAttribute<TEndpointFilterProvider> : EndpointFilterBaseAttribute
    where TEndpointFilterProvider : IEndpointFilterProvider
{
    public EndpointFilterBaseAttribute(int order)
        : base(typeof(TEndpointFilterProvider), order)
    {
    }
}

namespace Cike.Core.DependencyInjection;

public class DependencyAttribute
{
    public virtual ServiceLifetime? Lifetime { get; set; }

    public virtual bool TryRegister { get; set; }

    public virtual bool ReplaceServices { get; set; }

    public DependencyAttribute()
    {

    }

    public DependencyAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}

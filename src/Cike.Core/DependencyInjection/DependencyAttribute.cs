namespace Cike.Core.DependencyInjection;

public class DependencyAttribute : Attribute
{
    public virtual bool TryRegister { get; set; }

    public virtual bool ReplaceServices { get; set; }

    public virtual string? Key { get; set; }

    public DependencyAttribute()
    {

    }
}

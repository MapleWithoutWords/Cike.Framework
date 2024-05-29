namespace Cike.Auth.MultiTenant;

public interface ICurrentTenant
{
    Guid? Id { get; }

    IDisposable Change(Guid tenantId);
}

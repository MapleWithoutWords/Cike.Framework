namespace Cike.Auth.MultiTenant;

public interface ICurrentTenantAccessor
{
    Guid? GetTenantId();
    void SetTenantId(Guid? tenantId);
}

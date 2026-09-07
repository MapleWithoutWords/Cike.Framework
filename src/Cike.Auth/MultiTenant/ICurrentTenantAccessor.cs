namespace Cike.Auth.MultiTenant;

public interface ICurrentTenantAccessor
{
    long GetTenantId();
    void SetTenantId(long tenantId);
}

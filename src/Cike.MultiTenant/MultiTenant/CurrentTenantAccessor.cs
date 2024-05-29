
namespace Cike.Auth.MultiTenant;

public class CurrentTenantAccessor : ICurrentTenantAccessor
{
    protected AsyncLocal<Guid?> AsyncLocalTenantId { get; set; } = new AsyncLocal<Guid?>();
    public Guid? GetTenantId()
    {
        return AsyncLocalTenantId.Value;
    }

    public void SetTenantId(Guid? tenantId)
    {
        AsyncLocalTenantId.Value = tenantId;
    }
}

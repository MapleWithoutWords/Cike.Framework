namespace Cike.Auth.MultiTenant;

public class CurrentTenantAccessor : ICurrentTenantAccessor, ISingletonDependency
{
    protected AsyncLocal<long> AsyncLocalTenantId { get; set; } = new AsyncLocal<long>();
    public long GetTenantId()
    {
        return AsyncLocalTenantId.Value;
    }

    public void SetTenantId(long tenantId)
    {
        AsyncLocalTenantId.Value = tenantId;
    }
}

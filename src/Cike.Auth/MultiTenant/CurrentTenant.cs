using Cike.Core;

namespace Cike.Auth.MultiTenant;

public class CurrentTenant(ICurrentTenantAccessor _currentTenantAccessor) : ICurrentTenant, ITransientDependency
{
    public Guid? Id { get => _currentTenantAccessor.GetTenantId(); }

    public IDisposable Change(Guid tenantId)
    {
        var currentTenantId = _currentTenantAccessor.GetTenantId();

        _currentTenantAccessor.SetTenantId(tenantId);

        return new DisposeAction(() =>
        {
            _currentTenantAccessor.SetTenantId(currentTenantId);
        });
    }
}

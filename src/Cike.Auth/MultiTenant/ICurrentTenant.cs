namespace Cike.Auth.MultiTenant;

public interface ICurrentTenant
{
    long Id { get; }

    IDisposable Change(long tenantId);
}

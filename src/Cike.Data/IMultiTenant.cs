namespace Cike.Data;

public interface IMultiTenant
{
    Guid TenantId { get; set; }
}

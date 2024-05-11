namespace Cike.Data;

public interface IConnectionStringResolver
{
    Task<string> ResolveAsync(string connectionStringName);
}

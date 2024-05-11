
using Microsoft.Extensions.Options;

namespace Cike.Data;

public class DefaultConnectionStringResolver : IConnectionStringResolver
{
    private readonly CikeDbConnectionOptions _options;
    public DefaultConnectionStringResolver(IOptionsMonitor<CikeDbConnectionOptions> options)
    {
        _options = options.CurrentValue;
    }

    public Task<string> ResolveAsync(string connectionStringName)
    {
        return Task.FromResult(InternalResolveAsync(connectionStringName)!);
    }

    public string? InternalResolveAsync(string connectionStringName)
    {
        if (_options.ConnectionStrings.ContainsKey(connectionStringName))
        {
            return _options.ConnectionStrings[connectionStringName];
        }
        return null;
    }
}

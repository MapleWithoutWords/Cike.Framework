using Cike.Core.DependencyInjection;
using Cike.Locks.Options;
using Microsoft.Extensions.Options;

namespace Cike.Locks.DistributedRedis.Internals;

internal class DistributedRedisLock(IDistributedLockProvider distributedLockProvider, IOptionsMonitor<LockOptions> lockOptions) : ILock, ISingletonDependency
{
    public IDisposable? TryGet(
        string key,
        TimeSpan timeout = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var distributedLock = distributedLockProvider.CreateLock(key);

        return distributedLock.TryAcquire(timeout == default ? lockOptions.CurrentValue.DefaultTimeout : timeout);
    }

    public async Task<IAsyncDisposable?> TryGetAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        var distributedLock = distributedLockProvider.CreateLock(key);
        var handle = await distributedLock.TryAcquireAsync(timeout == default ? lockOptions.CurrentValue.DefaultTimeout : timeout, cancellationToken);

        return handle;
    }
}

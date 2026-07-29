using Cike.Core;
using Cike.Core.DependencyInjection;
using Cike.Locks.Abstracts;
using Cike.Locks.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Locks.Internals;

internal class LocalLock(IOptionsMonitor<LockOptions> lockOptions) : ILock, ISingletonDependency
{
    private readonly LazyManualMemoryCache<string, SemaphoreSlim> _localObjects = new();

    public IDisposable? TryGet(string key, TimeSpan timeout = default)
    {
        var semaphore = GetSemaphoreSlim(key);

        if (!semaphore.Wait(timeout == default ? lockOptions.CurrentValue.DefaultTimeout : timeout))
        {
            return null;
        }

        return new DisposeAction(semaphore.Dispose);
    }

    public async Task<IAsyncDisposable?> TryGetAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var semaphore = GetSemaphoreSlim(key);

        if (!await semaphore.WaitAsync(timeout == default ? lockOptions.CurrentValue.DefaultTimeout : timeout, cancellationToken))
        {
            return null;
        }

        return new DisposeAction(() => semaphore.Dispose());
    }

    private SemaphoreSlim GetSemaphoreSlim(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _localObjects.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }
}

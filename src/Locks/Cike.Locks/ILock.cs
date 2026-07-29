using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Locks.Abstracts;

public interface ILock
{
    IDisposable? TryGet(string key, TimeSpan timeout = default);

    Task<IAsyncDisposable?> TryGetAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default);
}

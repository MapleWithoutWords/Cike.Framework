using Cike.Core.DependencyInjection;
using Cike.EventBus.Local.Enums;

namespace Cike.EventBus.Local;

public class LocalEventExecutor : ILocalEventExecutor, IScopedDependency
{
    public int Counter { get; set; }

    public ExecutorStatusEnum Status { get; set; }

    public Exception? Exception { get; set; }

    public void Reset()
    {
        Counter = 0;
        Exception = null;
    }
}

using Cike.EventBus.Local.Enums;

namespace Cike.EventBus.Local;

public interface ILocalEventExecutor
{
    public int Counter { get; set; }

    public ExecutorStatusEnum Status { get; set; }

    public Exception? Exception { get; set; }

    public void Reset();
}

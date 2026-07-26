namespace Cike.EventBus.Local;

public class LocalEventContext : ILocalEventContext, IScopedDependency
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

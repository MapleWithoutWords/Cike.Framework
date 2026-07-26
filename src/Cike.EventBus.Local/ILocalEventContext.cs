namespace Cike.EventBus.Local;

public interface ILocalEventContext
{
    public int Counter { get; set; }

    public ExecutorStatusEnum Status { get; set; }

    public Exception? Exception { get; set; }

    public void Reset();
}

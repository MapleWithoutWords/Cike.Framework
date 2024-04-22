namespace Cike.EventBus;

public interface IEvent
{
    public string GetEventId();

    public void SetEventId(string id);

    public DateTime GetCreationTime();

    public DateTime SetCreationTime(DateTime creationTime);
}

public interface IEvent<TResult> : IEvent
{
    public TResult? Result { get; set; }
}

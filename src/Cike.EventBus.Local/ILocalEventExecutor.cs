namespace Cike.EventBus.Local;

public interface ILocalEventExecutor
{
    public Task ExecuteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    Task CancelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}

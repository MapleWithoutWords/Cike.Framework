namespace Cike.EventBus.LocalEvent;

public interface ILocalEventBus : IEventBus
{
    public Task CancelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}

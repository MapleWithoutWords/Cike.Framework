namespace Cike.EventBus.Local;

public interface ILocalEventBus : IEventBus
{
    public Task CancelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}

namespace Cike.EventBus;

public interface IQueueEventBus : IEventBus
{
    public Task EnqueueAsync<TEvent>(TEvent @event) where TEvent : IEvent;

    public Task PublishQueueAsync();

    public Task<bool> AnyQueueAsync();
}

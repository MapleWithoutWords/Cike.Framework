namespace Cike.EventBus.Adaptive;

public class QueueEventBus : EventBusAdaptive, IQueueEventBus, IScopedDependency
{
    private Queue<IEvent> _eventQueue = new Queue<IEvent>();

    public QueueEventBus(ILocalEventBus localEventBus) : base(localEventBus)
    {
    }

    public Task<bool> AnyQueueAsync()
    {
        return Task.FromResult(_eventQueue.Any());
    }

    public Task EnqueueAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        _eventQueue.Enqueue(@event);
        return Task.CompletedTask;
    }

    public Task PublishQueueAsync()
    {
        while (_eventQueue.Any())
        {
            var @event = _eventQueue.Dequeue();
            PublishAsync((dynamic)@event);
        }
        return Task.CompletedTask;
    }
}

namespace Cike.EventBus.Adaptive;

public class EventBusAdaptive(ILocalEventBus localEventBus) : IEventBus, IScopedDependency
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        if (@event is Local.LocalEvent)
        {
            await localEventBus.PublishAsync(@event, cancellationToken);
        }
    }
}

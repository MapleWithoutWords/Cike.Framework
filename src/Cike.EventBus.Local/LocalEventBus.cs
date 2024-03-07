
namespace Cike.EventBus.LocalEvent;

public class LocalEventBus : ILocalEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        throw new NotImplementedException();
    }
}

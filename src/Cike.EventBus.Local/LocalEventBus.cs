using Cike.EventBus.Local.Channels;

namespace Cike.EventBus.Local;

public class LocalEventBus(
    ILocalEventExecutor _localEventExecutor,
    ILogger<LocalEventBus> _logger,
    IChannelPublisher _channelPublisher) : ILocalEventBus, IScopedDependency
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event is IBackgroundEvent backgroundEvent && backgroundEvent.IsBackgroundThread())
        {
            _logger?.LogDebug("Publishing event {EventName} on background thread", @event.GetType().Name);
            await _channelPublisher.PublishAsync(@event, cancellationToken);
        }
        else
        {
            _logger?.LogDebug("Publishing event {EventName}", @event.GetType().Name);
            await _localEventExecutor.ExecuteAsync(@event, cancellationToken);
        }
    }
    public async Task CancelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        await _localEventExecutor.CancelAsync(@event, cancellationToken);
    }
}

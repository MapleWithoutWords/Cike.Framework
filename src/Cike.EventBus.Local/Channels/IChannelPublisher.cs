namespace Cike.EventBus.Local.Channels;

public interface IChannelPublisher
{
    Task PublishAsync<T>(T data, CancellationToken cancellationToken = default) where T : IEvent;
}

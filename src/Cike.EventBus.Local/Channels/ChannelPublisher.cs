namespace Cike.EventBus.Local.Channels;

public class ChannelPublisher : IChannelPublisher,ISingletonDependency
{
    private readonly IChannelManager _channelManager;

    public ChannelPublisher(IChannelManager channelManager)
    {
        _channelManager = channelManager;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
    {
        var channel = await _channelManager.GetChannel<T>(@event);

        await channel.Writer.WaitToWriteAsync(cancellationToken);
        await channel.Writer.WriteAsync(@event, cancellationToken);
    }
}

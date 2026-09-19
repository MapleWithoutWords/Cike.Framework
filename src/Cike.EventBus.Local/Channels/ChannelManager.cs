namespace Cike.EventBus.Local.Channels;

public class ChannelManager : IChannelManager, ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;

    public List<Task> ChannelTasks = new();

    public ChannelManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Channel<T>> GetChannel<T>(T @event) where T : IEvent
    {
        var channelWarpper = _serviceProvider.GetRequiredService<IChannelWrapper<T>>();

        var channel = await channelWarpper.GetChannel(@event);

        return channel;
    }
}

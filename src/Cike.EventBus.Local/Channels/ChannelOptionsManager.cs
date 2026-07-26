namespace Cike.EventBus.Local.Channels;

public class ChannelOptionsManager
{
    private readonly LazyManualMemoryCache<Type, ChannelOptions> _channelOptionMapper = new();

    public ChannelOptions? GetChannelOptions<T>()
    {
        _channelOptionMapper.Get(typeof(T), out var options);

        return options;
    }

    public ChannelOptionsManager RegisterChannelOptions<T>(ChannelOptions options)
    {
        _channelOptionMapper.AddOrUpdate(typeof(T), (_) => options);

        return this;
    }
}

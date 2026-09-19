namespace Cike.EventBus.Local.Channels;

public interface IChannelWrapper<T> where T : IEvent
{
    ValueTask<Channel<T>> GetChannel(T @event);
}

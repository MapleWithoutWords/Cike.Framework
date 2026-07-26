namespace Cike.EventBus.Local.Channels;

public interface IChannelWarpper<T> where T : IEvent
{
    ValueTask<Channel<T>> GetChannel(T @event);
}

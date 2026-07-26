namespace Cike.EventBus.Local.Channels;

public interface IChannelManager
{
    Task<Channel<T>> GetChannel<T>(T @event) where T : IEvent;
}

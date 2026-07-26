using Cike.EventBus.Local.Channels;

namespace Cike.EventBus.Local.Options;

public class LocalEventBusOptions
{
    public ChannelOptions DefaultChannelOptions { get; set; } = new UnboundedChannelOptions();

    public ChannelOptionsManager ChannelOptionsManager { get; } = new();
}

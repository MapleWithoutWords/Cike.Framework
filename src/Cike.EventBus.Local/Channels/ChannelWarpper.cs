using Microsoft.Extensions.Options;

namespace Cike.EventBus.Local.Channels;

internal class ChannelWarpper<T> : IChannelWarpper<T> where T : IEvent
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<ChannelWarpper<T>>? _logger;

    private readonly LocalEventBusOptions _options;

    private Channel<T>? _channel = null;

    private Task? _subscribeTask = null;

    public ChannelWarpper(IOptions<LocalEventBusOptions> options, IServiceProvider serviceProvider, ILogger<ChannelWarpper<T>>? logger = null)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public ValueTask<Channel<T>> GetChannel(T @event)
    {
        if (@event is not IBackgroundEvent backgroundEvent)
        {
            throw new NotSupportedException("Event must implement IBackgroundEvent");
        }

        var channel = _channel;
        if (channel == null)
        {
            var channelOptions = _options.ChannelOptionsManager.GetChannelOptions<T>() ?? _options.DefaultChannelOptions;

            if (channelOptions is BoundedChannelOptions boundedChannelOptions)
                channel = Channel.CreateBounded<T>(boundedChannelOptions);
            else if (channelOptions is UnboundedChannelOptions unboundedChannelOptions)
                channel = Channel.CreateUnbounded<T>(unboundedChannelOptions);
            else
                channel = Channel.CreateUnbounded<T>();

            Interlocked.CompareExchange(ref _channel, channel, null);
            if (_channel == channel)
            {
                _subscribeTask = Subscribe(_channel);
            }
        }

        return new ValueTask<Channel<T>>(_channel!);
    }

    private async Task Subscribe(Channel<T> channel)
    {
        await Parallel.ForEachAsync(channel.Reader.ReadAllAsync(), async (Data, ctx) =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var executor = scope.ServiceProvider.GetRequiredService<ILocalEventExecutor>();

                await executor.ExecuteAsync(Data, ctx);
                _logger?.LogDebug("Event {EventName} has been executed", Data.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error while executing event {EventName}", Data.GetType().Name);
            }
        });
    }
}

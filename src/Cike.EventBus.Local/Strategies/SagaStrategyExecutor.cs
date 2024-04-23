using Cike.Core.DependencyInjection;
using Cike.EventBus.Local.Enums;
using Cike.EventBus.LocalEvent;
using Microsoft.Extensions.Logging;

namespace Cike.EventBus.Local.Strategies;

public class SagaStrategyExecutor : IStrategyExecutor,ISingletonDependency
{
    private readonly ILogger<SagaStrategyExecutor>? _logger;

    public SagaStrategyExecutor(ILogger<SagaStrategyExecutor>? logger = null)
    {
        _logger = logger;
    }
    public async Task ExecuteAsync<TEvent>(LocalEventHandlerAttribute eventHandlerAttribute, TEvent @event, Func<Task> func, Func<Exception, FailureLevelEnum, Task> cancel) where TEvent : IEvent
    {
        var retryTimes = 0;
        Exception? exception;

        do
        {
            try
            {
                await func.Invoke();
                return;
            }
            catch (Exception ex)
            {
                exception = ex;

                if (retryTimes++ > 0)
                {
                    _logger?.LogError(ex, $"Strategy execute event failed [{{RetryTimes}} / {eventHandlerAttribute.RetryCount}], event id: {{EventId}}, event: {{@Event}}", retryTimes, @event.GetEventId(), @event);
                }
            }
        } while (retryTimes <= eventHandlerAttribute.RetryCount);

        await cancel(exception, eventHandlerAttribute.FailureLevel);
    }
}

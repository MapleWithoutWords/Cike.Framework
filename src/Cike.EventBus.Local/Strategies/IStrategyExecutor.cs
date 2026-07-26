using Cike.EventBus.Local.Enums;

namespace Cike.EventBus.Local.Strategies;

public interface IStrategyExecutor
{
    public Task ExecuteAsync<TEvent>(LocalEventHandlerAttribute eventHandlerAttribute, TEvent @event, Func<Task> func, Func<Exception, FailureLevelEnum, Task> cancel)
        where TEvent : IEvent;
}

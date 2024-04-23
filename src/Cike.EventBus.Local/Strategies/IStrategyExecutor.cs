using Cike.EventBus.Local.Enums;
using Cike.EventBus.LocalEvent;

namespace Cike.EventBus.Local.Strategies;

public interface IStrategyExecutor
{
    public Task ExecuteAsync<TEvent>(LocalEventHandlerAttribute eventHandlerAttribute, TEvent @event, Func<Task> func, Func<Exception, FailureLevelEnum, Task> cancel)
        where TEvent : IEvent;
}

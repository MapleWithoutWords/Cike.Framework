using Cike.EventBus.Local;

namespace Cike.EventBus.Adaptive;

public class EventBusAdaptive(ILocalEventBus localEventBus) : IEventBus, IScopedDependency
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        // 不做类型筛选直接转发本地总线：领域事件（DomainEvent）、Command/Query 都继承自 Event 而非 LocalEvent，
        // 按 is LocalEvent 过滤会把它们静默丢弃——尤其破坏"领域事件在事务提交前发布并被 [LocalEventHandler] 消费"的链路
        await localEventBus.PublishAsync(@event, cancellationToken);
    }
}

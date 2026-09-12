using Cike.EventBus.Local;

namespace Cike.EventBus.Adaptive;

public class QueueEventBus : EventBusAdaptive, IQueueEventBus, IScopedDependency
{
    private Queue<IEvent> _eventQueue = new Queue<IEvent>();

    public QueueEventBus(ILocalEventBus localEventBus) : base(localEventBus)
    {
    }

    public Task<bool> AnyQueueAsync()
    {
        return Task.FromResult(_eventQueue.Any());
    }

    public Task EnqueueAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        _eventQueue.Enqueue(@event);
        return Task.CompletedTask;
    }

    public async Task PublishQueueAsync()
    {
        while (_eventQueue.Any())
        {
            var @event = _eventQueue.Dequeue();
            // 必须逐个 await：漏掉 await 会让 handler 变成 fire-and-forget，
            // 与"工作单元提交前、同事务内消费领域事件"的语义冲突（竞态 + 异常被吞）
            await PublishAsync((dynamic)@event);
        }
    }
}

# Cike.EventBus

事件体系基座：定义事件契约与总线抽象，不含任何实现。本地实现见 [Cike.EventBus.Local](../Cike.EventBus.Local/README.md)，队列/自适应实现见 [Cike.EventBus.Adaptive](../Cike.EventBus.Adaptive/README.md)。

## 提供的能力

| 类型 | 说明 |
|---|---|
| `IEvent` | 事件契约（`GetEventId()` / `GetCreationTime()`） |
| `Event` | 事件基类（record），自动生成事件 Id 与创建时间；`EnableBackgroundThread()` 切后台通道 |
| `Event<TResult>` | 带结果的事件（`Result` 属性由 handler 赋值，调用方读取） |
| `IBackgroundEvent` | 后台事件标记（`IsBackgroundThread()` / `EnableBackgroundThread()`） |
| `IEventBus` | `PublishAsync<TEvent>(@event)` |
| `IQueueEventBus` | 事件队列：`EnqueueAsync` / `AnyQueueAsync` / `PublishQueueAsync`（领域事件发布用，实现在 Adaptive 包） |

【陷阱】`PublishAsync` 对没有注册 handler 的事件类型**静默忽略**（不抛错）——新事件"发布没反应"时先检查 handler 是否标注 `[LocalEventHandler]`、事件类型是否继承 `Event`。

## 模块信息

- 模块类：`CikeEventBusModule`（无逻辑）
- 直接依赖：无

## 更多

完整事件语义（同步派发、后台通道、Saga）：[AI 开发指南](../../docs/AI-GUIDE.md) 第 5 节。

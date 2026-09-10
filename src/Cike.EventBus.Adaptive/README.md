# Cike.EventBus.Adaptive

自适应事件总线与**领域事件队列**。核心价值是 `IQueueEventBus`：聚合根上的领域事件先入队、在数据库事务提交前依次发布。

## 提供的能力

| 类型 | 说明 |
|---|---|
| `EventBusAdaptive : IEventBus` | 把 `PublishAsync` 转发给本地事件总线 |
| `QueueEventBus : IQueueEventBus` | 进程内事件队列：`EnqueueAsync` 入队 / `AnyQueueAsync` 查询 / `PublishQueueAsync` 依次发布 |
| `CikeEventBusAdaptiveModule` | 模块类 |

领域事件的完整链路：`CikeDbContext.SaveChangesAsync` 把聚合根（`AggregateRoot.DomainEvents`）上的事件逐个入队 → `IUnitOfWork.CommitAsync` 在提交数据库事务前 drain 队列逐个 `PublishAsync`（handler 同样是 `[LocalEventHandler]` 方法）。

【陷阱】模块只声明依赖 `CikeEventBusModule`，但实现（`QueueEventBus` 构造注入 `ILocalEventBus`）**需要 `CikeEventBusLocalModule` 一起加载**——确保宿主的依赖树里有它（典型业务应用都在 Application 层显式依赖 Local 包）。

## 模块信息

- 模块类：`CikeEventBusAdaptiveModule`（无逻辑）
- 直接依赖：`CikeEventBusModule`（实际运行还需要 `CikeEventBusLocalModule`）

## 更多

领域事件用法：[AI 开发指南](../../docs/AI-GUIDE.md) 第 5.8 节。

# Cike.Cqrs

CQRS 用例基类。**没有独立 Dispatcher**——Command/Query 的派发复用本地事件总线（`ILocalEventBus.PublishAsync`）实现。

## 提供的能力

| 类型 | 定义 |
|---|---|
| `Command : Event, ICommand` | 无返回值用例；需要回传数据时加可写属性（如 `public long Id { get; set; }`，handler 填充后调用方读取） |
| `Query<TResult> : Event<TResult>, IQuery<TResult>` | 有返回值用例；handler 给 `query.Result` 赋值 |
| `ICommand` / `IQuery<TResult>` | 标记接口（继承 `IEvent`） |

典型写法：

```csharp
public record AddTodoCommand(AddTodoDto Dto) : Command { public long Id { get; set; } }
public record GetTodoQuery(long Id) : Query<TodoDetailDto>;

// Handler（任意类 + 方法标注）
[LocalEventHandler]
public async Task AddAsync(AddTodoCommand command, CancellationToken ct = default) { ... }

// Service 层派发
var command = new AddTodoCommand(dto);
await localEventBus.PublishAsync(command, ct);
return TypedResults.Ok(command.Id);
```

【陷阱】不继承 `Command` / `Query<TResult>` 的类型不是 `IEvent`，发布时被静默忽略。

## 模块信息

- 模块类：`CikeCqrsModule`（无逻辑）
- 直接依赖：`CikeEventBusModule`

## 更多

完整 Handler 规则与全链路 Recipe：[AI 开发指南](../../docs/AI-GUIDE.md) 第 5、9 节。

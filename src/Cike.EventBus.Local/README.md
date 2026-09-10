# Cike.EventBus.Local

本地事件总线：进程内同步事件派发，同时是框架 CQRS 的执行引擎。基于 Channel 提供可选的后台线程模式。

## 提供的能力

- **`ILocalEventBus`**：`PublishAsync<TEvent>`（默认同步执行，返回时 handler 已完成）+ `CancelAsync`
- **`[LocalEventHandler]`**：标注在任意类的 public 方法上即注册为 handler，特性参数：
  - `Order`（默认 100，升序执行）
  - `IsCancel = true`：注册为取消/补偿 handler（Saga）
  - `RetryCount`：失败重试次数（共 1+n 次尝试）
  - `FailureLevel`：`Throw`（默认）/ `ThrowAndCancel` / `Ignore`
- **`BackgroundEvent`**：继承它（或调用 `EnableBackgroundThread()`）→ 事件写入 Channel，由 `Parallel.ForEachAsync` 在独立 DI Scope 中消费，异常仅记日志
- **`ILocalEventMiddleware<TEvent>`**：事件中间件管线，内置三个（`OncePerTree` 策略，仅根发布装配）：
  - `DbTransactionLocalEventMiddleware`：事件树成功 `IUnitOfWork.CommitAsync` / 异常 `RollbackAsync`
  - `ExceptionLocalEventMiddleware`：状态跟踪（注意：`Succeed`/`InProgress` 分支当前是死代码，见陷阱）
  - `PreventRecursiveMiddleware`：树结束时重置嵌套计数

## 隐式行为

- Handler 方法约束：**有且仅有一个**参数是 `IEvent` 派生类型；额外参数从 DI 解析；`CancellationToken` 自动传入
- Handler 所在类不需要标记接口——模块自动把含 `[LocalEventHandler]` 方法的类 `AddScoped`
- 【陷阱】无 handler 的事件静默忽略；后台事件不参与发布方事务、异常不通知发布方
- 【陷阱】旧版包（`FailureLevel` 默认值修正前）中未显式设置 `FailureLevel` 且事件带取消 handler 时，失败会抛 `NotImplementedException`
- 【陷阱】`ExceptionLocalEventMiddleware` 的"已 Succeed 后异常→全量取消"分支依赖 `Status = Succeed`，但当前源码没有任何地方设置它——勿依赖"提交失败自动补偿"

## 模块信息

- 模块类：`CikeEventBusLocalModule`
- 直接依赖：`CikeEventBusModule`

## 更多

完整语义（Saga 补偿规则、事务边界、中间件自定义）：[AI 开发指南](../../docs/AI-GUIDE.md) 第 5.3–5.7 节。

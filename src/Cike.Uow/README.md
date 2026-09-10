# Cike.Uow

工作单元抽象：事务边界契约。接口在本包，**EF Core 实现在 [Cike.Data.EFCore](../Cike.Data.EFCore/README.md) 的 `EFCoreUnitOfWork<TDbContext>`**（由 `AddCikeDbContext<T>()` 注册）。

## 提供的能力

| 类型 | 说明 |
|---|---|
| `IUnitOfWork` | `BeginTranscationAsync` / `CommitAsync` / `RollbackAsync`、`DbContext`、`IsTransactionBegun`、`DbTransaction`、`CommitState` |
| `UnitOfWorkOptions` | `Enable`（默认 **true**）：实体状态变化时 `CikeDbContext` 自动开事务 |
| `UnitOfWorkCommitState` | `Unknown` → `Uncommitted` → `Committed` / `Rollbacked` |
| `CikeUowModule` | 模块类（无逻辑） |

谁在用它：
- `DbTransactionLocalEventMiddleware`（Cike.EventBus.Local）——每次根 `PublishAsync` 包一个事务
- `CikeDbContext`——跟踪到实体变化时自动 `BeginTransactionAsync`
- `EFCoreUnitOfWork.CommitAsync`——提交前先 drain 领域事件队列（`IQueueEventBus`）

【注意】`CommitState == Unknown`（没碰过数据库）时 `Commit/Rollback` 是空操作，不报错。

## 模块信息

- 模块类：`CikeUowModule`
- 直接依赖：无

## 更多

事务边界语义：[AI 开发指南](../../docs/AI-GUIDE.md) 第 5.4 节。

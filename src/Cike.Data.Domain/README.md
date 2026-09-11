# Cike.Data.Domain

领域实体基类与仓储抽象：审计/软删/聚合根的现成实现 + 三层仓储接口。业务实体的唯一推荐来源（不要自己拼 `IEntity + ISoftDelete` 组合）。

## 提供的能力（`Cike.Data.Domain.Entities` / `AggregateRoots` / `Repositories`）

### 实体基类

继承链：

| 基类 | 字段 |
|---|---|
| `Entity<TKey>` | `Id` |
| `AuditedEntity<TKey>` | + `CreatedAt` `CreatedBy` `UpdatedAt` `UpdatedBy`（UserId 为 `long`） |
| `FullAuditedEntity<TKey>` | + `IsDeleted` |
| `AggregateRoot<TKey>` | + `DomainEvents`（`AddDomainEvent` / `ClearDomainEvents`） |
| `FullAuditedAggregateRoot<TKey>` | 审计 + 软删 + `DomainEvents` + `ConcurrencyStamp`——**标准业务实体推荐基类** |

其他：`DomainEvent` / `IDomainEvent`、`ValueObject`、`EntityHelper`。

标准写法：

```csharp
public class Folder : FullAuditedAggregateRoot<long>, IMultiTenant
{
    public long TenantId { get; set; }
    // ...业务字段
}
```

【注意】`Cike.Data` 包里也有一组契约接口（`IEntity` 等）。本包的 `AggregateRoot` / `FullAuditedAggregateRoot` 链实现的是 `Cike.Data.Domain.AggregateRoots` 命名空间下的一组**同名接口**（与 `Cike.Data` 契约层的接口重复，待统一）；业务代码引用基类用本包。

### 仓储接口（`Cike.Domain.Repositories`）

三层结构，按注入侧的读写职责选型：

```
IReadOnlyRepository<TEntity, TKey>    // 查询：GetAsync / FindAsync / GetListAsync / GetPagedListAsync / GetCountAsync / AnyAsync
    └─ IBasicRepository<TEntity, TKey>   // + 增删改：Insert / Update / Delete（及 Many 变体、按 Id 删除），均带 autoSave
        └─ IRepository<TEntity, TKey>    // 合并标记接口
```

- **CQRS 查询侧**注入 `IReadOnlyRepository`——编译期即无写能力
- **命令侧 / 常规业务**注入 `IRepository`
- 语义约定：`GetAsync` 未找到抛 `UserFriendlyException`；`FindAsync` 返回 null；`DeleteAsync(id)` 不存在时静默返回（幂等）；`GetPagedListAsync` 返回 `(Total, Items)` 元组
- `autoSave` 默认 true 立即保存；配合工作单元传 false 由事务提交统一保存
- 接口不依赖任何 ORM——EF Core 实现见 **Cike.Data.EFCore**（默认仓储自动注册 + `GetQueryableAsync` 出口）

## 模块信息

- 模块类：`CikeDomainModule`（无逻辑）
- 直接依赖：`CikeDataModule`

## 更多

基类选择表与自动化行为：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7.1 节。

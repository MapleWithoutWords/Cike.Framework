# Cike.Data.Domain

领域实体基类：审计/软删/聚合根的现成实现。业务实体的唯一推荐来源（不要自己拼 `IEntity + ISoftDelete` 组合）。

## 提供的能力（`Cike.Data.Domain.Entities` / `AggregateRoots`）

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

【注意】`Cike.Data` 包里也有一组契约接口（`IEntity` 等），本包的基类实现它们——**业务代码引用基类用本包，判断能力用 `Cike.Data` 接口**。

## 模块信息

- 模块类：`CikeDomainModule`（无逻辑）
- 直接依赖：`CikeDataModule`

## 更多

基类选择表与自动化行为：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7.1 节。

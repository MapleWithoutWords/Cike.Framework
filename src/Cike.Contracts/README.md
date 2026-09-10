# Cike.Contracts

跨层数据契约：DTO 基类、分页契约与查询辅助扩展。无外部依赖，被所有业务分层引用。

## 提供的能力

### DTO 基类（`Cike.Contracts.EntityDtos`）
| 类型 | 字段 |
|---|---|
| `EntityDto<TKey>` | `Id` |
| `AuditedEntityDto<TKey>` | + `CreatedAt` `CreatedBy` `UpdatedAt` `UpdatedBy` |
| `FullAuditedEntityDto<TKey>` | + `IsDeleted` |
| `PagedAndSortedResultRequest` | `Page`（默认 1）、`PageSize`（默认 10）、`Sorting`（System.Linq.Dynamic 语法，如 `"CreatedAt desc"`） |
| `PagedResultDto<T>` | `Total`（long）+ `Items` |

### 查询辅助扩展（`Cike.Contracts.Extensions`）
```csharp
query.WhereIf(condition, x => ...)                      // 条件 Where
var (total, items) = await query.ToPaginationAsync(pageDto); // Count + OrderBy(Sorting) + Skip/Take
```

【陷阱】`ToPaginationAsync` 把 `Sorting` 字符串直接透传给 `System.Linq.Dynamic` 的 `OrderBy`——不要让前端传任意排序字段，先白名单化。

## 模块信息

- 模块类：`CikeContractsModule`（无逻辑）
- 直接依赖：无

## 更多

分页查询标准用法与 Recipe：[AI 开发指南](../../docs/AI-GUIDE.md) 第 8.7、9 节。

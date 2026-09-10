# Cike.Data

数据访问契约层：实体契约接口（无实现）、连接串解析、数据过滤器。是 `Cike.Data.Domain` / `Cike.Data.EFCore` 的地基。

## 提供的能力

### 实体契约接口（业务实体/Dto 通过实现这些接口获得框架自动化）
| 接口 | 触发的自动化 |
|---|---|
| `IEntity<TKey>` | Id 自动生成（long→雪花 / Guid→顺序 Guid） |
| `IAuditedEntity<TUserId>` | 审计字段自动填充（`CreatedAt/CreatedBy/UpdatedAt/UpdatedBy`） |
| `ISoftDelete` | 删除变软删（`IsDeleted=true`）、查询自动过滤已删数据 |
| `IMultiTenant`（`long TenantId`） | 插入自动填 TenantId、查询自动按租户过滤 |
| `IHasConcurrencyStamp` | 修改时自动刷新乐观并发戳 |
| `IPagedAndSortedRequest` | 分页契约（`Page/PageSize/Sorting`） |

### 连接串解析
- `[ConnectionStringName("name")]`：DbContext 连接串名 = 类名去掉 `DbContext` 后缀，特性可覆盖
- `IConnectionStringResolver` / `DefaultConnectionStringResolver`：按名字查 `appsettings.json` 的 `ConnectionStrings` 节（key 必须严格匹配）
- `CikeDbConnectionOptions`：绑定整份配置的 `ConnectionStrings` 字典

### 数据过滤器
- `IDataFilter.Enable<T>()/Disable<T>()`：运行时开关全局查询过滤器（软删/多租户），默认启用

## 模块信息

- 模块类：`CikeDataModule`
- 直接依赖：`CikeEventBusModule`

## 更多

连接串解析规则与过滤器语义：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7.1、7.3 节。

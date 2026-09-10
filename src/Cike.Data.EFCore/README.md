# Cike.Data.EFCore

EF Core 集成核心：`CikeDbContext` 基类（一大堆自动化）、DbContext 工厂与 `IUnitOfWork` 实现。数据库方言由 Provider 包（MySql / SqlServer）提供。

## 提供的能力

### `CikeDbContext<TDbContext>` —— 业务 DbContext 基类
- `public class XxxDbContext : CikeDbContext<XxxDbContext>`，**泛型参数必须是自身**；构造函数必须保留 `(DbContextOptions<T>, IServiceProvider)` 双参
- 自动化（全部在 ChangeTracker 钩子里，业务代码禁止手工做）：
  - 新增：雪花/顺序 Guid Id、`TenantId`（`IMultiTenant`）、`CreatedAt/CreatedBy`
  - 修改：`UpdatedAt/UpdatedBy`、`ConcurrencyStamp` 刷新
  - 删除：软删（`Reload + IsDeleted=true`）
  - 全局查询过滤器：`IMultiTenant` → 按租户、`ISoftDelete` → 排除已删（由 `IDataFilter` 开关，默认开）
  - `SaveChangesAsync` 时聚合根领域事件入队 `IQueueEventBus`；实体状态变化自动开事务（`UnitOfWorkOptions.Enable`）

### 注册与工厂
- `services.AddCikeDbContext<T>()`：注册 `DbContextOptions` 工厂（内部走连接串解析）+ `IUnitOfWork → EFCoreUnitOfWork<T>`
- 【陷阱】不要用 `AddDbContext/AddDbContextPool`——会绕开连接串解析与 UoW 集成

## 模块信息

- 模块类：`CikeDataEFCoreModule`（开启 Snowflake `IsEnable`）
- 直接依赖：`CikeAuthModule`、`CikeDomainModule`、`CikeUniversalIdModule`、`CikeUowModule`、`CikeEventBusAdaptiveModule`

## 更多

DbContext 标准写法、连接串解析、迁移 factory：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7.2、7.3、7.5 节。

# Cike.Data.EFCore.SqlServer

SQL Server 方言 Provider，与 [Cike.Data.EFCore.MySql](../Cike.Data.EFCore.MySql/README.md) 同构。依赖本模块即完成数据库方言配置。

## 做了什么

`CikeDataEFCoreSqlServerModule.ConfigureServicesAsync`：
1. `CikeSequentialGuidGeneratorOptions.DefaultSequentialGuidType` 默认设为 `SequentialAtEnd`（SQL Server 主键排序友好）
2. `Configure<CikeDbContextOptions>(options => options.UseSqlServer())`——并默认启用 **SplitQuery**（`QuerySplittingBehavior.SplitQuery`）

同 MySql 包的陷阱：`CikeDbContextOptions` 单委托、后设置者整体覆盖——应用模块里 `Configure<CikeDbContextOptions>(options => options.UseSqlServer(...))` 会整体替换本模块默认配置（须写完整配置），不写则用默认（框架模块先、应用模块后执行）。

## 模块信息

- 模块类：`CikeDataEFCoreSqlServerModule`
- 直接依赖：`CikeDataEFCoreModule`
- 连接串 key：DbContext 类名去掉 `DbContext` 后缀，须与 `ConnectionStrings` 的 key 严格匹配

## 更多

完整数据访问规范：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7 节。

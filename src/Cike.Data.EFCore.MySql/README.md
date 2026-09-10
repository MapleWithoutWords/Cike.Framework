# Cike.Data.EFCore.MySql

MySQL/Pomelo 方言 Provider。**依赖本模块即完成数据库方言配置**（`UseMySQL`），业务侧无需再写任何 provider 配置。

## 做了什么

`CikeDataEFCoreMySqlModule.ConfigureServicesAsync`：
1. `CikeSequentialGuidGeneratorOptions.DefaultSequentialGuidType` 默认设为 `SequentialAsString`（MySQL 字符串主键排序友好）
2. `Configure<CikeDbContextOptions>(options => options.UseMySQL())`——设置连接串解析后的 `UseMySql` 配置

【陷阱】`CikeDbContextOptions` 内部是**单委托、后设置者整体覆盖**，而模块执行顺序是应用先、框架后——在应用模块里 `Configure<CikeDbContextOptions>` 会被本模块**静默覆盖**。自定义 options 时请勿依赖 Provider 模块的默认配置（详见 AI 指南 7.2）。

## 模块信息

- 模块类：`CikeDataEFCoreMySqlModule`
- 直接依赖：`CikeDataEFCoreModule`
- 连接串 key：DbContext 类名去掉 `DbContext` 后缀，须与 `appsettings.json` → `ConnectionStrings` 的 key 严格匹配

## 更多

完整数据访问规范：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7 节。

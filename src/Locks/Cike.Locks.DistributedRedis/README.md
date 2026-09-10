# Cike.Locks.DistributedRedis

基于 Medallion.Threading.Redis + StackExchange.Redis 的分布式锁，实现 [Cike.Locks](../Cike.Locks/README.md) 的 `ILock` 契约。

## 做了什么

`CikeLockDistributedRedisModule.ConfigureServicesAsync`：注册 `IDistributedLockProvider`（`RedisDistributedSynchronizationProvider`）。**必须在配置里提供**：

```csharp
context.Services.Configure<CikeRedisDistributedLockOptions>(options =>
{
    options.RedisConnectionString = "...";   // 或直接给 options.RedisDatabase = 一个 IDatabase
});
```

未配置时模块初始化直接抛 `ApplicationException`。

## 陷阱

【重要】`DistributedRedisLock` 和 `LocalLock` 都实现 `ILock` 且都走自动注册。框架的接口注册规则是**后注册者胜**，而模块加载顺序是"依赖在前、上层在后"——同时依赖本模块和 `Cike.Locks` 时，`ILock` 默认解析到 **`LocalLock`（本地锁）**，分布式锁**静默不生效**。需要分布式锁时用 `[Dependency(ReplaceServices = true)]` 或 keyed 注册显式选择实现。

## 模块信息

- 模块类：`CikeLockDistributedRedisModule`
- 直接依赖：`CikeLocksModule`

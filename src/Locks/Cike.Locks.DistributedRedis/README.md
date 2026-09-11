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

【重要】`DistributedRedisLock` 和 `LocalLock` 都实现 `ILock` 且都走自动注册，接口注册 `Add` **后注册者胜**。本模块依赖 `CikeLocksModule`、后执行后注册——同时依赖两个包时 `ILock` 解析到 **`DistributedRedisLock`**（且未配置连接串时解析即抛异常）。要强制使用本地锁：只依赖 `Cike.Locks`，或用 `[Dependency(ReplaceServices = true)]` / keyed 注册显式选择实现。（旧版包行为相反：默认解析到 LocalLock，分布式锁静默不生效。）

## 模块信息

- 模块类：`CikeLockDistributedRedisModule`
- 直接依赖：`CikeLocksModule`

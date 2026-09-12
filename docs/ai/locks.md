# 锁

## 域定位

解决互斥问题：保证同一时刻只有一个执行流进入临界区（防重复执行、防并发写坏状态）。统一契约 `ILock`：`TryGet`/`TryGetAsync` 返回 `null` = 获取失败，返回 handle = 持锁，`Dispose`/`DisposeAsync` 即释放。

| 场景 | 选择 |
|---|---|
| 单实例部署、纯进程内互斥 | `Cike.Locks`（`LocalLock`，基于 `SemaphoreSlim(1,1)`）。注意现状缺陷：同一 key 进程生命周期内只能成功获取一次（见下文【陷阱】） |
| 多实例部署、跨进程互斥 | `Cike.Locks.DistributedRedis`（Redis 分布式锁，Medallion） |

选型前必须知道的全局规则：**两包共存时 `ILock` 解析到 DistributedRedis 实现**（后注册者胜），且此时**未配置连接串会在首次解析 `ILock` 时抛 `ApplicationException`**（不是启动时抛）。锁可与[缓存](./caching.md)共用同一 Redis 服务器，但连接与配置相互独立；模块加载与自动 DI 机制见[框架内核](./framework-core.md)。

## 覆盖的包

| 包名 | 职责 | 依赖 |
|---|---|---|
| `Cike.Locks` | `ILock` 抽象 + 进程内实现 `LocalLock`（internal，自动注册） | `Cike.Core`（项目引用） |
| `Cike.Locks.DistributedRedis` | `ILock` 的 Redis 分布式实现 `DistributedRedisLock`（internal，自动注册） | `Cike.Locks`（项目引用）；NuGet `DistributedLock.Redis` 1.1.1（即 Medallion.Threading.Redis，传递引入 StackExchange.Redis） |

两个包均为 net8.0。

## Cike.Locks

### 定位

定义锁契约 `ILock` 并提供进程内实现 `LocalLock`：key → `SemaphoreSlim(1,1)`，进程内互斥。模块类 `CikeLocksModule` 为空类（无任何配置逻辑），仅作为模块图锚点；实现类通过标记接口自动注册。单实例部署或纯进程内去重场景使用；**多实例部署下无跨进程互斥能力**。

### 能力清单

```csharp
// 命名空间：Cike.Locks.Abstracts（注意：ILock 不在 Cike.Locks 命名空间下）
public interface ILock
{
    // 获取失败返回 null；成功返回持锁句柄，Dispose 即释放
    IDisposable? TryGet(string key, TimeSpan timeout = default);

    // 获取失败返回 null；成功返回持锁句柄，DisposeAsync 即释放
    Task<IAsyncDisposable?> TryGetAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default);
}

// 命名空间：Cike.Locks.Options
public class LockOptions
{
    public TimeSpan DefaultTimeout { get; set; }   // 未配置时为 TimeSpan.Zero（不等待）
}
```

| 组件 | 说明 |
|---|---|
| `LocalLock` | internal（`Cike.Locks.Internals`），`ILock + ISingletonDependency` → 自动注册为 Singleton；key → `SemaphoreSlim(1,1)`，缓存在 `LazyManualMemoryCache<string, SemaphoreSlim>`（随单例存活整个进程） |
| `CikeLocksModule` | 空模块类，命名空间 `Cike.Locks.Abstracts`；直接依赖仅 `Cike.Core` |
| `DisposeAction` | 持锁句柄的运行时类型（来自 `Cike.Core`），同时实现 `IDisposable` 与 `IAsyncDisposable` |

### 隐式行为

1. **自动注册**：`LocalLock` 实现标记接口即被程序集扫描注册为 `ILock`（Singleton）。模块图包含 `CikeLocksModule` 即可注入 `ILock`，无需手写注册。
2. **timeout 回退规则**：`timeout == default` 时使用 `LockOptions.DefaultTimeout`。注意 `TimeSpan.Zero == default`——**显式传 `TimeSpan.Zero` 也会回退到 `DefaultTimeout`**，无法表达"零等待试一次"。要无限等待传 `Timeout.InfiniteTimeSpan`。框架自身从不给 `DefaultTimeout` 赋值，未配置时为 `TimeSpan.Zero`（单次尝试、不等待）。
3. **【陷阱】一次性锁缺陷（源码核实仍存在）**：`TryGet` 成功后返回 `new DisposeAction(semaphore.Dispose)`——释放时执行的是信号量的 **`Dispose()` 而非 `Release()`**，且 key→信号量的缓存条目不会移除（`LocalLock` 从不调用 `Remove`）。后果：
   - 同一 key 的第二次 `TryGet`/`TryGetAsync`（无论前次是否成功）会在 `Wait`/`WaitAsync` 上抛 `ObjectDisposedException`；
   - 并发场景更早暴露：线程 A 持锁、线程 B 在同 key 的 `Wait` 中阻塞，A 释放（Dispose）后 B 直接抛 `ObjectDisposedException`；
   - 净效果：**每个 key 在进程生命周期内只能成功获取一次**。`LocalLock` 现状仅适合"每 key 单次"的互斥。
4. **非重入**：持锁线程内嵌套获取同一 key 会阻塞到超时返回 `null`（`SemaphoreSlim` 不识别线程）。
5. **key 校验**：`null` 或空白字符串 key 抛 `ArgumentNullException(nameof(key))`。
6. **取消行为**：`TryGetAsync` 的 `cancellationToken` 触发时抛 `OperationCanceledException`（不是返回 `null`）；超时才返回 `null`。
7. **作用域**：仅当前进程内互斥，不跨实例、不跨进程。

### 示例

```csharp
using Cike.Locks.Abstracts; // ILock

public class DedupWorker(ILock lockService)
{
    // 同步用法：TryGet + using（Dispose 即释放）
    public bool RunOnce(string key)
    {
        using var handle = lockService.TryGet(key, TimeSpan.FromSeconds(5));
        if (handle is null)
        {
            return false; // 5 秒内未拿到锁
        }

        // 持锁执行互斥段
        return true;
    }

    // 异步用法：TryGetAsync + await using（DisposeAsync 即释放）
    public async Task<bool> RunOnceAsync(string key, CancellationToken ct)
    {
        await using var handle = await lockService.TryGetAsync(key, TimeSpan.FromSeconds(5), ct);
        if (handle is null)
        {
            return false;
        }

        // 持锁执行互斥段
        return true;
    }
}
```

注意：上述"同 key 反复获取"的常规去重形态在 `LocalLock` 现状下不可用——同一 key 第二次进入会抛 `ObjectDisposedException`（见隐式行为第 3 条）。需要重复获取同一 key 时，使用 `Cike.Locks.DistributedRedis` 实现或自带实现替换注册（见边界与反模式）。

### 配置

```csharp
using Cike.Core.Modularity;
using Cike.Locks.Options;

public class MyAppModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<LockOptions>(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(10); // TryGet/TryGetAsync 未传 timeout 时的等待上限
        });
        return Task.CompletedTask;
    }
}
```

- 也可绑定配置节：`context.Services.Configure<LockOptions>(context.Services.GetConfiguration().GetSection("Lock"))`。
- `LockOptions` 同时被 `LocalLock` 与 `DistributedRedisLock` 消费（后者读同一 `DefaultTimeout`）。
- 框架不设默认值；未配置 = `TimeSpan.Zero`（不等待）。

### 边界与反模式

- **不要对会重复进入的 key 使用 `LocalLock`**：一次性缺陷（隐式行为第 3 条）。修复方式是自带正确语义的实现并替换注册——释放用 `Release()` 而非 `Dispose()`：

```csharp
using System.Collections.Concurrent;
using Cike.Core;
using Cike.Core.DependencyInjection;
using Cike.Locks.Abstracts;
using Cike.Locks.Options;
using Microsoft.Extensions.Options;

[Dependency(ReplaceServices = true)] // 替换已有 ILock 注册（应用模块晚于框架模块处理，Replace 生效）
public class FixedLocalLock(IOptionsMonitor<LockOptions> options) : ILock, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public IDisposable? TryGet(string key, TimeSpan timeout = default)
    {
        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        return semaphore.Wait(timeout == default ? options.CurrentValue.DefaultTimeout : timeout)
            ? new DisposeAction(semaphore.Release)
            : null;
    }

    public async Task<IAsyncDisposable?> TryGetAsync(string key, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        return await semaphore.WaitAsync(timeout == default ? options.CurrentValue.DefaultTimeout : timeout, cancellationToken)
            ? new DisposeAction(semaphore.Release)
            : null;
    }
}
```

- **不要传 `TimeSpan.Zero` 表达"试一次"**——会被当作未传而回退到 `DefaultTimeout`；要无限等待用 `Timeout.InfiniteTimeSpan`。
- **多实例部署不要依赖 `ILock`（LocalLock）做防重**——无跨进程语义，各实例各自持锁。
- **不要假设取消令牌会返回 `null`**——`OperationCanceledException` 会抛出。
- key 为 `null`/空白直接抛 `ArgumentNullException`，无兜底。

## Cike.Locks.DistributedRedis

### 定位

`ILock` 的 Redis 分布式实现：基于 Medallion.Threading.Redis（NuGet 包名 `DistributedLock.Redis`）+ StackExchange.Redis。多实例部署下跨进程互斥，锁状态存于 Redis。模块类 `CikeLockDistributedRedisModule` 依赖 `CikeLocksModule`（传递引入 `ILock` 契约与 `LockOptions`）。

### 能力清单

```csharp
// 命名空间：Cike.Locks.DistributedRedis
public class CikeRedisDistributedLockOptions
{
    public string? RedisConnectionString { get; set; }  // StackExchange.Redis 连接串
    public IDatabase? RedisDatabase { get; set; }       // 与连接串二选一；同时设置时 RedisDatabase 优先
}

// 命名空间：Cike.Locks.Distributed（注意：模块类不在 Cike.Locks.DistributedRedis 命名空间下）
[DependsOn([typeof(CikeLocksModule)])]
public class CikeLockDistributedRedisModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context);
    // 注册 AddSingleton<IDistributedLockProvider>（工厂延迟创建 RedisDistributedSynchronizationProvider）
}
```

| 组件 | 说明 |
|---|---|
| `DistributedRedisLock` | internal（`Cike.Locks.DistributedRedis.Internals`），`ILock + ISingletonDependency` → 自动注册为 Singleton。`TryGet` → `distributedLockProvider.CreateLock(key).TryAcquire(timeout)`；`TryGetAsync` → `TryAcquireAsync(timeout, ct)`。未拿到返回 `null`，句柄 Dispose 即释放（Medallion 语义） |
| `IDistributedLockProvider` | 注册为 Singleton 的 Medallion `RedisDistributedSynchronizationProvider`；可单独注入以使用 Medallion 完整 API（如带 options 的 `CreateLock`） |

`CreateLock(key)` 未传任何 options：锁的自动过期、续期等语义完全由 Medallion 默认策略决定，框架未做定制。

### 隐式行为

1. **【陷阱】两实现共存时后注册者胜，DistributedRedis 胜出**。机制：接口注册一律 `services.Add`（追加），MS DI 解析取最后一条；模块按后序遍历加载（依赖在前）——`CikeLocksModule` 先处理（注册 `LocalLock` → `ILock`），`CikeLockDistributedRedisModule` 后处理（注册 `DistributedRedisLock` → `ILock`，追加在后）。因此同时依赖两个包时 `ILock` 解析到 **`DistributedRedisLock`**（机制详见[框架内核](./framework-core.md)的自动 DI 与模块加载）。
2. **配置缺失延迟抛出（非模块初始化时）**：`IDistributedLockProvider` 以工厂单例注册，`ApplicationException("Please use Config<CikeRedisDistributedLockOptions>")` 在**首次解析 `ILock`（即首次构建 `DistributedRedisLock`）时**才抛出，启动阶段无任何报错。抛出条件：`IOptionsMonitor<CikeRedisDistributedLockOptions>` 不可解析、或 `CurrentValue` 为 null、或 `RedisConnectionString` 为 null/空串且 `RedisDatabase` 为 null。
3. **连接懒创建且独立**：`ConnectionMultiplexer.Connect(RedisConnectionString)` 在首次解析时执行，仅创建一次；不复用 [缓存](./caching.md) 的连接（锁不读缓存模块的 `RedisConfig` 配置节，也不共享其 `IConnectionMultiplexer`）。`RedisDatabase` 非空时优先使用，跳过自建连接。
4. **timeout 回退规则与 `LocalLock` 相同**：`timeout == default`（含显式 `TimeSpan.Zero`）→ `LockOptions.DefaultTimeout`（未配置 = `TimeSpan.Zero`，单次尝试不等待）；无限等待传 `Timeout.InfiniteTimeSpan`。
5. **key 校验 / 取消行为**：`null` 或空白 key 抛 `ArgumentNullException`；`cancellationToken` 触发抛 `OperationCanceledException`；超时返回 `null`。
6. **锁以 key 为名存于目标 Redis database**：与业务缓存 key 共存于同一 database（若共用实例），注意 key 命名前缀避免碰撞。
7. `DistributedRedisLock` 同时也把自身注册到 `ILock` 之外的接口（`ISingletonDependency` 等标记接口），对业务无意义，忽略即可。

### 示例

模块依赖与配置（应用模块中）：

```csharp
using Cike.Core.Modularity;
using Cike.Locks.Distributed;      // 注意：CikeLockDistributedRedisModule 在此命名空间
using Cike.Locks.DistributedRedis; // CikeRedisDistributedLockOptions
using Microsoft.Extensions.DependencyInjection;

[DependsOn([typeof(CikeLockDistributedRedisModule)])] // 传递依赖 CikeLocksModule
public class MyAppModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<CikeRedisDistributedLockOptions>(options =>
        {
            options.RedisConnectionString = "127.0.0.1:6379";
            // 或复用已有连接：options.RedisDatabase = multiplexer.GetDatabase();
        });
        return Task.CompletedTask;
    }
}
```

用法（与 `ILock` 契约一致，跨实例生效）：

```csharp
using Cike.Locks.Abstracts; // ILock

public class StockWorker(ILock distributedLock)
{
    public async Task<bool> DeductAsync(string sku, CancellationToken ct)
    {
        await using var handle = await distributedLock.TryGetAsync($"lock:stock:{sku}", TimeSpan.FromSeconds(3), ct);
        if (handle is null)
        {
            return false; // 未拿到——另一实例正在扣减
        }

        // 互斥段：跨实例互斥由 Redis 保证
        return true;
    }

    public bool Deduct(string sku)
    {
        using var handle = distributedLock.TryGet($"lock:stock:{sku}", TimeSpan.FromSeconds(3));
        if (handle is null)
        {
            return false;
        }

        return true;
    }
}
```

### 配置

| 配置项 | 必填 | 说明 |
|---|---|---|
| `CikeRedisDistributedLockOptions.RedisConnectionString` | 与 `RedisDatabase` 二选一 | StackExchange.Redis 连接串；首次解析时用于 `ConnectionMultiplexer.Connect` |
| `CikeRedisDistributedLockOptions.RedisDatabase` | 与 `RedisConnectionString` 二选一 | 直接给 `IDatabase`；同时设置时优先于连接串 |
| `LockOptions.DefaultTimeout` | 可选 | 未传 timeout 时的等待上限，两实现共用 |

- **必须**通过 `context.Services.Configure<CikeRedisDistributedLockOptions>(...)`（或自行绑定配置节）提供连接信息——模块**不会**从 `IConfiguration` 自动读取任何配置节（这点与[缓存](./caching.md)的 `RedisConfig` 自动绑定不同）。
- 未配置的后果：启动正常，首次解析 `ILock` / `IDistributedLockProvider` 时抛 `ApplicationException`。

### 边界与反模式

- **【陷阱】只想用本地锁却同时引用了本包**：`ILock` 会静默解析到 `DistributedRedisLock`，未配连接串则首次使用即抛异常。强制使用本地锁的三种方式：
  1. 模块图只含 `Cike.Locks`（移除对 `CikeLockDistributedRedisModule` 的依赖）；
  2. 自定义 `ILock` 实现 + `[Dependency(ReplaceServices = true)]`（如上文 `FixedLocalLock`）；
  3. 自定义实现 + `[Dependency(Key = "local-lock")]` keyed 注册，消费方用 `[FromKeyedServices("local-lock")] ILock` 或 `GetRequiredKeyedService<ILock>("local-lock")` 解析。
  - `LocalLock` / `DistributedRedisLock` 均为 internal，无法直接在其上加 attribute，"替换"指提供自己的实现类。
- **历史项目注意**：旧包 `Cike.Locks.Distributed`（已移除出解决方案）行为相反——`ILock` 默认解析到 `LocalLock`，分布式锁静默不生效。遇到旧代码先确认引用的是哪个包。
- **不要假设缺配置会在启动时失败**：异常延迟到首次解析（隐式行为第 2 条），上线后首个用到锁的请求才 500。
- **长任务持锁**：过期/续期语义由 Medallion 默认策略决定（`CreateLock` 未传 options），持锁时长接近锁默认生命周期前自行验证或直接注入 `IDistributedLockProvider` 定制。
- **共用 Redis 实例**：锁连接独立于缓存连接（隐式行为第 3 条），评估连接数与容量时按两份计算。

# 缓存

## 域定位

解决三类问题：①多实例共享缓存状态（分布式缓存）；②热点数据进程内低延迟读取（内存缓存）；③多实例部署下各实例内存缓存的一致性（跨实例失效）。

两级缓存模型：`IMultilevelCacheClient` = **L1 内存（`IMemoryCache`，进程内）+ L2 Redis（分布式）**。

```
GetAsync<T>(key)
  │  key 格式化（默认 TypeName：实际 key = "{T 的简单类型名}.{key}"）
  ├─ L1 命中 → 直接返回（零网络开销；滑动过期由 MemoryCache 续期）
  └─ L1 未命中 → 查 L2 Redis（HGET absexp sldexp data）
        ├─ 命中 → 反序列化 → 回填 L1（按 L1 自己的 TTL）→ 返回
        │        同时按存储的过期元数据重设 L2 TTL（滑动过期 = 读后重设窗口）
        └─ 未命中 → Get/GetList：返回 null（null 也回填 L1）
                    GetOrSet*：执行 setter → 写 L2 + 回填 L1 + PubSub 广播新值

SetAsync：写 L2 → 写 L1 → PubSub 广播 Set+新值（已订阅实例把各自 L1 更新为新值）
RemoveAsync：删 L2 → PubSub 广播 Remove（已订阅实例逐出各自 L1 并退订）→ 删本机 L1
```

L1 与 L2 的 TTL **相互独立**；写操作通过 Redis PubSub 同步其他实例（应用层实现，不需要 Redis keyspace notifications）。适合读多写少、可短暂容忍跨实例不一致的数据——**需要强一致（读到必须最新）的场景不要用两级缓存**（PubSub 尽力而为、无确认无重放），改用纯 Redis 客户端或放弃缓存。

## 覆盖的包

| 包 | 模块类 | 内容 |
|---|---|---|
| Cike.Caching（`src/Cike.Caching`，net8.0） | `CikeCachingModule` | 基于 StackExchange.Redis 2.8.16 的分布式缓存 + 内存/Redis 两级缓存、发布订阅、key 格式化、类型别名（内部改造自 MASA Stack，`MultilevelCachePublish<T>` 等仍保留 `Masa.Contrib.Caching.MultilevelCache` 命名空间） |

## Cike.Caching

### 定位

基于 StackExchange.Redis 的缓存客户端库：分布式缓存（`IDistributedCacheClient` → `RedisCacheClient`）、两级缓存（`IMultilevelCacheClient` → `MultilevelCacheClient`，业务默认选它）、Redis 发布订阅、缓存 key 格式化与类型别名机制。

**什么时候不用**：需要强一致读的场景（两级缓存的 L1 可能短暂持有旧值，跨实例失效依赖 PubSub 广播，丢失时旧值存活到自身 TTL 到期）；需要分布式锁时用[锁](./locks.md)（不要用缓存模拟）；业务进程间事件通知用事件总线（见[事件与 CQRS](./events-cqrs.md)），本包 PubSub 仅适合轻量级缓存/通知场景。

两个实现类均标记 `ISingletonDependency`（自动 DI 见[框架内核](./framework-core.md)，接口注册一律 Singleton）；`ConnectionMultiplexer` 在 `RedisCacheClient` 构造时同步创建（`AbortOnConnectFail` 默认 false，Redis 不可达时首连接不抛异常，后台重连）。

### 能力清单

**DI 入口**（两个客户端均为 Singleton）：

| 抽象 | 默认实现 | 用途 |
|---|---|---|
| `IMultilevelCacheClient : ICacheClient` | `MultilevelCacheClient` | L1+L2 两级缓存，业务默认 |
| `IDistributedCacheClient : ICacheClient, IDisposable` | `RedisCacheClient` | 纯 Redis（无内存层）；额外含 Exists、模糊 key、KeyExpire、计数器、PubSub |
| `ICacheClient` | ——（仅公共契约） | **不要直接注入**：两个实现都会注册到该接口，解析结果取决于注册顺序 |

**辅助类型**：

| 类型 | 成员 / 说明 |
|---|---|
| `CacheEntry<T>` | `T? Value`；构造器 `(T? value)` / `(T? value, DateTimeOffset absoluteExpiration)` / `(T? value, TimeSpan absoluteExpirationRelativeToNow)`。setter 的返回载体 |
| `CacheEntryOptions` | `AbsoluteExpiration`、`AbsoluteExpirationRelativeToNow`、`SlidingExpiration`（后两者并存时 RelativeToNow 优先；setter 拒绝 `<= TimeSpan.Zero`；全空 = 永久有效） |
| `CacheOptions` | `CacheKeyType? CacheKeyType`——单次调用覆盖 key 格式化 |
| `MultilevelCacheOptions : CacheOptions` | + `CacheEntryOptions? MemoryCacheEntryOptions`——单次调用覆盖 L1 TTL |
| `CombinedCacheEntryOptions` | `MemoryCacheEntryOptions` + `DistributedCacheEntryOptions`——一次 Set 分别指定两级 TTL |
| `CombinedCacheEntry<T>` | `DistributedCacheEntryFunc` / `DistributedCacheEntryAsyncFunc`（仅异步方法使用；两者同设时同步版优先）/ `MemoryCacheEntryOptionsAction` |
| `PublishOptions : PubSubOptionsBase` | + `object? Value` |
| `SubscribeOptions<T> : PubSubOptionsBase` | + `T? Value`、`bool IsPublisherClient`（消息是否本客户端发出） |
| `PubSubOptionsBase` | `SubscribeOperation Operation`（Set/Remove）、`string Key`、`Guid UniquelyIdentifies` |
| `IFormatCacheKeyProvider` | key 格式化扩展点（替换默认实现需注册为 Singleton） |
| `ITypeAliasProvider` | 类型别名扩展点：`string GetAliasName(string typeName)` |

枚举：`CacheKeyType { None = 1, TypeName, TypeAlias }`；`SubscribeKeyType { ValueTypeFullName = 1, ValueTypeFullNameAndKey = 2, SpecificPrefix = 3 }`；`SubscribeOperation { Set = 1, Remove = 2 }`。

**`ICacheClient`（两级客户端公共契约）**：

```csharp
IEnumerable<T?> GetList<T>(params string[] keys);
Task<IEnumerable<T?>> GetListAsync<T>(params string[] keys);

// 单写：过期参数三种形态（DateTimeOffset? / TimeSpan? / CacheEntryOptions），同步/异步 ×3
void Set<T>(string key, T value, CacheEntryOptions? options = null, Action<CacheOptions>? action = null);
Task SetAsync<T>(string key, T value, CacheEntryOptions? options = null, Action<CacheOptions>? action = null);

// 批量写：Dictionary + 同上三种过期形态 × 同步/异步
void SetList<T>(Dictionary<string, T?> keyValues, CacheEntryOptions? options = null, Action<CacheOptions>? action = null);
Task SetListAsync<T>(Dictionary<string, T?> keyValues, CacheEntryOptions? options = null, Action<CacheOptions>? action = null);

void Remove<T>(string key, Action<CacheOptions>? action = null);
void Remove<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null);
Task RemoveAsync<T>(string key, Action<CacheOptions>? action = null);      // + IEnumerable 重载
void Refresh<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null);
Task RefreshAsync<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null);
```

**`IMultilevelCacheClient` 增量签名**：

```csharp
// 读（含变更回调变体；同步 Get 为阻塞实现）
T? Get<T>(string key, Action<MultilevelCacheOptions>? action = null);
T? Get<T>(string key, Action<T?> valueChanged, Action<MultilevelCacheOptions>? action = null);
Task<T?> GetAsync<T>(string key, Action<MultilevelCacheOptions>? action = null);
Task<T?> GetAsync<T>(string key, Action<T?> valueChanged, Action<MultilevelCacheOptions>? action = null);
IEnumerable<T?> GetList<T>(IEnumerable<string> keys, Action<MultilevelCacheOptions>? action = null);
Task<IEnumerable<T?>> GetListAsync<T>(IEnumerable<string> keys, Action<MultilevelCacheOptions>? action = null);

// GetOrSet：L1、L2 均未命中才执行 setter；memoryCacheEntryOptionsAction 单次覆盖 L1 TTL
T? GetOrSet<T>(string key, Func<CacheEntry<T>> distributedCacheEntryFunc,
    Action<CacheEntryOptions>? memoryCacheEntryOptionsAction = null, Action<CacheOptions>? action = null);
T? GetOrSet<T>(string key, CombinedCacheEntry<T> combinedCacheEntry, Action<CacheOptions>? action = null);
Task<T?> GetOrSetAsync<T>(string key, Func<CacheEntry<T>> distributedCacheEntryFunc,
    Action<CacheEntryOptions>? memoryCacheEntryOptionsAction = null, Action<CacheOptions>? action = null);
Task<T?> GetOrSetAsync<T>(string key, Func<Task<CacheEntry<T>>> distributedCacheEntryFunc,
    Action<CacheEntryOptions>? memoryCacheEntryOptionsAction = null, Action<CacheOptions>? action = null);
Task<T?> GetOrSetAsync<T>(string key, CombinedCacheEntry<T> combinedCacheEntry, Action<CacheOptions>? action = null);

// 写：两级 TTL 分别指定；单 options 走 ICacheClient 基类重载（同一份 options 同用于两级）
void Set<T>(string key, T value, CacheEntryOptions? distributedOptions, CacheEntryOptions? memoryOptions, Action<CacheOptions>? action = null);
void Set<T>(string key, T value, CombinedCacheEntryOptions? options, Action<CacheOptions>? action = null);
Task SetAsync<T>(string key, T value, CacheEntryOptions? distributedOptions, CacheEntryOptions? memoryOptions, Action<CacheOptions>? action = null);
Task SetAsync<T>(string key, T value, CombinedCacheEntryOptions? options, Action<CacheOptions>? action = null);
void SetList<T>(Dictionary<string, T?> keyValues, CacheEntryOptions? distributedOptions, CacheEntryOptions? memoryOptions, Action<CacheOptions>? action = null);
Task SetListAsync<T>(Dictionary<string, T?> keyValues, CombinedCacheEntryOptions? options, Action<CacheOptions>? action = null);   // + 两级分列重载

// 删 / 续期：注意是 params 且【无 action 重载】，key 类型恒用全局配置
void Remove<T>(params string[] keys);
Task RemoveAsync<T>(params string[] keys);
void Refresh<T>(params string[] keys);
Task RefreshAsync<T>(params string[] keys);
```

**`IDistributedCacheClient` 增量签名**：

```csharp
T? Get<T>(string key, Action<CacheOptions>? action = null);
Task<T?> GetAsync<T>(string key, Action<CacheOptions>? action = null);
IEnumerable<T?> GetList<T>(IEnumerable<string> keys, Action<CacheOptions>? action = null);   // + Async
T? GetOrSet<T>(string key, Func<CacheEntry<T>> setter, Action<CacheOptions>? action = null);
Task<T?> GetOrSetAsync<T>(string key, Func<CacheEntry<T>> setter, Action<CacheOptions>? action = null);
Task<T?> GetOrSetAsync<T>(string key, Func<Task<CacheEntry<T>>> setter, Action<CacheOptions>? action = null);

// 存在性：非泛型 = 原始 key（不格式化）；泛型 = 格式化
bool Exists(string key);
bool Exists<T>(string key, Action<CacheOptions>? action = null);
Task<bool> ExistsAsync(string key);                    // + 泛型/Async 变体

// 模糊 key（Redis KEYS 命令，O(N) 阻塞）：非泛型 = 原始 pattern；泛型 = 格式化为 "{类型前缀}{pattern}"
IEnumerable<string> GetKeys(string keyPattern);
IEnumerable<string> GetKeys<T>(string keyPattern, Action<CacheOptions>? action = null);   // + Async
IEnumerable<KeyValuePair<string, T?>> GetByKeyPattern<T>(string keyPattern, Action<CacheOptions>? action = null);   // + Async，取 key+值

// 改 TTL：单 key（原始 / 泛型格式化）、批量（原始 / 泛型），返回是否成功 / 成功数
bool KeyExpire(string key, CacheEntryOptions? options = null);
bool KeyExpire<T>(string key, CacheEntryOptions? options = null, Action<CacheOptions>? action = null);
long KeyExpire<T>(IEnumerable<string> keys, CacheEntryOptions? options = null, Action<CacheOptions>? action = null);
// 另有 TimeSpan? / DateTimeOffset 直接过期参数变体 × 同步/异步，共 16 个重载；key 不存在返回 false/不计入

// 原始 key 的删/续期（非泛型，不格式化）
void Remove(params string[] keys);  Task RemoveAsync(params string[] keys);
void Refresh(params string[] keys); Task RefreshAsync(params string[] keys);

// 计数器（限流/配额）：key 按 T=long 格式化（前缀 Int64），操作 hash 的 data 字段，Lua 原子
Task<long> HashIncrementAsync(string key, long value = 1, Action<CacheOptions>? action = null, CacheEntryOptions? options = null);
Task<long?> HashDecrementAsync(string key, long value = 1, long defaultMinVal = 0, Action<CacheOptions>? action = null, CacheEntryOptions? options = null);
// Decrement：当前值 <= defaultMinVal 时返回 null（不减）

// 发布订阅（channel 原样使用，不做 key 格式化）
void Publish(string channel, Action<PublishOptions> options);   Task PublishAsync(...);
void Subscribe<T>(string channel, Action<SubscribeOptions<T>> options);   Task SubscribeAsync<T>(...);
void UnSubscribe<T>(string channel);   Task UnSubscribeAsync<T>(string channel);
```

### 隐式行为

#### key 格式化（最大的隐式行为）

**默认实际 key 不是你传入的字符串**。由 `CacheKeyType` 决定（multilevel 用 `MultilevelCache:GlobalCacheOptions:CacheKeyType`，distributed 直接用 `RedisConfig:GlobalCacheOptions:CacheKeyType`，两者独立、默认值相同）：

| 值 | 实际 key | 说明 |
|---|---|---|
| `TypeName`（默认） | `{类型名}.{key}` | `typeof(T).Name`（**不含命名空间**，分隔符是 `.`）；泛型为 `List`1[User]`，`Dictionary<,>` 特殊取第二个泛型参数（`Dictionary`2[User]`）。不同类型同 key 互不冲突 |
| `None` | `{key}` | 原样使用 |
| `TypeAlias` | `{类型别名}:{key}` | 别名由 `ITypeAliasProvider` 提供，查不到抛异常 |

- 【陷阱】**Redis 里排查 key / 手动删 key 时注意**：默认 `TypeName` 模式下所有 key 都带类型名前缀且以 `.` 分隔（如 `UserDto.user:1`）；`HashIncrementAsync` 的 key 前缀是 `Int64`。非泛型 API（`Exists(string)`、`GetKeys(string)`、`KeyExpire(string,...)`、`Remove(params string[])`、`Refresh(params string[])`）不做格式化，传参前需自己拼好前缀。
- 单次调用覆盖：`action => options.CacheKeyType = ...`。【陷阱】**传了 action 但未设置 CacheKeyType 时**——multilevel 各方法回落到 `TypeName`（而非全局配置值，全局配了 `None`/`TypeAlias` 会被悄悄改回 `TypeName`）；distributed 各方法直接抛 `InvalidOperationException`（`CacheKeyType` 为 null）。传 action 就必须显式设置 `CacheKeyType`。
- 【陷阱】**`TypeAlias` 模式默认不可用**：`CikeCachingModule` 把 `TypeAliasOptions.GetAllTypeAliasFunc` 配成**返回空字典**，`DefaultTypeAliasProvider.GetAliasName` 找不到别名即抛 `ArgumentNullException`。用 `TypeAlias` 必须先在代码中替换该委托（别名表按**简单类型名**（`typeof(T).Name`，与 `TypeName` 模式同名）为键），并注意按 `RefreshTypeAliasInterval`（默认 30s）周期刷新。
- **InstanceId 前缀**：`RedisConfig:InstanceId` 非空时，所有 L2 key 再加 `{InstanceId}:` 前缀；`MultilevelCache:InstanceId` 非空时，L1 key（及传给 L2 的 key）加该前缀。两个前缀独立配置、可叠加。
- multilevel 内部调 distributed 时传已格式化 key + `CacheKeyType.None`，**不会双重格式化**；因此 `RedisConfig:GlobalCacheOptions:CacheKeyType` 只影响直接使用 `IDistributedCacheClient` 的场景。

#### L2 存储与序列化

- 每个 key 是一个 Redis Hash：`absexp`（绝对过期 ticks，-1=无）、`sldexp`（滑动过期 ticks）、`data`（值）。**redis-cli 里 data 字段不可直接读**：数值类型原样存储；`string` 存 GZip(UTF-8)；其他类型存 GZip(JSON)（System.Text.Json，启用 dynamic types 转换器）。
- 每次 L2 读命中会按存储的过期元数据**重设 TTL**（滑动过期的 Redis 实现方式）；`Refresh` 同理——滑动 = 重设整个窗口，绝对 = 重设剩余时间。
- `RedisConfig` 节本身继承 `CacheEntryOptions`，其 `AbsoluteExpiration` / `AbsoluteExpirationRelativeToNow` / `SlidingExpiration` 是 **L2 全局默认 TTL**（调用未传 options 的写入时生效）。

#### L1/L2 TTL 相互独立

- L2 TTL：每次写入传入的 `CacheEntryOptions`，未传用 `RedisConfig` 全局默认。
- L1 TTL：全局 `MultilevelCache:CacheEntryOptions`；或单次覆盖——`GetOrSet*` 的 `memoryCacheEntryOptionsAction`、`Get*` 的 `action.MemoryCacheEntryOptions`、`Set*` 的 `memoryOptions`。两级都未配置 → L1 条目**永不过期**（直到进程重启或 Remove）。
- `ICacheClient` 的单 `options` Set/SetList 重载：**同一份 options 同用于 L1 和 L2**。

#### 跨实例失效（两级缓存一致性）

- `Set/SetList` → 写 L2 + 本机 L1 + PubSub 广播 **Set + 新值**：已订阅该 channel 的其他实例把新值写入自己 L1（是**更新为新值**，不是失效），并触发各自的 `valueChanged` 回调。
- `GetOrSet*` → 仅当 setter 真正执行（L1、L2 均未命中）时才广播 Set+新值；L2 命中回填 L1 不广播。
- `Remove` → 删 L2 + 广播 **Remove**：订阅实例逐出各自 L1 并退订 channel；本机 L1 最后删除。
- `Get/GetList` 在 **L1 未命中回填时**订阅该 key 的 channel（幂等：每实例每 channel 一次）；L1 命中的 Get 不订阅、不触网络。
- channel 命名由 `MultilevelCache:SubscribeKeyType` 决定：`ValueTypeFullNameAndKey`（默认）= `[类型全名]key`（**每个 (类型,key) 一条订阅**，key 多则订阅多）；`ValueTypeFullName` = 按类型一条 channel；`SpecificPrefix` = `{SubscribeKeyPrefix}{key}`。注意 channel 用的是 `FullName`，与缓存 key 前缀的简单类型名不同。
- 这是**应用层 PubSub，不需要**配置 Redis keyspace notifications（`notify-keyspace-events`）。
- 【陷阱】**广播丢失时（Redis 断连/重连窗口、订阅实例离线）L1 旧值存活到自身 TTL 到期**——对一致性敏感的数据把 L1 TTL 配短，或不用 multilevel。
- 【陷阱】**`valueChanged` 回调只在首次为该 (T,key) 建立订阅的 Get 上生效**：若本实例此前已用无回调的 Get 订阅过同一 channel，后续带回调的 Get 不会补挂回调（内部按 channel 去重直接返回）。该 key 被 Remove（本机或远端）退订后，下一次 Get 才会带新回调重新订阅。
- `valueChanged` 在 Redis 订阅线程触发（非请求上下文，无 HttpContext）；本实例自己 Set 同一 key 时，若此前已订阅该 channel，也会收到自己发的广播。

#### 其他

- Redis 连接断开期间所有操作抛 `NotSupportedException("Redis service has been disconnected...")`；`AbortOnConnectFail` 默认 false，连接在后台自动重试。
- `Set` 的 value 为 null 抛 `ArgumentNullException`。`GetOrSet` setter 返回 null 值：L2 不写、**L1 会缓存 null**（L1 负缓存，下次 Get 直接命中 null 直到 L1 TTL 到期）；`Get` 未命中回填 L1 同样会缓存 null。
- 同步方法（`Get`/`GetOrSet` 等非 Async 版本）是 sync-over-async 阻塞实现（已 `ConfigureAwait(false)`，无死锁风险但占线程），统一用异步版本。
- `GetKeys`/`GetByKeyPattern` 底层是 Redis **KEYS 命令**（O(N) 阻塞主线程），框架未封装 SCAN。
- `MultilevelCacheGlobalOptions` 继承 `MemoryCacheOptions`，但实际注入的 `IMemoryCache` 来自 `AddMemoryCache()`（独立的标准 `MemoryCacheOptions`）——`MultilevelCache` 节里 `SizeLimit` 等 MemoryCacheOptions 属性**不会生效**。
- `MultilevelCacheClient` 跨实例广播的消息体 `MultilevelCachePublish<T>` 位于命名空间 `Masa.Contrib.Caching.MultilevelCache`（历史来源，勿引用业务代码）。

### 示例

```csharp
using Cike.Caching;
using Cike.Caching.Options;

public class UserCacheService(IMultilevelCacheClient cache)
{
    // 1) GetOrSetAsync：最常用形态（读穿透保护 + 两级 TTL 各自指定）
    public async Task<UserDto?> GetUserAsync(long id)
    {
        return await cache.GetOrSetAsync(
            $"user:{id}",                                            // 实际 key: UserDto.user:{id}
            async () => new CacheEntry<UserDto?>(                   // L1、L2 均未命中才执行
                await LoadUserAsync(id),                            // 数据加载（查库等）
                TimeSpan.FromMinutes(30)),                          // L2 TTL
            memory => memory.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));  // L1 TTL
    }

    // 2) Set / Remove：写后删，两级 TTL 分别指定
    public async Task UpdateUserAsync(UserDto user)
    {
        await cache.SetAsync($"user:{user.Id}", user,
            distributedOptions: new CacheEntryOptions(TimeSpan.FromHours(2)),   // L2
            memoryOptions: new CacheEntryOptions(TimeSpan.FromMinutes(5)));     // L1
        // SetAsync 内部已广播，其他实例的 L1 会被更新为新值
    }

    public async Task InvalidateUserAsync(long id)
        => await cache.RemoveAsync<UserDto>($"user:{id}");   // 泛型参数参与 key 格式化，必须与写入时同类型

    // 3) 跨实例失效感知：valueChanged 回调（其他实例 Set/Remove 该 key 时触发）
    public async Task<UserDto?> WatchUserAsync(long id)
    {
        return await cache.GetAsync($"user:{id}", valueChanged: newValue =>
        {
            // newValue = 远端写入的新值（Remove 时为 default(UserDto)）
            // 运行在 Redis 订阅线程：只做轻量处理，不要阻塞、不要依赖 HttpContext
        });
    }

    private static Task<UserDto?> LoadUserAsync(long id)
        => Task.FromResult(new UserDto { Id = id, Name = "demo" });
}

public record UserDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
```

纯 Redis 客户端独有能力（模糊 key、计数器限流、TTL 调整、PubSub）：

```csharp
using Cike.Caching;
using Cike.Caching.Options;

IDistributedCacheClient redis = provider.GetRequiredService<IDistributedCacheClient>();

// 计数限流：key 实际为 Int64.rate:{userId}，60s 窗口
long hits = await redis.HashIncrementAsync($"rate:{userId}", value: 1,
    options: new CacheEntryOptions(TimeSpan.FromSeconds(60)));

// 存在性 / 模糊 key / 改 TTL
bool exists = redis.Exists<UserDto>("user:1");          // 检查 UserDto.user:1（格式化）
bool raw     = redis.Exists("UserDto.user:1");          // 原始 key（不格式化）
var keys     = redis.GetKeys<UserDto>("user:*");        // KEYS UserDto.user:*（生产大库慎用）
redis.KeyExpire<UserDto>("user:1", TimeSpan.FromHours(1));

// 轻量发布订阅（channel 不做 key 格式化）
await redis.PublishAsync("order.paid", o => o.Value = new OrderPaidEvent { OrderId = 42 });
redis.Subscribe<OrderPaidEvent>("order.paid", o => Console.WriteLine($"paid: {o.Value?.OrderId}, self={o.IsPublisherClient}"));
```

### 配置

模块类 `CikeCachingModule`（无其他 Cike 模块依赖）：`AddMemoryCache()`、绑定 `RedisConfig` 与 `MultilevelCache` 配置节、初始化 TypeAlias 空实现。节名固定，不可改。

```jsonc
{
  "RedisConfig": {                              // L2 分布式层（RedisConfigurationOptions）
    "InstanceId": "",                           // 非空则所有 L2 key 加 "{InstanceId}:" 前缀
    "Servers": [ { "Host": "127.0.0.1", "Port": 6379 } ],   // 空 → 默认 localhost:6379
    "DefaultDatabase": 0,
    "Password": "",
    "AbortOnConnectFail": false,                // false = 断连不抛异常，后台重试（默认）
    "AllowAdmin": false,
    "ConnectTimeout": 5000,                     // ms
    "ConnectRetry": 3,
    "SyncTimeout": 5000,                        // ms
    "AsyncTimeout": 5000,                       // ms
    "ClientName": "",
    "ChannelPrefix": "",
    "Ssl": false,
    "Proxy": "None",                            // StackExchange.Redis.Proxy: None / Twemproxy
    "GlobalCacheOptions": { "CacheKeyType": "TypeName" },   // 纯 Redis 客户端的 key 格式化
    "AbsoluteExpiration": null,                 // ↓ 三项继承自 CacheEntryOptions：L2 全局默认 TTL
    "AbsoluteExpirationRelativeToNow": null,
    "SlidingExpiration": null
  },
  "MultilevelCache": {                          // 两级缓存调优（MultilevelCacheGlobalOptions）
    "GlobalCacheOptions": { "CacheKeyType": "TypeName" },   // key 格式化（L1/L2 共用）
    "CacheEntryOptions": { "AbsoluteExpirationRelativeToNow": "00:30:00" },  // L1 默认 TTL
    "SubscribeKeyType": "ValueTypeFullNameAndKey",          // 跨实例失效 channel 粒度
    "SubscribeKeyPrefix": "",                   // SpecificPrefix 模式的前缀
    "InstanceId": null                          // 非空则 L1 key 加前缀
  }
}
```

| 配置类 | 节名 | 属性全集 |
|---|---|---|
| `RedisConfigurationOptions : CacheEntryOptions` | `RedisConfig` | `InstanceId`、`Servers`（`RedisServerOptions: Host/Port`，缺省 localhost:6379）、`DefaultDatabase`=0、`Password`、`AbortOnConnectFail`=false、`AllowAdmin`=false、`ConnectTimeout`=5000、`ConnectRetry`=3、`SyncTimeout`=5000、`AsyncTimeout`=5000、`ClientName`、`ChannelPrefix`、`Ssl`=false、`Proxy`=None、`GlobalCacheOptions.CacheKeyType`=TypeName、继承的 `AbsoluteExpiration`/`AbsoluteExpirationRelativeToNow`/`SlidingExpiration`（L2 全局默认 TTL） |
| `MultilevelCacheGlobalOptions : MemoryCacheOptions` | `MultilevelCache` | `GlobalCacheOptions.CacheKeyType`=TypeName、`CacheEntryOptions`=null（L1 默认 TTL）、`SubscribeKeyType`=ValueTypeFullNameAndKey、`SubscribeKeyPrefix`=""、`InstanceId`=null；继承的 MemoryCacheOptions 属性不生效（见隐式行为） |
| `TypeAliasOptions` | 无配置节（仅代码配置） | `RefreshTypeAliasInterval`=30（秒）、`GetAllTypeAliasFunc`（默认返回空字典，用 TypeAlias 必须替换） |

`TypeAliasOptions` 替换方式（模块 `ConfigureServicesAsync` 或 `services.Configure`）：

```csharp
services.Configure<TypeAliasOptions>(o =>
{
    o.RefreshTypeAliasInterval = 60;
    o.GetAllTypeAliasFunc = () => new Dictionary<string, string>
    {
        // 键 = 简单类型名（typeof(T).Name，与 TypeName 模式同名），值 = key 前缀别名
        ["UserDto"] = "u"
        // 启用后 key 形如 "u.user:1"
    };
});
```

### 边界与反模式

- **强一致场景不用两级缓存**：跨实例失效是 PubSub 尽力而为（无确认、无重放），广播丢失时 L1 旧值存活到自身 TTL；一致性敏感数据把 L1 TTL 配短，或直接用 `IDistributedCacheClient`（每次读写直达 Redis）。
- **不要直接注入 `ICacheClient`**：两个实现都注册到该接口，解析结果取决于注册顺序；按需注入 `IMultilevelCacheClient` 或 `IDistributedCacheClient`。
- **不要在 `MultilevelCacheClient.Remove/Refresh` 上期待 action 重载**：它们是 `params string[]` 且无 action，key 类型恒用全局配置；需要按 `None` 精确删 key 时用基类 `Remove<T>(key, action => action.CacheKeyType = CacheKeyType.None)` 或 distributed 的非泛型 `Remove`（原始 key）。
- **传 action 就必须设置 `CacheKeyType`**：multilevel 不设会回落 `TypeName`（覆盖全局 `None`/`TypeAlias` 配置），distributed 不设直接抛异常。
- **`TypeAlias` 不配别名表就用 = 运行时抛异常**：默认 `GetAllTypeAliasFunc` 返回空字典。
- **`GetKeys`/`GetByKeyPattern` 是 KEYS 命令**：O(N) 阻塞 Redis 主线程，生产大库禁用；框架内无 SCAN 封装。
- **`valueChanged` 回调不可靠挂载**：仅首次订阅该 channel 的 Get 生效；回调在订阅线程执行，不要做耗时/阻塞操作或依赖请求上下文。
- **L1 不配 TTL = 永不过期**：务必配置 `MultilevelCache:CacheEntryOptions` 或逐调用传 L1 TTL，否则本机缓存无界存活。
- **null 值**：`Set(key, null)` 抛 `ArgumentNullException`；表达"无值"用 `Remove`。`GetOrSet` setter 返回 null 会造成 L1 负缓存（L1 TTL 内直接返回 null，不再执行 setter）。
- **Redis 断连窗口**：所有操作抛 `NotSupportedException`，调用方需自行处理重试；不要在启动路径上依赖缓存可用性。
- **分布式锁不要用缓存模拟**（`HashIncrementAsync` 无过期续期语义），用[锁](./locks.md)。
- **同步方法为阻塞实现**（占线程池线程），统一使用 Async 版本。

# Cike.Caching

缓存客户端：基于 StackExchange.Redis 的分布式缓存与**内存 + Redis 两级缓存**，含发布订阅、缓存 key 格式化与类型别名机制。（内部改造自 MASA Stack 的缓存实现，个别类型仍保留 Masa 命名空间。）

## 两级缓存（多级缓存）概念

`IMultilevelCacheClient` = **L1 内存（`IMemoryCache`，进程内）+ L2 Redis（分布式）**，读路径：

```
Get(key)
  ├─ L1 命中 → 直接返回（零网络开销）
  └─ L1 未命中 → 查 L2 Redis
        ├─ 命中 → 回填 L1（按 L1 自己的 TTL）→ 返回
        └─ 未命中 → 返回 null（GetOrSet 则执行 setter 回填两级）
```

- L1 与 L2 的过期时间**相互独立**：L2 每次写入时传 `CacheEntryOptions`；L1 用全局 `MultilevelCache:CacheEntryOptions`，或调用时通过 `action` 单次覆盖
- 写操作（Set/Remove）会通过 Redis PubSub 广播、失效其他实例的 L1（见下"跨实例失效"）
- 适合读多写少、可短暂容忍跨实例不一致的数据

## 提供的能力

| 接口 | 定位 | 主要方法 |
|---|---|---|
| `IMultilevelCacheClient` | 两级缓存（业务默认选它） | `Get/GetAsync`（含 `valueChanged` 变更回调变体）、`GetOrSet/GetOrSetAsync`（未命中执行 setter 并回填两级，**最常用**）、`GetList/SetList`（批量）、`Set/SetAsync`、`Remove/RemoveAsync`、`Refresh`（续期） |
| `IDistributedCacheClient` | 纯 Redis（无内存层） | 上述基础能力 + `Exists`、`GetKeys(pattern)` / `GetByKeyPattern`（模糊 key）、`KeyExpire`（改 TTL）、`HashIncrement/HashDecrementAsync`（计数器/限流）、`Publish/Subscribe/UnSubscribe` |
| `ICacheClient` | 两者共同契约 | `GetList/Set/SetList/Remove/Refresh` |

`CacheEntryOptions` 支持绝对过期（`AbsoluteExpiration` / `AbsoluteExpirationRelativeToNow`）与滑动过期（`SlidingExpiration`），全空 = 永久有效。

## 缓存 key 格式化（重要隐式行为）

默认**实际 key 不是你传的字符串**：由 `MultilevelCache:GlobalCacheOptions:CacheKeyType`（`CacheKeyType` 枚举）决定：

| 值 | 实际 key | 说明 |
|---|---|---|
| `TypeName`（默认） | `{类型全名}:{key}` | 不同类型同 key 互不冲突；Redis 里看到的 key 都带类型前缀 |
| `None` | `{key}` | 原样使用 |
| `TypeAlias` | `{类型别名}:{key}` | 别名由 `ITypeAliasProvider` 提供，用于缩短 Redis key 长度 |

调用时可用 `action => options.CacheKeyType = ...` 单次覆盖。

【陷阱】`TypeAlias` 模式依赖 `TypeAliasOptions.GetAllTypeAliasFunc` 提供别名表——`CikeCachingModule` 默认把它配成**返回空字典**，不替换该委托就用 `TypeAlias` 会在解析别名时抛异常。别名表按 `RefreshTypeAliasInterval`（默认 30s）周期刷新。

## 跨实例失效（两级缓存一致性）

`Set/Remove` 时通过 Redis PubSub 向约定 channel 广播 `MultilevelCachePublish<T>`，订阅该 channel 的其他实例收到后失效各自 L1——是**应用层 PubSub，不需要**配置 Redis keyspace notifications（`notify-keyspace-events`）。相关配置（`MultilevelCacheGlobalOptions`）：

- `SubscribeKeyType`（默认 `ValueTypeFullNameAndKey`）：订阅 channel 粒度——按类型 / 按类型+key / 统一前缀
- `SubscribeKeyPrefix`：`SpecificPrefix` 模式的前缀
- `InstanceId`：实例标识
- `GetAsync(key, valueChanged)` 的 `valueChanged` 回调：所读 key 被其他实例改动时触发

## 配置

```jsonc
"RedisConfig": {                          // L2 分布式层
  "Servers": [ { "Host": "...", "Port": 6379 } ],
  "DefaultDatabase": 0,
  "Password": "..."
},
"MultilevelCache": {                      // 两级缓存调优（MultilevelCacheGlobalOptions）
  "GlobalCacheOptions": { "CacheKeyType": "TypeName" },
  "CacheEntryOptions": { "AbsoluteExpirationRelativeToNow": "00:30:00" },  // L1 默认 TTL
  "SubscribeKeyType": "ValueTypeFullNameAndKey"
}
```

## 陷阱

- key 默认带**类型全名前缀**（`TypeName`），在 Redis 里排查 key / 手动删 key 时注意
- 跨实例失效依赖 Redis PubSub，广播丢失时其他实例的 L1 旧值会存活到自身 TTL 到期——对一致性敏感的数据把 L1 TTL 配短
- 业务项目通常不直接用它，而是封装成 `ICacheService<TModel>`（见 Workflow 项目的 `*.Caching` 层）

## 模块信息

- 模块类：`CikeCachingModule`（注册 MemoryCache、绑定 `RedisConfig` / `MultilevelCache` 配置节、初始化 TypeAlias 空实现）
- 直接依赖：无

## 更多

缓存封装模式：[AI 开发指南](../../docs/AI-GUIDE.md) 第 8.3 节。

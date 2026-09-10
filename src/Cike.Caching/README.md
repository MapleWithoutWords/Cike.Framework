# Cike.Caching

缓存客户端：基于 StackExchange.Redis 的**分布式缓存**与**内存 + Redis 两级缓存**，含发布订阅与类型别名机制。

## 提供的能力

| 类型 | 说明 |
|---|---|
| `IMultilevelCacheClient` | 两级缓存：内存未命中 → Redis 取并回填内存；`Get/GetAsync`（含 `valueChanged` 订阅变体）、`GetList/GetListAsync`（批量 key）、`Set/SetAsync`、`Remove/RemoveAsync`、订阅通知 |
| `IDistributedCacheClient` | 纯 Redis 客户端（无内存层） |
| `ICacheClient` | 共同契约（GetList/Set/Remove，绝对过期/相对过期/`CacheEntryOptions`） |
| `CikeCachingModule` | 模块：注册 MemoryCache + 读取配置节 |

## 配置

```jsonc
"RedisConfig": {                          // 分布式层
  "Servers": [ { "Host": "...", "Port": 6379 } ],
  "DefaultDatabase": 0,
  "Password": "..."
},
"MultilevelCache": { }                    // 两级缓存调优（MultilevelCacheGlobalOptions）
```

## 陷阱

- 两级缓存的内存层过期策略默认与全局配置挂钩，跨实例一致性靠 Redis 订阅失效通知——**配置 Redis 为通知模式**（`notify-keyspace-events`）才能及时失效
- 业务项目通常不直接用它，而是封装成 `ICacheService<TModel>`（见 Workflow 项目的 `*.Caching` 层）

## 模块信息

- 模块类：`CikeCachingModule`
- 直接依赖：无

## 更多

缓存封装模式：[AI 开发指南](../../docs/AI-GUIDE.md) 第 8.3 节。

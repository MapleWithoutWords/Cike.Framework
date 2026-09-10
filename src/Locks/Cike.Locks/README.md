# Cike.Locks

锁抽象：`ILock` 接口 + 进程内（SemaphoreSlim）实现。分布式实现见 [Cike.Locks.DistributedRedis](../Cike.Locks.DistributedRedis/README.md)。

## 提供的能力

```csharp
public interface ILock
{
    IDisposable? TryGet(string key, TimeSpan timeout = default);                       // 拿不到返回 null
    Task<IAsyncDisposable?> TryGetAsync(string key, TimeSpan timeout = default, CancellationToken ct = default);
}
```

- `LocalLock`（internal，自动注册）：key → `SemaphoreSlim(1,1)`，进程内互斥
- `LockOptions.DefaultTimeout`：未传 timeout 时的默认等待时长

## 陷阱

【重要】`LocalLock.TryGet` 释放锁时会 **`Dispose` 掉缓存的 SemaphoreSlim**（`new DisposeAction(semaphore.Dispose)`），而 key→信号量的缓存（`LazyManualMemoryCache`）不会随之移除——**同一个 key 第二次获取锁会在 `Wait` 时抛 `ObjectDisposedException`**。当前实现只能"一把锁用一次"。使用前先验证此行为是否已修复，或自行替换注册。

## 模块信息

- 模块类：`CikeLocksModule`（无逻辑）
- 直接依赖：`Cike.Core`

## 更多

锁与模块注册顺序问题（本地/分布式实现共存时谁生效）：见 [Cike.Locks.DistributedRedis](../Cike.Locks.DistributedRedis/README.md) 的陷阱说明。

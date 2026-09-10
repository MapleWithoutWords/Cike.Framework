# Cike.UniversalId

分布式 Id 生成：雪花算法（long）与顺序 Guid。

## 提供的能力

| 类型 | 说明 |
|---|---|
| `ISnowflakeIdGenerator.NextId() : long` | 雪花 Id（Twitter 算法：41 位时间戳 + 10 位机器号 + 12 位序列） |
| `IGuidGenerator.Create() : Guid` | 顺序 Guid（`SequentialGuidGenerator`，按 `SequentialGuidType` 控制Sequential 区段），自动注册为 Transient |
| `CikeSnowflakeOptions` | `IsEnable`（由 `CikeDataEFCoreModule` 置 true） |
| `CikeSequentialGuidGeneratorOptions` | `DefaultSequentialGuidType`——MySql Provider 默认 `SequentialAsString`，SqlServer Provider 默认 `SequentialAtEnd` |

## 陷阱

【重要】`CikeUniversalIdModule` 注册雪花生成器时 **机器号硬编码为 `1`**：

```csharp
services.AddSingleton(typeof(ISnowflakeIdGenerator), new SnowflakeIdGenerator(1));
```

**同一服务多实例部署时会产生重复 Id**。多实例场景必须自行替换注册（如按实例编号/机器指纹注册不同的 machineId），或改造本模块把 machineId 做成配置。

业务层一般不需要手动调用：`CikeDbContext` 在新增 `IEntity<long>` 实体（Id 为 0）时自动生成雪花 Id，`IEntity<Guid>` 实体自动生成顺序 Guid。

## 模块信息

- 模块类：`CikeUniversalIdModule`（注册雪花生成器）
- 直接依赖：无

## 更多

Id 生成时机与实体基类：[AI 开发指南](../../docs/AI-GUIDE.md) 第 7.1、8.8 节。

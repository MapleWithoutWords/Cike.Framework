# Id 生成

## 域定位

分布式主键 Id 生成：雪花算法（`long`，趋势递增）与顺序 Guid（`Guid`，按目标数据库的排序规则连续）。替代两种常见坏方案——数据库自增（分库分表/多写难协调）与 `Guid.NewGuid()`（随机 v4 导致聚集索引碎片）。

**核心定位：业务层一般不需要手动调用，`CikeDbContext` 自动生成。** 实体被跟踪为 `Added` 时自动赋 Id：`IEntity<long>` 且 `Id == 0` → 雪花 Id；`IEntity<Guid>` 且 `Id == Guid.Empty` → 顺序 Guid。业务代码只管 `new` 实体 + `InsertAsync`，不要手工赋 Id。赋值发生在 `AddAsync` 跟踪瞬间（早于 `SaveChanges`），时机与实体基类链见[数据访问](./data-access.md)。

**多实例部署陷阱（必读）**：默认雪花注册的机器号硬编码为 `1`，同一服务多实例部署会产生重复 Id，必须替换注册（见"边界与反模式"）。

## 覆盖的包

| 包 | 目录 | 模块类 | 说明 |
|---|---|---|---|
| Cike.UniversalId | src/Cike.UniversalId | `CikeUniversalIdModule` | 雪花 Id + 顺序 Guid。模块无 `[DependsOn]`（程序集仅引用 Cike.Core）。通常无需单独引入：`CikeDataEFCoreModule` 已 `[DependsOn]` 本模块，引入 EF Core 数据访问栈即传递加载 |

## Cike.UniversalId

### 定位

两个互不依赖的生成器，纯通用基础设施：

| 生成器 | 产出 | 注册方式 | 生命周期 |
|---|---|---|---|
| `SnowflakeIdGenerator` | `long` 雪花 Id | `CikeUniversalIdModule` 手工实例注册 | Singleton |
| `SequentialGuidGenerator` | `Guid` 顺序 Guid | 标记接口 `ITransientDependency` 自动 DI | Transient |

- 无号段（segment）模式、无中心化机器号协调——多实例 machineId 分配是使用方责任。
- 顺序 Guid 的唯一目标：让 Guid 在**特定数据库的排序规则下**连续，减少聚集索引页分裂；跨进程不保证全局有序。

### 能力清单

命名空间：`Cike.UniversalId.ULong`（雪花）、`Cike.UniversalId.Guids`（Guid）。

| API / 类型 | 精确签名 | 说明 |
|---|---|---|
| `ISnowflakeIdGenerator` | `long NextId()` | 雪花 Id。线程安全（全局锁） |
| `SnowflakeIdGenerator` | `SnowflakeIdGenerator(long machineId)` | 构造校验 machineId ∈ [0, 1023]，越界抛 `ArgumentException` |
| `IGuidGenerator` | `Guid Create()` | 顺序 Guid，用 `DefaultSequentialGuidType` |
| `SequentialGuidGenerator` | `Guid Create()`<br>`Guid Create(SequentialGuidType guidType)` | 第二个重载按次覆盖顺序类型（仅具体类有，接口只有无参版） |
| `CikeSnowflakeOptions` | `bool IsEnable { get; set; }` | 见"配置"——现状为无人读取的预留开关 |
| `CikeSequentialGuidGeneratorOptions` | `SequentialGuidType? DefaultSequentialGuidType { get; set; }`<br>`SequentialGuidType GetDefaultSequentialGuidType()` | 可空，`null` 时 getter 兜底 `SequentialAtEnd` |
| `SequentialGuidType` | 枚举，见下表 | 顺序段在 16 字节中的布局方式 |

**雪花 Id 位结构**（Twitter 雪花变体，共 63 位）：

| 区段 | 位数 | 说明 |
|---|---|---|
| 时间戳 | 41 | `UnixUtcMs - 1288834974657`（Twepoch = 2010-11-04T01:42:54.657Z）；容量约至 2080 年，无溢出保护 |
| 机器号 | 10 | 0~1023 |
| 序列 | 12 | 同毫秒内自增，0~4095，溢出自旋等待下一毫秒 |

Id 公式：`((unixUtcMs - 1288834974657) << 22) | (machineId << 12) | sequence`

**`SequentialGuidType` 枚举值**（16 字节 = 48 位毫秒时间戳 + 80 位加密随机数）：

| 枚举值 | 顺序段位置 | 适用数据库 |
|---|---|---|
| `SequentialAsString` | 头部（小端系统做字节序补偿，`ToString()` 排序连续） | MySql、PostgreSql（本仓库仅提供 MySql 方言包） |
| `SequentialAsBinary` | 头部（`ToByteArray()` 排序连续） | Oracle（本仓库无方言包，需手动 Configure） |
| `SequentialAtEnd` | 尾部（Data4 末 6 字节，SqlServer 聚集索引按尾部排序） | SqlServer |

### 隐式行为

1. **雪花注册硬编码 machineId=1**：`CikeUniversalIdModule.ConfigureServicesAsync` 执行 `context.Services.AddSingleton(typeof(ISnowflakeIdGenerator), new SnowflakeIdGenerator(1))`。单实例无害；多实例产生重复 Id（见"边界与反模式"）。
2. **顺序 Guid 自动 DI**：`SequentialGuidGenerator : IGuidGenerator, ITransientDependency`，按标记接口机制自动注册（Transient），注入 `IGuidGenerator` 即得 `SequentialGuidGenerator`。
3. **`CikeDataEFCoreModule` 自动置 `CikeSnowflakeOptions.IsEnable = true`**：且该模块 `[DependsOn(CikeUniversalIdModule)]`——引用任何 EF Core 数据访问包（含 .MySql/.SqlServer 方言包，它们都依赖 `CikeDataEFCoreModule`）即传递加载本模块。
4. **方言包各自填默认顺序类型**（仅当当前值为 `null` 时才填，不覆盖用户已配置值）：
   - `CikeDataEFCoreMySqlModule` → `SequentialAsString`
   - `CikeDataEFCoreSqlServerModule` → `SequentialAtEnd`
   - 两者都未加载时，`GetDefaultSequentialGuidType()` 兜底返回 `SequentialAtEnd`
5. **`CikeDbContext` 自动赋 Id**：构造时挂接 `ChangeTracker.Tracked` / `StateChanged` 钩子 → 实体进入 `EntityState.Added` → `EntityHelper.TrySetId(entry)`：`IEntity<long>` 且 `Id == 0` 调 `NextId()`；`IEntity<Guid>` 且 `Id == Guid.Empty` 调 `Create()`。即 `repository.InsertAsync` 内部 `AddAsync` 的跟踪瞬间赋值，**早于 `SaveChanges`**——传 `autoSave: false` 时 `entity.Id` 也已可用。已手工赋过非默认 Id 的实体不会被覆盖。
6. **`IsEnable` 现状为死开关**：框架内无任何代码读取 `CikeSnowflakeOptions.IsEnable`，自动赋 Id 无条件执行；把它设回 `false` 不会关闭赋值。

### 示例

**1. 默认用法——零调用（绝大多数场景）**

```csharp
// Domain 层：实体继承基类即可，Id 留默认值
public class Order : AggregateRoot<long>   // 已实现 IEntity<long>
{
    public string OrderNo { get; set; } = default!;
}

// Application 层：不注入任何 Id 生成器
public class OrderCommandHandler(IRepository<Order, long> orderRepository)
{
    public async Task HandleAsync(CreateOrderCommand command)
    {
        var order = new Order { OrderNo = command.OrderNo };  // Id == 0，不要手工赋值
        await orderRepository.InsertAsync(order, autoSave: false);
        // AddAsync 跟踪瞬间已赋雪花 Id，此处 order.Id 已可用，无需等 SaveChanges
    }
}
```

**2. 手动注入——仅在两种情况才需要**

需要手动调用的判定：(a) 构造实体**之前**就要 Id（如领域事件载荷携带实体 Id、聚合构造函数引用自身 Id）；(b) 生成**非实体标识**但要入库做索引的值（文件 key、外部关联号）。注意 `InsertAsync(autoSave: false)` 后 Id 已就位——"保存前要 Id"不构成手动生成的理由。

```csharp
using Cike.UniversalId.Guids;
using Cike.UniversalId.ULong;

public class FileService(IGuidGenerator guidGenerator, ISnowflakeIdGenerator snowflakeIdGenerator)
{
    // (b) 非实体标识但入库做索引列：用顺序 Guid，不要 Guid.NewGuid()（随机 v4 碎片化索引）
    public string CreateBlobKey()
        => $"blob/{guidGenerator.Create():N}";

    // (a) 构造实体前就要 Id：事件载荷携带
    public OrderCreatedEvent BuildEventBeforeEntity()
        => new(OrderId: snowflakeIdGenerator.NextId(), Sku: "SKU-001");
}
```

**3. 多实例部署——替换默认雪花注册（必做）**

```csharp
using Cike.Core.Modularity;
using Cike.UniversalId.ULong;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// 启动模块：模块按依赖排序执行、启动模块最后运行，
// 此处注册必然晚于 CikeUniversalIdModule 的默认注册
public class ProductServiceModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        // machineId ∈ [0, 1023]，多实例间必须互不相同
        //（配置中心 / 环境变量 / K8s StatefulSet 序号 / IP 段分配，使用方自定）
        var machineId = context.Services.GetConfiguration().GetValue("IdGenerator:MachineId", 1L);

        // Replace：移除默认的 machineId==1 注册，换成自己的
        context.Services.Replace(
            ServiceDescriptor.Singleton<ISnowflakeIdGenerator>(new SnowflakeIdGenerator(machineId)));

        return base.ConfigureServicesAsync(context);
    }
}
```

说明：再次 `AddSingleton` 也能让单次解析命中新注册（DI 取最后一条），但旧描述符仍残留在 `IEnumerable<ISnowflakeIdGenerator>` 中，`Replace` 更干净。`[Dependency(ReplaceServices = true)]` 不适用于此处——它只作用于标记接口自动注册，而默认注册是模块代码里的手工实例注册。

### 配置

| 选项 | 类型 / 默认 | 行为 |
|---|---|---|
| `CikeSnowflakeOptions.IsEnable` | `bool`，默认 `false` | `CikeDataEFCoreModule` 无条件置 `true`；**框架内无代码读取**，不控制任何行为，视为预留字段 |
| `CikeSequentialGuidGeneratorOptions.DefaultSequentialGuidType` | `SequentialGuidType?`，默认 `null` | 决定无参 `Create()` 的顺序类型。`null` 时取值链：方言包默认 → `SequentialAtEnd` 兜底 |
| machineId | 无配置项 | 硬编码 `1`，只能靠替换注册改变（见示例 3） |

未显式配置时各方言的实际顺序类型：

| 已加载方言包 | 实际 `DefaultSequentialGuidType` |
|---|---|
| Cike.Data.EFCore.MySql | `SequentialAsString` |
| Cike.Data.EFCore.SqlServer | `SequentialAtEnd` |
| 均未加载 | `SequentialAtEnd`（getter 兜底） |

显式覆盖（放在启动模块，晚于方言包执行，必然生效；方言包只填 `null`，不会反向覆盖用户值）：

```csharp
using Cike.UniversalId.Guids;

public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
{
    context.Services.Configure<CikeSequentialGuidGeneratorOptions>(options =>
    {
        options.DefaultSequentialGuidType = SequentialGuidType.SequentialAsBinary; // 如 Oracle
    });
    return base.ConfigureServicesAsync(context);
}
```

按次覆盖（不走全局配置）：注入具体类 `SequentialGuidGenerator`（自动注册了自身），调 `Create(SequentialGuidType)` 重载。

### 边界与反模式

**机器号硬编码 1——多实例重复 Id（最高优先级陷阱）**：默认注册 `new SnowflakeIdGenerator(1)`。单实例（含开发环境）无问题；同一服务横向扩到 ≥2 个实例时，两个进程可能产出**完全相同的 Id**（同毫秒 + 同机器号 + 序列各自独立自增），造成主键冲突或更隐蔽的静默数据错乱。多实例部署必须按示例 3 替换注册。框架不提供机器号中心化分配，协调成本由使用方承担。

**时钟回拨直接抛异常**：`NextId()` 检测到当前时间早于上次生成时间即 `throw new Exception("Clock moved backwards...")`——是裸 `Exception`，不属于框架异常体系（不会转 HTTP 400）。宿主机 NTP 跳变可触发，部署需保证时钟单调（如 chrony 渐进同步）。

**单机吞吐上限**：每个 machineId 每毫秒 4096 个 Id；同毫秒溢出时 `WaitNextMillis` **自旋等待**（busy loop 占 CPU），不抛错、不阻塞让出。

**时间戳无溢出保护**：41 位自 2010-11-04 起约 69 年，约 2080 年后高位溢出破坏 Id 结构，代码无检测。

**锁为 static**：`SnowflakeIdGenerator` 内部锁对象是 `static`——同进程内所有实例共享一把锁。多 new 几个生成器不会提升并发吞吐，实例隔离的意义仅在 machineId 不同。

**顺序 Guid 的有序性边界**：单进程内按生成顺序单调（毫秒精度、`DateTime.UtcNow`）；跨进程/跨实例不保证全局有序。唯一性依赖 80 位加密随机段，同毫秒碰撞概率 2^-80（可忽略但非零）；产出**不是** RFC 4122 v4 兼容 Guid，仅作数据库主键用。

**long Id 的前端精度**：雪花 Id 超 JS `Number.MAX_SAFE_INTEGER`，HTTP 层已自动把 `long`/`long?` 序列化为字符串，前端按 string 处理（详见[HTTP 接口层](./http-api.md)）。

**反模式**：

- 不要给新增实体手工赋 Id、更不要手工填 `Guid.NewGuid()`——随机 v4 作聚集索引主键会碎片化；确需构造前持有 Id，用 `IGuidGenerator.Create()`（赋过的非默认 Id 框架不会覆盖）。
- 不要 `new SnowflakeIdGenerator(...)` 临时实例使用——绕开统一注册与机器号管理；要 Id 就注入 `ISnowflakeIdGenerator`。
- 不要试图用 `CikeSnowflakeOptions.IsEnable = false` 关闭自动赋 Id——该开关现状无人读取，设置无效。
- 不要假设顺序 Guid 跨服务全局递增——跨进程只有"趋势大致有序"，排序语义仅限单库单索引。
- 单实例项目直接用默认注册即可，但上生产扩容前必须先替换 machineId，不要带着默认注册横向扩容。

# Cike.Framework 面向 AI 的开发指南

> 本文档的读者是 AI 编码助手。目标：让 AI 在基于 Cike.Framework 开发业务项目时，**不读框架源码也能正确工作**。
> 分层结构以 `Cike.Workflow/Backend` 为基准（DDD 分层 + Store 模式），框架源码位于 `Cike.Framework/src`。
> 最后更新：2026-09-10，对应框架源码 main 分支（含 `FailureLevel` 默认值修正：`Throw`）。
>
> **部署方式**：本文件是框架指南，不含项目信息。新建项目时配合 `docs/CLAUDE-TEMPLATE.md` 使用——
> 用它生成项目的 `CLAUDE.md`（项目硬事实 + 与本指南的偏离点），本项目则作为参考资料放路径引用或拷贝进项目。
> 两个文件配合的裁决规则见 CLAUDE-TEMPLATE.md 第 2 节。

---

## 0. 本文使用规则（AI 必读）

1. 本文中所有标注 **【隐式行为】** 的条目，是框架靠约定/扫描自动完成的行为，**不需要**（也不应该）手写代码注册。
2. 本文中所有标注 **【陷阱】** 的条目，是违反直觉的实现细节，写代码时必须避开。
3. 新增业务功能时，严格按 [第 9 节 Recipe](#9-recipe新增一个业务功能的完整步骤) 的文件结构和代码模式复制修改，不要自行发明结构。
4. 术语约定（全文严格一致）：
   - **Dto**：Application.Contracts 中的数据传输对象（`XxxDto`）
   - **Command / Query**：应用层用例定义，通过 `ILocalEventBus.PublishAsync` 派发
   - **Handler**：用 `[LocalEventHandler]` 标注的处理方法所在的类（`XxxCommandHandler` / `XxxQueryHandler`）
   - **Store**：领域层的数据访问接口 `IXxxStore`，EFCore 层实现 `XxxStore`
   - **Service**：HTTP 端点类，继承 `MinimalApiServiceBase`（注意拼写是 Service，不是 AppService）
   - **Module**：`CikeModule` 子类，每层一个

---

## 1. 全局架构速览

一个请求在框架中的完整链路：

```
HTTP 请求
  → MinimalApi 路由（按类名/方法名约定自动生成）
  → XxxService（MinimalApiServiceBase 子类）          [Service.Open 层]
  → new XxxCommand / XxxQuery()
  → ILocalEventBus.PublishAsync(command)               [本地事件总线 = CQRS 总线]
  → [LocalEventHandler] XxxCommandHandler.HandleAsync  [Application 层]
  → IXxxStore（IBaseStore<T> 实现）                    [Domain 定义接口 / EFCore 实现]
  → CikeDbContext（审计/软删/多租户/雪花Id 全自动）
  → 事件树结束后 IUnitOfWork.CommitAsync 提交事务
```

框架的核心机制只有四个，全部是**约定优于配置**：

| 机制 | 入口 | 作用 |
|---|---|---|
| 模块系统 | `CikeModule` + `[DependsOn]` | 替代手动 `Program.cs` 配置，按模块组织所有注册逻辑 |
| 自动 DI | `ISingletonDependency` 等标记接口 | 程序集扫描自动注册，禁止手写 `services.AddXxx` 注册业务类 |
| 本地事件总线 | `[LocalEventHandler]` + `ILocalEventBus` | 既是事件机制，也是 CQRS 的 Command/Query 派发器 |
| 自动路由 | `MinimalApiServiceBase` | 按类名/方法名约定生成 REST 路由，不需要写 `app.MapGet` |

---

## 2. 解决方案分层规范（以 Cike.Workflow 为基准）

标准解决方案结构（`src/` 下，项目名以 `Cike.Workflow` 为例，替换为你自己项目名）：

| 项目 | 职责 | 放什么 | 不许放什么 |
|---|---|---|---|
| `*.Domain.Shared` | 最稳定的领域概念 | 枚举、值对象、常量 | 实体类、业务逻辑 |
| `*.Domain` | 领域层 | 实体（`Data/Entities/`）、`IXxxStore` 接口（`Data/`）、领域服务、物化器 | EF Core 代码、Dto、HTTP 相关 |
| `*.Application.Contracts` | 应用层契约 | `XxxDto`（按业务子目录组织） | 业务逻辑、Handler |
| `*.Application` | 应用层 | `XxxCommand`/`XxxQuery`（`Commands/`、`Queries/` 子目录）、`XxxCommandHandler`/`XxxQueryHandler`、FluentValidation 的 `Validator`（`Validators/` 子目录） | 实体类、DbContext、HTTP 相关 |
| `*.EntityFrameworkCore` | EF Core 实现 | `XxxDbContext`、`XxxStore` 实现、`EntityConfigurations`、`Migrations` | 业务规则 |
| `*.Service.Open`（或 `*.Host`） | HTTP 宿主 | `Program.cs`、启动模块、`Services/XxxService.cs`（MinimalAPI 端点）、`appsettings.json` | 业务逻辑、实体 |
| `*.Caching` / `*.Common`（可选） | 项目级基础设施 | 项目自用的缓存封装（`ICacheService<T>`、缓存投影模型 `XxxCacheModel`）、序列化工具等 | 业务逻辑 |

**命名规范**：
- 每个项目一个模块类：`CikeWorkflowApplicationModule`、`CikeWorkflowDomainModule` …（`项目名去掉点 + Module`）
- 每个项目一个 `_Imports.cs`，用 `global using` 集中管理公共引用（代替逐文件 using）
- 业务按聚合根组织子目录：`Application/Folders/` 下有 `Commands/`、`Queries/`、`Validators/`、`FolderCommandHandler.cs`、`FolderQueryHandler.cs`
- 注意：`CommandHanlder` 这个拼写错误在历史代码中存在（少了个 d），**新代码一律写 `Handler` 正确拼写**

**模块依赖关系**（谁 DependsOn 谁）：

```
Service.Open ──→ Application ──→ Domain ──→ Domain.Shared
      │              │            │
      │              │            └──→ CikeCachingModule
      │              └──→ Application.Contracts
      │
      ├──→ EntityFrameworkCore ──→ CikeDataEFCoreMySqlModule + CikeCachingModule
      ├──→ CikeAspNetCoreMinimalApiModule
      └──→ CikeFluentValidationModule
```

---

## 3. 模块系统

### 3.1 CikeModule 生命周期

每个模块继承 `Cike.Core.Modularity.CikeModule`，可覆写三个方法：

```csharp
public class CikeWorkflowApplicationModule : CikeModule
{
    // 阶段一：服务注册。等价于 Program.cs 里的 builder.Services.AddXxx
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        // context.Services 是 IServiceCollection
        // context.Services.GetConfiguration() 拿 IConfiguration
        await base.ConfigureServicesAsync(context);
    }

    // 阶段二：应用初始化（管道配置）。等价于 Program.cs 里的 app.UseXxx / app.MapXxx
    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();          // IApplicationBuilder，来自 Cike.AspNetCore.MinimalAPIs
        var routeBuilder = context.GetEndpointRouteBuilder(); // IEndpointRouteBuilder
        await base.InitializeAsync(context);
    }

    // 阶段三：应用停止
    public override async Task ShutdownAsync(ApplicationShutdownContext context) { }
}
```

- `GetApplicationBuilder()` / `GetEndpointRouteBuilder()` 是 `Cike.AspNetCore.MinimalAPIs` 提供的扩展方法，**只能在宿主层（依赖了 `CikeAspNetCoreMinimalApiModule`）使用**
- 重写方法体末尾保持 `await base.XxxAsync(context)` 调用（跟随现有项目习惯）

### 3.2 模块声明与依赖

```csharp
[DependsOn([
    typeof(CikeWorkflowApplicationModule),
    typeof(CikeAspNetCoreMinimalApiModule),
    ])]
public class CikeWorkflowServiceOpenModule : CikeModule { }
```

- `[DependsOn]` 递归收集整棵依赖树，重复依赖自动去重
- 【隐式行为】依赖了某模块，等于激活了它的一切：自动 DI 扫描范围、事件 Handler 扫描范围、MinimalAPI 端点扫描范围，**都是"所有已加载模块所在的程序集"**。所以：一个程序集里的类要被框架发现，唯一途径是它所在的项目被某个已加载模块代表
- 【隐式行为】**模块执行顺序**：`ConfigureServicesAsync` 从启动模块开始、由上层到底层模块串行执行（启动模块最先，基础框架模块最后）；`InitializeAsync` 各模块**并发执行**（`Task.WhenAll`）。因此：不要依赖"框架模块先执行"的假设；想覆盖框架默认配置，用 `services.Configure<T>(...)` 追加而非替换

### 3.3 启动引导（宿主 Program.cs 的全部内容）

```csharp
var builder = WebApplication.CreateBuilder(args);

// Serilog 日志（现有项目惯例，可按需调整）
Log.Logger = new LoggerConfiguration() /* ... */ .CreateLogger();
builder.Logging.AddSerilog();

// 一行完成所有模块加载、DI 约定注册、ConfigureServicesAsync
await builder.Services.AddApplicationAsync<CikeWorkflowServiceOpenModule>();

var app = builder.Build();

// 一行完成所有模块 InitializeAsync + Shutdown 钩子注册
await app.InitializeApplicationAsync();
app.Run();
```

`AddApplicationAsync<TStartupModule>` 内部做三件事（`ModularityFactory`）：
1. 从启动模块递归收集模块树，实例化所有模块
2. 对每个模块程序集做自动 DI 注册（见第 4 节）
3. 按序调用每个模块的 `ConfigureServicesAsync`

---

## 4. 自动依赖注入（禁止手写注册）

### 4.1 标记接口

| 接口 | 生命周期 | 用在 |
|---|---|---|
| `ISingletonDependency` | Singleton | 无状态服务、`MinimalApiServiceBase` 子类 |
| `IScopedDependency` | Scoped | DbContext、Store、有状态服务 |
| `ITransientDependency` | Transient | 轻量无状态服务 |

命名空间：`Cike.Core.DependencyInjection`。实现任意一个即被自动注册，**不需要也不允许再手写 `services.AddSingleton<XxxService>()`**。

【隐式行为】扫描范围 = 所有已加载模块所在程序集的全部非抽象类。注册内容为：
1. **类自身**，按标记接口指定的生命周期（`TryAdd`，先到先得）
2. **它的所有接口和基类** 全部映射到该实现

【陷阱】接口/基类的注册生命周期在框架实现里**硬编码为 Singleton**（`ModularityFactory` 中 `ServiceDescriptor.Describe(interfaceType, typeItem, ServiceLifetime.Singleton)`），与类自身标记的生命周期无关。因此：
- 通过接口注入得到的是单例实例——**把 Handler、Service、Store 设计成无状态**（现有项目全部如此）
- 不要在 Singleton 生命周期类中用构造函数注入并持有 Scoped 服务；需要时在方法参数上用 `[FromServices]` 现取（MinimalAPI 端点就是这么做的）

### 4.2 [Dependency] 特性（需要 keyed 服务或替换时）

```csharp
[Dependency(Key = "legacy", ReplaceServices = true)]  // keyed 注册 / 替换已有注册
public class LegacyCacheService : ICacheService<FolderCacheModel> { }
```

### 4.3 哪些类"不用标记也会被注册"

【隐式行为】`CikeEventBusLocalModule` 会把**所有含 `[LocalEventHandler]` 方法的类**自动 `AddScoped`——Handler 类不需要实现任何标记接口（现有项目的 `FolderCommandHandler` 就没有）。

---

## 5. CQRS：Command / Query / Handler

框架的 CQRS 派发复用本地事件总线实现，**没有独立的 Dispatcher**。

### 5.1 定义用例

```csharp
// Application/Folders/Commands/AddFolderCommand.cs
// Command：无返回值；需要向调用方回传数据时，用可写属性（引用传递）
public record AddFolderCommand(AddFolderDto Dto) : Command   // 继承 Cike.Cqrs.Commands.Command
{
    public long Id { get; set; }   // handler 填充，Service 层读取
}

// Application/Folders/Queries/GetFolderQuery.cs
// Query：有返回值，Result 由 handler 赋值
public record GetFolderQuery(long Id) : Query<FolderDetailDto>;  // 继承 Cike.Cqrs.Queries.Query<TResult>
```

### 5.2 编写 Handler

```csharp
// Application/Folders/FolderCommandHandler.cs
public class FolderCommandHandler(
    IFolderStore folderStore,
    IWorkspaceStore workspaceStore)
{
    [LocalEventHandler]
    public async Task AddAsync(AddFolderCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Dto;
        // 业务校验失败 → 抛 UserFriendlyException，自动变成 400 响应
        var exists = await workspaceStore.Queryable.AsNoTracking()
            .AnyAsync(x => x.Id == dto.WorkspaceId, cancellationToken);
        if (!exists)
            throw new UserFriendlyException("所属工作空间不存在，请检查后重试。");

        var entity = dto.Adapt<Folder>();              // Mapster 映射 Dto → Entity
        await folderStore.AddAsync(entity, cancellationToken);
        command.Id = entity.Id;                        // 回传给调用方
    }
}
```

Handler 方法签名规则（`LocalEventHandlerRelationContainer` 强制）：
- 方法必须标注 `[LocalEventHandler]`
- 参数中**有且仅有一个**参数是 `IEvent` 派生类型（即 Command/Query）
- 可以有任意多个额外参数，额外参数从 DI 解析注入（等价构造函数注入）；`CancellationToken` 会被自动传入
- Handler 实例本身是 Scoped，通过 DI 构造函数注入依赖
- 同一个 Command 可以有多个 Handler，按 `[LocalEventHandler(order: n)]` 的 `Order` 升序执行（默认 100）

### 5.3 派发用例（Service 层写法）

```csharp
var query = new GetFolderQuery(folderId);
await localEventBus.PublishAsync(query, cancellationToken);   // 同步执行，返回时 handler 已完成
return TypedResults.Ok(query.Result);                          // 直接读 Result
```

`ILocalEventBus.PublishAsync` 语义（`LocalEventExecutor` + `SagaStrategyExecutor`）：
- 【陷阱】**同步**执行：PublishAsync 返回时所有 handler 已执行完（事务随后由中间件提交）。不存在"发布后不管"的异步语义，除非调用 `command.EnableBackgroundThread()` 切到后台通道
- 【陷阱】**没有注册 handler 的事件被静默忽略**（不抛错）。新增 Command 后接口 404/无反应时，先检查 handler 是否标注了 `[LocalEventHandler]`、Command 类型是否一致
- Handler 抛异常：默认中断后续 handler 并把异常抛回调用方；`[LocalEventHandler(RetryCount = n)]` 失败后自动重试 n 次（共 1+n 次尝试）；`FailureLevel = Ignore` 可忽略单个 handler 的失败
- 补偿语义：`IsCancel = true` 的 handler 是取消处理器，在普通 handler 失败时按 Order 补偿（Saga），详见 5.6

### 5.4 事务

【隐式行为】Service 层的"根" `PublishAsync` 会经过 `DbTransactionLocalEventMiddleware`（`ExecutionPolicy = OncePerTree`）：整个事件树执行成功 → `IUnitOfWork.CommitAsync()`；任一 handler 抛异常 → `RollbackAsync()`。即 **一次根 PublishAsync = 一棵事件树 = 一个事务**。此外 `CikeDbContext` 在实体状态变化时会自动开启事务（`UnitOfWorkOptions.Enable`，默认 true）。

精确语义：
- 事务包裹的是**整棵树**——handler 内部再 `PublishAsync` 别的事件（嵌套发布），嵌套事件不会开启第二个事务，与外层同生共死（`OncePerTree`：同一 Scope 内首次发布才装配这些中间件；树结束时 `PreventRecursiveMiddleware` 重置计数，同一请求里下一次根发布重新装配）
- handler 全程没碰数据库时，UoW 未开事务，Commit/Rollback 均为空操作（`CommitState == Unknown` 直接跳过），不会报错
- 所以：**不要在 Handler 里手动管理事务**，需要原子性的一组操作放进同一个 Command 的一个 Handler 里

### 5.5 后台事件（fire-and-forget）

不需要阻塞请求/不要求事务的事件，走后台线程：

```csharp
// 方式一：定义事件时直接继承 BackgroundEvent（构造时自动 EnableBackgroundThread）
public record SendEmailCommand(string To, string Subject) : BackgroundEvent;

// 方式二：发布前对任意 Event 调用
command.EnableBackgroundThread();
await localEventBus.PublishAsync(command, ct);
```

【隐式行为】`PublishAsync` 检测到 `IsBackgroundThread()` 为 true → 写入 Channel 立即返回；后台由 `Parallel.ForEachAsync` 并发消费，**每个事件在独立的 DI Scope 中执行**。

【陷阱】后台事件的 handler 异常**只记日志、不抛给发布方**，且**不参与发布方的事务**（`DbTransactionLocalEventMiddleware` 各事件树独立包裹）。需要"要么全成功要么全回滚"的操作不要用后台事件。

### 5.6 取消处理器（Saga 补偿）

一个事件可以注册两类 handler：

```csharp
public class OrderCommandHandler
{
    [LocalEventHandler(order: 100)]                          // FailureLevel 默认 Throw；旧版包项目需显式 FailureLevel = FailureLevelEnum.Throw
    public async Task CreateAsync(CreateOrderCommand command) { /* ... */ }

    [LocalEventHandler(order: 100, IsCancel = true)]         // 取消 handler：正常 handler 失败时补偿
    public async Task CreateCancelAsync(CreateOrderCommand command) { /* 回滚业务 */ }
}
```

执行语义（`LocalEventExecutor`）：
- 正常 handler 按 Order 升序串行执行；某个 handler 抛异常（重试耗尽后）→ 触发补偿：执行取消 handler 中 `Order` 不大于失败点（`FailureLevel=Throw` → `<= 失败Order-1`）/ 含失败 handler 自身（`ThrowAndCancel` → `<= 失败Order`）的那些，然后把异常抛回发布方
- `[LocalEventHandler(FailureLevel = FailureLevelEnum.Ignore)]`：该 handler 的失败被忽略（记 Error 日志），不中断、不补偿
- 取消 handler 失败 → 状态记为 `RollbackFailed`，原异常附带 `cancel handler exception` 数据后抛出
- 【陷阱】`FailureLevel` **当前默认值为 `Throw`**（2026-09 框架源码修正）。更早的版本中默认值是未定义的枚举值 0（`Throw = 1` 起始），事件一旦注册取消 handler、普通 handler 失败就会抛 `NotImplementedException`——**仍引用旧版 NuGet 包（如 Workflow 项目的 `$(CikeVersion)` 固定版本）的存量项目必须显式写 `FailureLevel = FailureLevelEnum.Throw`**
- 【陷阱】`ExceptionLocalEventMiddleware` 里有一条"事件已 Succeed 但外层异常 → 调用 `CancelAsync` 全量补偿"的分支（典型场景：handler 成功但事务提交失败），但**当前源码没有任何地方把 `Status` 置为 `Succeed`/`InProgress`**，该分支是未完成实现的死代码——不要依赖"提交失败自动补偿"，提交阶段的问题自己处理（重试/告警）

### 5.7 自定义事件中间件

所有事件在 handler 前后经过中间件管线（`ILocalEventMiddleware<TEvent>`），框架内置三个（`DbTransactionLocalEventMiddleware` / `ExceptionLocalEventMiddleware` / `PreventRecursiveMiddleware`）。为特定事件类型追加自定义中间件（参考 `Cike.Workflow.Core` 的用法）：

```csharp
public class ExceptionRunActivityInstanceMiddleware : ILocalEventMiddleware<RunActivityInstanceCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.OncePerTree;
    public async Task HandleAsync(RunActivityInstanceCommand @event, EventHandlerDelegate next)
    {
        try { await next(); }
        catch (Exception ex) { /* 记录执行日志等 */ throw; }
    }
}

// 模块中注册（TryAddEnumerable，open generic）
context.Services.TryAddEnumerable(new ServiceDescriptor(
    typeof(ILocalEventMiddleware<RunActivityInstanceCommand>),
    typeof(ExceptionRunActivityInstanceMiddleware), ServiceLifetime.Transient));
```

### 5.8 领域事件（聚合根 → 事件）

聚合根继承 `AggregateRoot` 系基类后自带 `AddDomainEvent(IDomainEvent)` / `DomainEvents`：

```csharp
public class Todo : FullAuditedAggregateRoot<long>
{
    public void Complete() => AddDomainEvent(new TodoCompletedDomainEvent(Id));
}
```

【隐式行为】`CikeDbContext.SaveChangesAsync` 时把聚合根上的领域事件逐个入队 `IQueueEventBus`；`IUnitOfWork.CommitAsync` 在提交数据库事务前按队列依次发布（领域事件 handler 同样是 `[LocalEventHandler]` 方法，事件类型是 `IDomainEvent` 实现）。即：**领域事件在 SaveChanges 之后、事务提交之前被处理**。

---

## 6. HTTP 接口层：MinimalApiServiceBase

### 6.1 端点类写法

```csharp
// Service.Open/Services/FolderService.cs
[AutoValidation]                                    // 类级：自动 FluentValidation 校验
public class FolderService : MinimalApiServiceBase  // 注意：基类已实现 ISingletonDependency，禁止再写标记接口
{
    public async Task<Results<Ok<FolderDetailDto>, BadRequest>> GetAsync(
        [FromServices] ILocalEventBus localEventBus,   // Scoped 服务一律用方法参数注入
        long folderId,                                  // 简单类型自动绑定（query/body 按动词决定）
        CancellationToken cancellationToken = default)
    {
        var query = new GetFolderQuery(folderId);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<long>, BadRequest>> AddAsync(
        [FromServices] ILocalEventBus localEventBus,
        AddFolderDto dto,                              // 复杂类型 = 请求体（POST）
        CancellationToken cancellationToken = default)
    {
        var command = new AddFolderCommand(dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok(command.Id);
    }
}
```

- 返回值用 `Results<Ok<T>, BadRequest>` 的 Typed Results
- MinimalAPI 端点是 Singleton：**禁止在 Service 里用构造函数注入 Scoped 服务**（DbContext、Store、EventBus 都必须 `[FromServices]` 方法参数注入）

### 6.2 路由生成规则（记住这张表就够了）

【隐式行为】端点路由完全由命名约定推导，规则（`CikeAspNetCoreMinimalApiModule.AddCikeMinimalAPIs`）：

1. 路由前缀固定：`api/v1`（`GlobalMinimalApiRouteOptions.Prefix="api"` + `Version="v1"`）
2. 资源名 = 类名去掉后缀 `Service`/`AppService`（大小写不敏感），再**英文复数化**（保留原大小写）：`FolderService` → `Folder` → `Folders`
3. HTTP 动词由方法名**前缀**映射；无匹配前缀 → **默认 POST**：

   | 方法名前缀 | 动词 |
   |---|---|
   | `Get` / `Find` | GET |
   | `Post` / `Create` / `Add` / `Upsert` | POST |
   | `Put` / `Update` / `Modify` | PUT |
   | `Delete` / `Remove` | DELETE |

4. 路由尾段 = 方法名去掉动词前缀和 `Async` 后缀；为空则不追加
5. 参数里名为 `id`（不区分大小写、无绑定特性）的简单参数变成路由段 `{id}`
6. 扫描范围：所有已加载模块程序集中的 `MinimalApiServiceBase` 非抽象子类；`[NonAction]` 标注的方法排除

示例推导结果（`FolderService`，注意资源段**保留类名大小写**、只有名为 `id` 的参数才生成 `{id}` 段）：

| 方法 | 生成的路由 | 说明 |
|---|---|---|
| `GetAsync(long id)` | `GET api/v1/Folders/{id}` | 参数名必须是 `id` 才进路由 |
| `GetAsync(long folderId)` | `GET api/v1/Folders?folderId=1` | 参数名不是 id → 查询串绑定 |
| `AddAsync(AddFolderDto dto)` | `POST api/v1/Folders` | 复杂类型 = 请求体 |
| `UpdateAsync(long id, UpdateFolderDto dto)` | `PUT api/v1/Folders/{id}` | id 进路由，Dto 进请求体 |
| `UpdateAsync(long folderId, UpdateFolderDto dto)` | `PUT api/v1/Folders?folderId=1` | 注意与上一行的差别 |
| `DeleteAsync(long id)` | `DELETE api/v1/Folders/{id}` | |
| `GetPagedListAsync(...)` | `GET api/v1/Folders/PagedList` | 方法名去动词前缀后非空 → 追加段 |

【陷阱】路由匹配大小写不敏感（`/api/v1/folders/1` 也能命中），但 Swagger 上展示的是 `api/v1/Folders/{id}`（保留 `FolderService` 去后缀后 `Folder` 的首字母大写 + 复数化）。REST 路径段是否有 `{id}` **完全取决于参数名是不是 `id`**——Workflow 项目实际用的参数名是 `folderId`/`workspaceId`，因此它们的真实路由全部走查询串，前端对接时以 Swagger 为准。

显式覆盖：`[MinimalApiRoute("/my/route", "GET")]` 标注方法即可完全指定 pattern 和动词。

### 6.3 全局 HTTP 行为（自动生效）

【隐式行为】`CikeAspNetCoreMinimalApiModule` 初始化时自动装配：
- `BusinessExceptionMiddleware`：`UserFriendlyException` / `BusinessException` → **HTTP 400**，响应体为异常 Message 文本（`Results.BadRequest(ex.Message)`），其他异常按 500 透传
- CORS：读取 `appsettings.json` 的 `CorsDomains` 数组
- JSON：`long`/`long?` 序列化为字符串（`LongToStringConverter`，防前端精度丢失）——**前端拿到的 Id 是字符串**
- Swagger：宿主模块显式调用（见 6.4）

【陷阱】默认**每个端点都要求 Authorization**（`MinimalApiRouteOptions.EnabledAuthorization = true`，`RouteHandlerBuilder = RequireAuthorization`）。开发期未配 JWT 时接口会 401，需要在启动模块 `ConfigureServicesAsync` 中关闭：

```csharp
context.Services.Configure<GlobalMinimalApiRouteOptions>(options =>
{
    options.EnabledAuthorization = false;  // 或 true 时确保 app.UseAuthentication/UseAuthorization 生效
});
```

### 6.4 宿主模块的标准装配（照抄）

```csharp
[DependsOn([
    typeof(CikeWorkflowApplicationModule),
    typeof(CikeWorkflowEntityFrameworkCoreModule),
    typeof(CikeAspNetCoreMinimalApiModule),
    typeof(CikeFluentValidationModule),
    ])]
public class CikeWorkflowServiceOpenModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddCikeSwagger("Cike");   // 文档名；Validator 已由 CikeFluentValidationModule 自动注册，勿重复
        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        context.GetApplicationBuilder().UseCikeSwaggerUI("Cike");
        // 需要多租户时（实体实现了 IMultiTenant 必加）：
        // context.GetApplicationBuilder().UseMultiTenant();
        await base.InitializeAsync(context);
    }
}
```

---

## 7. 数据访问

### 7.1 实体基类选择

命名空间 `Cike.Data.Domain`（基类）/ `Cike.Data`（接口）。继承链：

| 基类 | 获得 | 用途 |
|---|---|---|
| `Entity<TKey>` | `Id` | 纯实体 |
| `AuditedEntity<TKey>` | + `CreatedAt` `CreatedBy` `UpdatedAt` `UpdatedBy`（`long` UserId） | 需审计 |
| `FullAuditedEntity<TKey>` | + `IsDeleted`（软删） | 需软删 |
| `AggregateRoot<TKey>` | + `DomainEvents`（`AddDomainEvent`/`ClearDomainEvents`，领域事件队列，见 5.8） | 聚合根（无审计） |
| `FullAuditedAggregateRoot<TKey>` | 审计 + 软删 + `DomainEvents` + `ConcurrencyStamp`（乐观并发，见 7.1 末尾） | **标准业务聚合根（推荐）** |

标准业务实体写法（无状态 POCO，继承 `FullAuditedAggregateRoot<long>`）：

```csharp
// Domain/Data/Entities/Folder.cs
public class Folder : FullAuditedAggregateRoot<long>, IMultiTenant
{
    public long TenantId { get; set; }    // 实现接口 IMultiTenant（Cike.Data），多租户自动生效
    public long WorkspaceId { get; set; }
    public string Name { get; set; } = null!;
}
```

【隐式行为】`CikeDbContext` 在实体被跟踪/状态变化时自动完成（`EntityHelper`）：
- **新增**：`IEntity<long>.Id == 0` → 自动生成**雪花 Id**（不要自己赋值 Id）；`IMultiTenant` 实体 → 自动填充 `TenantId = ICurrentUser.TenantId`；自动填 `CreatedAt/CreatedBy`
- **修改**：自动填 `UpdatedAt/UpdatedBy`，自动刷新 `ConcurrencyStamp`（乐观并发）
- **删除**：`ISoftDelete` 实体 → 自动软删（`IsDeleted = true`，从数据库 Reload 后更新，**不是** EF 的硬删）
- **查询过滤器**：`IMultiTenant` 实体全局过滤 `TenantId == ICurrentUser.TenantId`；`ISoftDelete` 实体全局过滤 `!IsDeleted`。**所有查询自动带上这两个条件**，写业务查询时不要重复手写

### 7.2 DbContext 标准写法

```csharp
// EntityFrameworkCore/CikeWorkflowDbContenxt.cs
public class CikeWorkflowDbContenxt : CikeDbContext<CikeWorkflowDbContenxt>   // 泛型参数必须是自身
{
    public DbSet<Folder> Folders { get; set; }

    public CikeWorkflowDbContenxt(DbContextOptions<CikeWorkflowDbContenxt> options,
        IServiceProvider serviceProvider)                                       // 双参构造必须保留
        : base(options, serviceProvider) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

EFCore 层模块标准写法（注册 DbContext 与 UoW）：

```csharp
[DependsOn([
    typeof(CikeWorkflowDomainModule),
    typeof(CikeDataEFCoreMySqlModule),        // 数据库 Provider（MySql；SqlServer 为 CikeDataEFCoreSqlServerModule）
    typeof(CikeWorkflowCachingModule),
    ])]
public class CikeWorkflowEntityFrameworkCoreModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddCikeDbContext<CikeWorkflowDbContenxt>();   // 注册 DbContextOptions 工厂 + IUnitOfWork
        return base.ConfigureServicesAsync(context);
    }
}
```

【陷阱】不要在应用模块里 `Configure<CikeDbContextOptions>(options => options.UseMySQL())`——数据库方言已由 Provider 模块（`CikeDataEFCoreMySqlModule`）设置；且 `CikeDbContextOptions` 内部是**单个 `DefaultConfigureAction` 委托，后设置者整体覆盖**。模块执行顺序是应用模块先、框架模块后（见 3.2），你在应用模块里的 Configure 会被 Provider 模块**静默覆盖**（框架 sample 里那行 `UseMySQL()` 是冗余的历史写法，不要模仿）。需要自定义 DbContext options（如自定义迁移程序集、连接级参数）时，不要依赖 Provider 模块的默认配置：在模块依赖里去掉 Provider 模块、自己 `Configure<CikeDbContextOptions>` 写完整的 `UseMySQL(...)` 配置即可。

### 7.3 连接串解析规则

【隐式行为】`DbContextOptionsFactory` 解析连接串的规则：
1. 连接串名 = DbContext 类名去掉 `DbContext` 后缀；可用 `[ConnectionStringName("name")]` 特性在 DbContext 类上显式指定
2. 用该名字去 `appsettings.json` 的 `ConnectionStrings` 节查值

```json
{
  "ConnectionStrings": {
    "CikeWorkflowDb": "Server=...;Port=...;Database=...;Uid=...;Pwd=...;",
    "CommandTimeout": 300
  }
}
```

【陷阱】名字必须严格匹配（类名后缀 `DbContext` 被去掉后的字符串）。注意 `CikeWorkflowDbContenxt` 这个历史拼写（`Contenxt`）导致类名后缀不匹配 `DbContext`，因此连接串 key 必须是完整的 `CikeWorkflowDbContenxt`。新项目 DbContext 命名请用正确拼写 `XxxDbContext`，连接串 key 用 `Xxx`。

### 7.4 Store 模式（数据访问的标准形态）

Domain 层定义接口（约束 `IEntity<long>`）：

```csharp
// Domain/Data/IBaseStore.cs —— 通用 CRUD 契约（直接复制）
public interface IBaseStore<TEntity> where TEntity : class, IEntity<long>
{
    IQueryable<TEntity> Queryable { get; }                    // 注意：使用时自行 AsNoTracking
    Task<TEntity?> FindAsync(long id, CancellationToken ct = default);
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity,bool>> filter, string sorting = "CreatedAt desc", CancellationToken ct = default);
    Task<(long Total, List<TEntity> Items)> ToPaginationAsync(IQueryable<TEntity> query, IPagedAndSortedRequest page, CancellationToken ct = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
}

// Domain/Data/IFolderStore.cs —— 每个聚合根一个空接口
public interface IFolderStore : IBaseStore<Folder> { }
```

EFCore 层实现（基类 `BaseStore<TEntity>` 直接复制；带缓存的泛型基类 `BaseStore<TEntity, TCacheModel>` 在写/删后自动同步 `ICacheService<TCacheModel>`）：

```csharp
// EntityFrameworkCore/Stores/FolderStore.cs
public class FolderStore(CikeWorkflowDbContenxt context, ICacheService<FolderCacheModel> cacheService)
    : BaseStore<Folder, FolderCacheModel>(context, cacheService), IFolderStore;
```

在 `*.Caching` 项目放缓存的投影模型（`Models/` 目录）。基类按需选 `EntityDto<long>`（只要 Id）或 `FullAuditedEntityDto<long>`（含审计字段，Workflow 项目的选择），必须实现 `IMultiTenant`：

```csharp
// *.Caching/Models/FolderCacheModel.cs —— 命名空间跟随项目（如 Cike.Workflow.Caching.Models）
public class FolderCacheModel : FullAuditedEntityDto<long>, IMultiTenant
{
    public long TenantId { get; set; }
    public long WorkspaceId { get; set; }
    public long ParentId { get; set; }
    public string Name { get; set; } = null!;
}
```

【陷阱】Workflow 项目里这些模型的物理位置在 `Cike.Workflow.Caching/Models/`，但**命名空间写的是 `Cike.Workflow.Domain.Shared.CacheModels`**（文件位置与命名空间不一致的历史遗留）。新项目让命名空间跟随所在项目，不要模仿。

【隐式行为】`BaseStore` 是抽象类但实现 `IScopedDependency`，子类实现接口即自动注册。读操作用 `Queryable.AsNoTracking()`，写操作内部 `SaveChangesAsync`（审计/软删/雪花 Id 在此刻自动生效）。

### 7.5 数据库迁移

用 EF 标准迁移（`dotnet ef migrations add <name>`），项目需包含 design-time factory（EF 工具链在生成迁移时不走框架 DI，直接 new）：

```csharp
// EntityFrameworkCore/CikeWorkflowDbContextFactory.cs
public class CikeWorkflowDbContextFactory : IDesignTimeDbContextFactory<CikeWorkflowDbContenxt>
{
    public CikeWorkflowDbContenxt CreateDbContext(string[] args)
    {
        var connectionString = "Server=localhost;Port=...;Database=...;Uid=...;Pwd=...;"; // 与 appsettings 一致（迁移时才用到）
        var optionsBuilder = new DbContextOptionsBuilder<CikeWorkflowDbContenxt>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        // 第二参数 IServiceProvider：给一个最小容器即可（模型构建不需要完整 DI）
        return new CikeWorkflowDbContenxt(optionsBuilder.Options,
            new ServiceCollection().AddLogging().BuildServiceProvider());
    }
}
```

连接串在 factory 里**硬编码或从 appsettings 读取均可**（只影响 `dotnet ef` 工具，不影响运行时——运行时走第 7.3 节的解析规则）。

---

## 8. 横切设施

### 8.1 异常与响应

- 业务校验失败：`throw new UserFriendlyException("人类可读的消息")`（`Cike.Core.Exceptions`）→ HTTP 400，响应体 = 消息文本
- 【陷阱】`UserFriendlyException` 之外的任何异常 → 500；且 handler 抛出的非 BusinessException 会先触发事件树回滚
- 消息使用中文、以句号结尾（跟随现有项目惯例）

### 8.2 参数验证（FluentValidation）

1. `Validator` 放 `Application/<聚合>/Validators/`，命名 `XxxDtoValidator`（校验对象是 **Dto**，不是 Command）
2. Service 类标注 `[AutoValidation]` → 请求自动校验所有带 Validator 的参数；失败返回 **400 ValidationProblem**，响应体是 ProblemDetails 格式：`{ "errors": { "<属性名>": ["<Validator 中 WithMessage 的消息>"] } }`（注意与 `UserFriendlyException` 的纯文本 400 不同）

【隐式行为】依赖 `CikeFluentValidationModule` 后，**所有已加载模块程序集中的 Validator 自动全部注册**（模块内部逐程序集调用 `AddValidatorsFromAssembly`），不需要再手写注册。（Workflow 宿主模块里残留的 `AddValidatorsFromAssembly(...)` 调用是历史冗余，新代码不要模仿。）

```csharp
public class AddWorkspaceDtoValidator : AbstractValidator<AddWorkspaceDto>
{
    public AddWorkspaceDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("工作空间名称不能为空。")
            .MaximumLength(128).WithMessage("工作空间名称不能超过128个字符。");
    }
}
```

### 8.3 缓存

框架设施（`Cike.CachingModule`，自动依赖加载）：
- `IMultilevelCacheClient`：内存 + Redis 两级缓存客户端，key 方法 `GetAsync<T>(key)` / `SetAsync` / `RemoveAsync` / `GetListAsync<T>(keys)` / `SetListAsync`
- 配置节 `RedisConfig`（`Servers[].Host/Port`、`DefaultDatabase`、`Password`），两级缓存配置节 `MultilevelCache`

项目级标准封装（`*.Caching` 项目，可直接照抄 Workflow 的实现）：

```csharp
public interface ICacheService<TModel> where TModel : EntityDto<long>, IMultiTenant
{
    Task<TModel?> GetAsync(long id, CancellationToken ct = default);
    Task<IEnumerable<TModel>> GetListAsync(Expression<Func<TModel, bool>>? filter = null, CancellationToken ct = default);
    Task SetAsync(TModel model, CancellationToken ct = default);
    Task SetListAsync(IEnumerable<TModel> models, CancellationToken ct = default);
    Task RemoveAsync(long id, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
}
```

注册方式（项目 Caching 模块内，open generic）：

```csharp
context.Services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ICacheService<>), typeof(BaseCacheService<>)));
```

### 8.4 多租户

- 实体实现 `IMultiTenant`（加 `long TenantId` 属性）→ 插入自动填 TenantId、查询自动过滤
- 启用请求级租户解析：宿主模块 `InitializeAsync` 中 `app.UseMultiTenant()`（`TenantMiddleware` 解析优先级：JWT Claims → Header `tenantid` → Cookie → Query，类型 `long`）
- 当前用户/租户：注入 `ICurrentUser`（Id/UserName/TenantId/Roles，来自 Claims）、`ICurrentTenant`
- 【陷阱】不调用 `UseMultiTenant()` 时，`ICurrentTenant` 不会被请求上下文更新；全局查询过滤器的租户值取自 `ICurrentUser.TenantId`（Claims），两者来源不同，注意区分

### 8.5 认证授权

JWT 配置写在宿主模块 `ConfigureServicesAsync`（`AddAuthentication().AddJwtBearer(...)` + `AddAuthorization`），参考 `samples/cqrs/CQRS.WebApi/CQRSWebApiModule.cs`。策略授权用 Claim（如 `RequireClaim("userType", "1")`）。端点默认 `RequireAuthorization`（见 6.3 的陷阱）。

### 8.6 对象映射

统一用 **Mapster**：`dto.Adapt<TEntity>()`、`entity.Adapt<XxxDto>()`（global using `Mapster` 已在各项目 `_Imports.cs` 中）。

### 8.7 分页查询

框架内置分页契约（`Cike.Contracts.EntityDtos`），配合 `IBaseStore.ToPaginationAsync`：

```csharp
// 请求参数（直接用，不要自造）：Page=1 起、PageSize、Sorting 为 System.Linq.Dynamic 语法（如 "CreatedAt desc"）
public class PagedAndSortedResultRequest : IPagedAndSortedRequest { Page; PageSize; Sorting; }

// 返回包装：Total + Items
public class PagedResultDto<T> { public long Total; public List<T> Items; }
```

完整用法见第 9 节的"分页列表查询"扩展 recipe。LINQ 辅助扩展（`Cike.Contracts.Extensions`，可直接用）：

```csharp
query.WhereIf(!string.IsNullOrEmpty(keyword), x => x.Name.Contains(keyword))  // 条件 Where
var (total, items) = await query.ToPaginationAsync(pageDto, cancellationToken); // Count + OrderBy + Skip/Take 一条龙
```

### 8.8 Id 生成器（需要手工生成 Id 时）

- `ISnowflakeIdGenerator.NextId()` → `long`（`Cike.UniversalId`；实体主键为空时框架已自动调用，通常不需要手工用）
- `IGuidGenerator.Create()` → 顺序 Guid（同上）
- `IEntity<Guid>` 主键实体同样自动生成

### 8.9 本地化（Cike.Localization）

提供 `IStringLocalizer` 集成（`CikeStringLocalizer` + `CikeStringLocalizerFactory`）。现有项目多为中文硬编码消息（`UserFriendlyException("中文消息")`），未启用本地化资源；如果项目要求多语言消息，再引入此模块，用 `[LocalizationResourceName]` 标注资源类。

### 8.10 appsettings 完整参考

```jsonc
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",

  // 数据库连接：key 必须匹配 DbContext 名（去掉 DbContext 后缀），见 7.3
  "ConnectionStrings": {
    "CikeWorkflowDb": "Server=...;Port=...;Database=...;Uid=...;Pwd=...;SslMode=None;Pooling=true;"
  },

  // Redis（Cike.Caching 用）：IMultilevelCacheClient 的分布式层
  "RedisConfig": {
    "Servers": [ { "Host": "...", "Port": 6379 } ],
    "DefaultDatabase": 0,
    "Password": "..."
  },

  // CORS 白名单（CikeAspNetCoreMinimalApiModule 自动读取）：字符串数组，null 时默认 ["localhost"]
  "CorsDomains": [ "http://localhost:3000", "https://www.example.com" ],

  // JWT（宿主模块手动 Configure 时读取的 key 约定，非框架强制）
  "Jwt": { "Issuer": "...", "Audience": "...", "SecretKey": "..." },

  // 多级缓存调优（可选，Cike.Caching）
  "MultilevelCache": { /* MultilevelCacheGlobalOptions */ }
}
```

### 8.11 锁（Cike.Locks / Cike.Locks.DistributedRedis）

```csharp
public interface ILock
{
    IDisposable? TryGet(string key, TimeSpan timeout = default);            // 拿到锁返回 IDisposable，拿不到返回 null
    Task<IAsyncDisposable?> TryGetAsync(string key, TimeSpan timeout = default, CancellationToken ct = default);
}

// 用法
var locker = serviceProvider.GetRequiredService<ILock>();
await using var handle = await locker.TryGetAsync($"order:{orderId}", TimeSpan.FromSeconds(5));
if (handle is null) throw new UserFriendlyException("操作正在处理中，请稍后重试。");
// ...受保护的操作
```

两个实现：`LocalLock`（进程内 SemaphoreSlim，依赖 `CikeLocksModule`）/ `DistributedRedisLock`（Medallion.Redis，需在配置中提供 `CikeRedisDistributedLockOptions.RedisConnectionString`）。

【陷阱】
1. **本地锁当前是"一把锁只能用一次"**：释放时会 Dispose 缓存的信号量（key 缓存不随之移除），同 key 第二次获取会抛 `ObjectDisposedException`——用前先验证此行为或换分布式实现
2. **同时依赖两个 Locks 模块时，`ILock` 默认解析到 LocalLock**（接口注册后写者胜 + 模块加载顺序"依赖在后注册"），分布式锁静默不生效；需要时用 `[Dependency(ReplaceServices = true)]` 显式替换

---

## 9. Recipe：新增一个业务功能的完整步骤

以"新增聚合根 `Todo`"为例。**严格按顺序创建以下 8 个文件**，每个文件的完整代码如下。

**第 1 步：实体** — `Domain/Data/Entities/Todo.cs`

```csharp
namespace Cike.Workflow.Domain.Data.Entities;

public class Todo : FullAuditedAggregateRoot<long>, IMultiTenant
{
    public long TenantId { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDone { get; set; }
}
```

**第 2 步：Store 接口** — `Domain/Data/ITodoStore.cs`

```csharp
namespace Cike.Workflow.Domain.Data;

public interface ITodoStore : IBaseStore<Todo> { }
```

**第 3 步：缓存模型**（仅当该聚合需要缓存；不需要可跳过，第 7 步 Store 改继承 `BaseStore<Todo>`）— `*.Caching/Models/TodoCacheModel.cs`

```csharp
namespace Cike.Workflow.Caching.Models;

public class TodoCacheModel : FullAuditedEntityDto<long>, IMultiTenant
{
    public long TenantId { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDone { get; set; }
}
```

**第 4 步：Dto** — `Application.Contracts/Todos/AddTodoDto.cs`、`TodoDetailDto.cs`

```csharp
public class AddTodoDto
{
    public string Title { get; set; } = null!;
}

public class TodoDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDone { get; set; }
}
```

**第 5 步：Command / Query** — `Application/Todos/Commands/AddTodoCommand.cs`、`Queries/GetTodoQuery.cs`

```csharp
public record AddTodoCommand(AddTodoDto Dto) : Command
{
    public long Id { get; set; }
}

public record GetTodoQuery(long Id) : Query<TodoDetailDto>;
```

**第 6 步：Handler** — `Application/Todos/TodoCommandHandler.cs`、`TodoQueryHandler.cs`

```csharp
public class TodoCommandHandler(ITodoStore todoStore)
{
    [LocalEventHandler]
    public async Task AddAsync(AddTodoCommand command, CancellationToken cancellationToken = default)
    {
        var entity = command.Dto.Adapt<Todo>();
        await todoStore.AddAsync(entity, cancellationToken);
        command.Id = entity.Id;
    }
}

public class TodoQueryHandler(ITodoStore todoStore)
{
    [LocalEventHandler]
    public async Task GetAsync(GetTodoQuery query, CancellationToken cancellationToken = default)
    {
        var entity = await todoStore.FindAsync(query.Id, cancellationToken)
            ?? throw new UserFriendlyException("待办事项不存在，请检查后重试。");
        query.Result = entity.Adapt<TodoDetailDto>();
    }
}
```

**第 7 步：Store 实现 + DbContext** — `EntityFrameworkCore/Stores/TodoStore.cs`，并在 `CikeWorkflowDbContenxt` 加一行 `public DbSet<Todo> Todos { get; set; }`

```csharp
public class TodoStore(CikeWorkflowDbContenxt context) : BaseStore<Todo>(context), ITodoStore;
```

**第 8 步：Service 端点** — `Service.Open/Services/TodoService.cs`

```csharp
[AutoValidation]
public class TodoService : MinimalApiServiceBase
{
    public async Task<Results<Ok<long>, BadRequest>> AddAsync(
        [FromServices] ILocalEventBus localEventBus,
        AddTodoDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddTodoCommand(dto);
        await localEventBus.PublishAsync(command, cancellationToken);
        return TypedResults.Ok(command.Id);
    }

    public async Task<Results<Ok<TodoDetailDto>, BadRequest>> GetAsync(
        [FromServices] ILocalEventBus localEventBus,
        long id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTodoQuery(id);
        await localEventBus.PublishAsync(query, cancellationToken);
        return TypedResults.Ok(query.Result);
    }
}
```

**第 9 步：验证 + 迁移**

- 需要参数校验时加 `Application/Todos/Validators/AddTodoDtoValidator.cs`（依赖了 `CikeFluentValidationModule` 即自动注册生效，无额外动作）
- 执行 `dotnet ef migrations add AddTodo` + `dotnet ef database update`

### 扩展 Recipe：分页列表查询

列表查询固定模式（Query 携带 `PagedAndSortedResultRequest`，返回 `PagedResultDto<T>`）：

```csharp
// 1. Query 定义 —— Application/Todos/Queries/GetPagedTodoListQuery.cs
public record GetPagedTodoListQuery(string? Keyword, PagedAndSortedResultRequest PageDto)
    : Query<PagedResultDto<TodoItemDto>>;

// 2. Handler —— TodoQueryHandler 中追加
[LocalEventHandler]
public async Task GetPagedListAsync(GetPagedTodoListQuery query, CancellationToken cancellationToken = default)
{
    var queryable = todoStore.Queryable.AsNoTracking()
        .WhereIf(!string.IsNullOrEmpty(query.Keyword), x => x.Title.Contains(query.Keyword!));

    var (total, items) = await todoStore.ToPaginationAsync(queryable, query.PageDto, cancellationToken);

    query.Result = new PagedResultDto<TodoItemDto>
    {
        Total = total,
        Items = items.Adapt<List<TodoItemDto>>()
    };
}

// 3. Service 端点 —— TodoService 中追加（真实模式：GET + [AsParameters] 查询串绑定）
public async Task<Results<Ok<PagedResultDto<TodoItemDto>>, BadRequest>> GetPagedListAsync(
    [FromServices] ILocalEventBus localEventBus,
    string? keyword,
    [AsParameters] PagedAndSortedResultRequest pageDto,      // [AsParameters] 把 DTO 展开成查询串参数
    CancellationToken cancellationToken = default)
{
    var query = new GetPagedTodoListQuery(keyword, pageDto);
    await localEventBus.PublishAsync(query, cancellationToken);
    return TypedResults.Ok(query.Result);
}
// 生成路由：GET api/v1/Todos/PagedList?keyword=&page=1&pageSize=10&sorting=
```

注意：
- 分页端点标准写法是 **GET + `[AsParameters]`**（复杂 DTO 的属性逐一映射为查询串参数），参考 `Cike.Workflow` 的 `WorkspaceService.GetPagedListAsync`
- `Sorting` 使用 System.Linq.Dynamic 语法（`"CreatedAt desc"`、`"Name"`），由分页扩展透传给 `OrderBy`。**不要**让前端传任意 Sorting 字符串直接进查询——先在 Handler 里白名单化

**验收清单**（AI 自查）：
- [ ] 实体继承了正确基类并实现 `IMultiTenant`（多租户项目）
- [ ] `_Imports.cs`：新命名空间是否需要补 global using（Command/Query/Dto 所在目录）
- [ ] Handler 方法有 `[LocalEventHandler]`，参数里有 Command/Query 类型
- [ ] Service 方法名前缀与期望 HTTP 动词一致（见 6.2 表）
- [ ] Scoped 依赖（EventBus、Store）在 Service 中用 `[FromServices]` 方法参数注入
- [ ] 没有手写任何 `services.AddXxx<业务类>()`

---

## 10. 隐式行为清单（汇总）

| # | 行为 | 触发条件 | 后果 |
|---|---|---|---|
| 1 | 程序集自动 DI 注册 | 类实现 `ISingleton/IScoped/ITransientDependency` | 自动注册自身+所有接口/基类；**接口一律 Singleton** |
| 2 | Handler 自动注册 | 方法标注 `[LocalEventHandler]` | 所在类 AddScoped，按事件类型建索引 |
| 3 | MinimalAPI 自动路由 | 类继承 `MinimalApiServiceBase` | 按命名约定生成 `api/v1/...` 路由 |
| 4 | 雪花 Id 生成 | `IEntity<long>.Id == 0` 的实体被跟踪（Add） | 跟踪时自动赋雪花 Id |
| 5 | 审计字段填充 | 实体实现 `IAuditedEntity` | Added 填 CreatedAt/CreatedBy；Modified 填 UpdatedAt/UpdatedBy |
| 6 | 软删 | 实体实现 `ISoftDelete` | Delete 变为 IsDeleted=true；查询自动过滤 IsDeleted |
| 7 | 多租户过滤 | 实体实现 `IMultiTenant` | 插入自动填 TenantId；查询自动 `TenantId == CurrentUser.TenantId` |
| 8 | 事务包裹 | 每次**根** `PublishAsync`（嵌套发布共享同一事务） | 事件树成功 Commit / 失败 Rollback |
| 9 | 领域事件 | 聚合根 `AddDomainEvent(...)` | SaveChanges 时入队 `IQueueEventBus`，UoW Commit 前按队列发布 |
| 10 | 业务异常转 400 | 抛 `UserFriendlyException`/`BusinessException` | `BusinessExceptionMiddleware` 返回 BadRequest+Message |
| 11 | Long→string JSON | 所有 HTTP 响应 | 前端收到的 long 是字符串 |
| 12 | 乐观并发 | 实体继承 `IHasConcurrencyStamp` | 修改时自动刷新 ConcurrencyStamp |
| 13 | 后台事件 | 事件继承 `BackgroundEvent` 或 `EnableBackgroundThread()` | 走 Channel，`Parallel.ForEachAsync` + 独立 Scope 消费，异常仅记日志 |
| 14 | Saga 补偿 | `[LocalEventHandler(IsCancel = true)]` | 同事件正常 handler 失败时按 Order 倒序补偿 |
| 15 | 领域事件 | 聚合根 `AddDomainEvent(...)` | SaveChanges 时入队，事务提交前由 `IQueueEventBus` 依次发布 |
| 16 | Validator 自动注册 | 依赖 `CikeFluentValidationModule` | 所有模块程序集的 `AbstractValidator<T>` 全部注册 |
| 17 | 事件中间件管线 | 每次 `PublishAsync` | 按 `ILocalEventMiddleware<TEvent>` 注册顺序包裹（事务→异常→防递归） |

## 11. 反模式（不要做）

1. **不要**手写 `services.AddSingleton/AddScoped/AddTransient` 注册业务类——用标记接口或让框架机制发现它
2. **不要**手写 `app.MapGet/MapPost`——继承 `MinimalApiServiceBase` 靠命名约定生成路由
3. **不要**在 MinimalAPI Service 的构造函数注入 Scoped 服务（EventBus/Store/DbContext）——必须 `[FromServices]` 方法参数
4. **不要**在 Handler 里手动管理事务——`PublishAsync` 的事务中间件已覆盖
5. **不要**给新增实体手工赋 Id 或手工填审计字段/`TenantId`——框架自动完成
6. **不要**在查询里手写 `TenantId == ...` 或 `IsDeleted == false`——全局过滤器已带上
7. **不要**在非宿主项目引用 `Cike.AspNetCore.*`（MinimalAPIs/Swagger）——HTTP 只属于 Service.Open 层
8. **不要**新建 Command/Query 时忘记继承 `Command` / `Query<TResult>`——不继承就不是 `IEvent`，发布时被静默忽略（无 handler 命中）
9. **不要**让 Application 层直接依赖 `XxxDbContext`——一律走 `IXxxStore`（Workflow 分层规约；框架 sample 有反例，不采用）
10. **不要**在 DTO 之外的地方引入 HTTP 概念（如 `Results`、`[FromServices]`）到 Application/Domain 层
11. **不要**用后台事件实现需要"全成功或全回滚"的流程——后台事件无事务保障、异常不会通知发布方（见 5.5）
12. **不要**手写 `AddValidatorsFromAssembly`——`CikeFluentValidationModule` 已自动扫描所有模块程序集，重复注册只会造成解析混乱
13. **不要**手写 `AddCikeDbContext` 之外的 DbContext 注册方式（`AddDbContext/AddDbContextPool` 会绕开框架的连接串解析与 UoW 集成）
14. **不要**在引用旧版 NuGet 包（`FailureLevel` 默认值修正前）的项目里使用 `IsCancel` 时省略 `FailureLevel = FailureLevelEnum.Throw`（见 5.6）
15. **不要**在应用模块里 `Configure<CikeDbContextOptions>`——会被后执行的 Provider 模块整体覆盖（见 7.2）
16. **不要**依赖"事件树执行成功但事务提交失败时自动触发取消 handler"——该分支是框架里的死代码（`Status` 从未被置为 `Succeed`，见 5.6）

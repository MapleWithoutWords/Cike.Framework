# Cike.Core

框架内核：模块系统、依赖注入约定、异常体系与基础工具。所有 Cike 包都直接或间接依赖它，**它自己不是模块**（定义模块机制本身）。

## 提供的能力

### 模块系统（`Cike.Core.Modularity`）
| 类型 | 说明 |
|---|---|
| `CikeModule` | 模块基类，生命周期：`ConfigureServicesAsync`（注册服务）→ `InitializeAsync`（应用初始化）→ `ShutdownAsync`（停止）+ `Dispose` |
| `[DependsOn]` | 声明模块依赖，递归收集、自动去重，可标注多次 |
| `CikeModuleContainer` | 已加载模块容器（`ModuleTypes` + 实例列表） |
| `ModuleLoader` / `ModularityFactory` | 模块树收集与加载入口 |
| `ServiceConfigurationContext` | 注册阶段上下文（`.Services`，`GetConfiguration()`） |
| `ApplicationInitializationContext` | 初始化阶段上下文（`.ServiceProvider`；`GetApplicationBuilder()` 等扩展由 AspNetCore 包提供） |
| `AddApplicationAsync<TStartupModule>()` | 宿主引导第一行：加载模块树 + DI 约定扫描 + 按序 ConfigureServices |

模块执行顺序：`ConfigureServicesAsync` 从启动模块开始、由上层到底层串行执行；`InitializeAsync` 各模块并发（`Task.WhenAll`）。

### 依赖注入约定（`Cike.Core.DependencyInjection`）
- 标记接口：`ISingletonDependency` / `IScopedDependency` / `ITransientDependency`（实现在所有已加载模块程序集内自动注册）
- `[Dependency(Key, ReplaceServices, TryRegister)]`：keyed 注册 / 替换注册

【陷阱】自动注册把实现类的**所有接口和基类**统一以 **Singleton** 生命周期注册（硬编码），与类自身标记的生命周期无关。

### 异常（`Cike.Core.Exceptions`）
- `BusinessException`（Code/Message/Details/LogLevel）
- `UserFriendlyException : BusinessException`——业务校验失败的推荐抛法，HTTP 层自动转 400

### 其他
`IObjectAccessor`/`ObjectAccessor`（跨阶段传递 `IApplicationBuilder` 等）、`LazyManualMemoryCache`、`AsyncHelper`、字符串/类型/JSON/集合扩展（`RemovePostFix`、`Adapt` 辅助等）、`Hasher`。

## 更多

完整开发规范（模块系统细节、DI 约定、陷阱清单）：[AI 开发指南](../../docs/AI-GUIDE.md) 第 3、4 节。

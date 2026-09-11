---
title: Cike.Data.Domain 仓储能力
labels: [ready-for-agent]
---

# Cike.Data.Domain 仓储能力

## Problem Statement

框架目前没有仓储（Repository）抽象。业务代码访问数据只能直接注入并操作 `CikeDbContext<TDbContext>`，导致：

- 业务层与 EF Core 直接耦合，无法面向持久化无关的抽象编程，也难以在测试中替换数据访问
- 查询、分页、软删、审计等逻辑散落在各业务服务中重复编写
- 领域层（`Cike.Data.Domain`）只有实体基类（`Entity` / `AggregateRoot` / 审计基类），缺少配套的数据访问抽象，领域建模能力不完整

## Solution

为框架补齐仓储能力，分两层：

- **领域层**（`Cike.Data.Domain`）提供持久化无关的三层仓储接口：`IReadOnlyRepository`（查询）→ `IBasicRepository`（+增删改）→ `IRepository`（合并标记接口）。业务代码注入接口即可完成增删改查、分页、条件查询
- **EF Core 层**（`Cike.Data.EFCore`）提供默认实现 `EfCoreRepository<TDbContext, TEntity, TKey>`，复用框架现有的审计属性填充、软删除、多租户过滤、主键生成、领域事件与分页设施；调用 `AddCikeDbContext<TDbContext>()` 时自动为所有 DbSet 实体注册默认仓储，开箱即用

## User Stories

1. 作为业务开发者，我想注入 `IRepository<TEntity, TKey>` 就能对聚合做增删改查，这样业务层不再直接依赖 `DbContext`
2. 作为业务开发者，我想用 `FindAsync` 查询单个实体（不存在时返回 null），这样"可能不存在"的场景不需要靠异常做控制流
3. 作为业务开发者，我想用 `GetAsync` 查询单个实体（不存在时抛出用户友好的异常），这样"必须存在"场景的错误行为在全框架统一
4. 作为业务开发者，我想用 `GetListAsync(predicate)` 按条件查列表，这样常见过滤查询不用手写 LINQ 样板
5. 作为业务开发者，我想用 `GetPagedListAsync` 配合 `IPagedAndSortedRequest` 一次拿到分页数据与总数，这样列表页查询一步到位
6. 作为业务开发者，我想在分页查询里传可选的过滤条件，这样分页与过滤可以一次调用完成
7. 作为业务开发者，我想用 `GetCountAsync` / `AnyAsync` 做计数与存在性判断，这样不需要把全量数据拉到内存
8. 作为业务开发者，我想插入实体时主键自动生成（long 雪花 / Guid），这样不用手动管理 ID
9. 作为业务开发者，我想增删改时审计字段（CreatedAt / CreatedBy / UpdatedAt / UpdatedBy）自动填充，这样不用在每个写路径手写
10. 作为业务开发者，我想删除 `ISoftDelete` 实体时自动转为软删除（IsDeleted=true 且后续查询自动过滤），这样误删可恢复
11. 作为业务开发者，我想多租户实体自动按当前租户过滤，这样跨租户数据不会泄漏
12. 作为业务开发者，我想聚合根上通过 `AddDomainEvent` 挂载的领域事件在保存时自动发布，这样事件机制对仓储使用方透明
13. 作为业务开发者，我想用 `autoSave = false` 配合工作单元让一个业务用例内的多次写操作统一提交，这样多个写入是一个事务
14. 作为业务开发者，我想用 `InsertManyAsync` / `UpdateManyAsync` / `DeleteManyAsync` 做批量写，这样批量场景不用循环单条
15. 作为业务开发者，我想按 Id 删除且实体不存在时静默返回，这样删除操作天然幂等
16. 作为 CQRS 查询侧开发者，我想只注入 `IReadOnlyRepository`（接口上只有查询方法），这样查询侧在编译期就无法执行写操作
17. 作为领域建模者，我想继承 `EfCoreRepository` 编写自定义仓储并暴露领域语义的查询方法，这样复杂业务查询有清晰的归属而不是散落各处
18. 作为框架使用者，我想调用 `AddCikeDbContext<TDbContext>()` 后无需任何额外配置即可注入任意实体的仓储，这样默认体验开箱即用
19. 作为框架使用者，我想自定义仓储自动覆盖同一实体类型的默认仓储，这样业务定制不会被默认实现挡住
20. 作为业务开发者，我想通过 `GetQueryableAsync` 扩展拿到 `IQueryable` 做 Include 和复杂查询，这样仓储不会成为查询表达力的天花板
21. 作为业务开发者，我想通过 `GetDbContextAsync` 拿到强类型 `TDbContext`，这样仓储覆盖不到的 EF 能力（原生 SQL、Include 链等）仍可触达
22. 作为库维护者，我想仓储抽象放在领域层且不依赖 EF Core，这样未来可以新增 Dapper 等其他实现而不动业务代码
23. 作为库维护者，我想仓储实现完全复用 `CikeDbContext` 现有钩子（审计/软删/并发戳/领域事件），这样已有项目升级后行为无变化
24. 作为库维护者，我想 `AddCikeDbContext` 的新参数带默认值，这样已有调用方零改动（向后兼容）
25. 作为测试工程师，我想从 DI 容器解析仓储做集成测试，这样测试走的是和生产完全相同的注册链路

## Implementation Decisions

- **三层接口链**：`IReadOnlyRepository<TEntity, TKey>`（纯查询）→ `IBasicRepository<TEntity, TKey>`（继承查询，追加增删改）→ `IRepository<TEntity, TKey>`（合并标记接口）。注入 `IRepository` 即全能力；查询侧可只注入 `IReadOnlyRepository` 实现编译期只读
- **分层位置**：抽象在 `Cike.Data.Domain`（命名空间 `Cike.Domain.Repositories`），实现 `EfCoreRepository<TDbContext, TEntity, TKey>` 在 `Cike.Data.EFCore`
- **实体约束**：`where TEntity : class, IEntity<TKey>`，不要求 `IAggregateRoot`（支持非聚合根实体）
- **autoSave 语义**：所有写方法带 `autoSave` 参数，默认 `true`（立即 SaveChanges）；配合工作单元时显式传 `false`，由事务提交统一保存
- **查询语义约定**：`GetAsync` 未找到抛 `UserFriendlyException`（与框架现有 IQueryable 扩展的 "Id {id} is NotFound." 行为一致）；`FindAsync` 返回 null；`DeleteAsync(id)` 实体不存在时静默返回（幂等）
- **分页返回**：`(long Total, List<TEntity> Items)` 元组，复用现有 `ToPaginationAsync`，不新造 PagedResult 类型
- **默认仓储注册**：扩展 `AddCikeDbContext`（新增参数 `addDefaultRepositories`，默认 true，向后兼容），利用现有 `GetEntityTypes` + 领域层 `EntityHelper.FindPrimaryKeyType`，为每个实现了 `IEntity<TKey>` 的 DbSet 实体注册 closed-generic 实现到全部三个接口（Scoped）
- **自定义仓储**：继承 `EfCoreRepository` + 实现业务接口 + 标注 `IScopedDependency`，由现有模块约定扫描（ModularityFactory）自动注册到其全部接口上，天然覆盖默认仓储，框架无需为此新增机制
- **EfCoreRepository 不实现任何 DI 标记接口**，避免被约定扫描重复注册；默认仓储由注册扩展显式 AddScoped
- **横切能力零重复实现**：软删转换、审计填充、主键生成、多租户过滤、领域事件入队全部由 `CikeDbContext` 现有 ChangeTracker 钩子完成；仓储的删除就是 `Remove`，软删由钩子自动转换
- **复杂查询出口**：EF Core 层提供 `GetQueryableAsync` / `GetDbContextAsync` 扩展方法，通过一个 EF Core 层的仓储-DbContext 访问器接口实现（规避开放泛型模式匹配问题）
- **多 DbContext 同实体**：后注册者覆盖（Add 语义），文档注明即可
- **命名空间**：领域层 `Cike.Domain.Repositories`；EF Core 层 `Cike.Data.EFCore.Repositories`

## Testing Decisions

- **唯一接缝**：从 DI 容器解析 `IRepository<TEntity, TKey>` —— 测试走完整的模块加载 + `AddCikeDbContext` 自动注册链路，与生产一致
- 新建 xUnit 测试项目，用 SQLite in-memory provider 的测试 DbContext + 假基础设施（FakeCurrentUser 等）组装容器
- **好测试的标准**：只断言仓储公开 API 的外部可观察行为——CRUD 结果、分页元组的 Total/Items、未找到时的异常类型与消息、软删后实体仍可查且计数正确、审计字段值、默认仓储可解析且实现类型正确、自定义仓储覆盖默认仓储；不 mock、不测私有方法、不绕过容器直接构造实现
- 仓库现有测试几乎空白（tests 下仅有 MinimalAPIs 的 Web 宿主型工程和一个控制台临时工程，均无数据层测试），本项目将作为后续数据层测试的模板
- 测试项目内需要假实现（FakeCurrentUser / 测试 DbContext / 测试实体），属于测试脚手架而非生产代码

## Out of Scope

- 导航属性加载 API（includeDetails / WithDetails）——用 `GetQueryableAsync().Include(...)` 替代
- `IRepository<TEntity>`（Guid 默认键）便捷变体——框架主用 long 雪花 ID
- `DeleteAsync(predicate)` 条件批量删除——用 `GetListAsync` + `DeleteManyAsync` 组合
- 基于 `ConnectionStringName` 的多连接串仓储路由
- `samples/` 目录改动（正在重构中）
- Dapper 或其他 ORM 的仓储实现
- CQRS 意义上的独立读模型（读写分离存储）

## Further Notes

- 设计已与仓库所有者确认：抽象+实现分离、自动注册默认仓储、ABP 三层接口、测试接缝为"仓储接口 + DI 容器"、spec 存本地 markdown
- `Cike.Data.EFCore` 内已有 internal `EntityHelper` 单例与领域层静态 `EntityHelper` 同名，实现时需注意消歧（全限定名或别名）
- 现有 `IQueryablePaginationExtensions` 文件位于 Cike.Data.EFCore 项目但命名空间是 `Cike.Contracts.Extensions`，历史遗留，本次不改
- 领域事件经 `IQueueEventBus` 入队、由工作单元提交时发布是现有机制，仓储不改变其时序

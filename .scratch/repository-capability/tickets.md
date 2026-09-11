# Tickets: Cike.Data.Domain 仓储能力

为框架补齐仓储能力：领域层三层仓储抽象 + EF Core 默认实现 + 默认仓储自动注册 + IQueryable 出口。来源 spec：[.scratch/repository-capability/spec.md](spec.md)。

Work the **frontier**: any ticket whose blockers are all done. 依赖图 `1 → 2 → {3, 4, 5} → 6`，工单 3/4/5 相互独立可并行。

---

## 1. 仓储测试基座（容器组装 + SQLite in-memory）

**What to build:** 后续所有仓储工单共用的唯一测试接缝：一个能完整走模块加载链路组装出 DI 容器的 xUnit 测试项目。容器内有假当前用户、SQLite in-memory 的测试 DbContext（含普通/审计/软删/多租户测试实体），能解析出 DbContext 与工作单元并完成基本读写。跑一次测试全绿即证明基座可用。

**Blocked by:** None — can start immediately.

- [x] 新 xUnit 测试项目加入解决方案，引用 Cike.Data.EFCore
- [x] 通过模块加载（与生产一致的方式）组装容器，提供 FakeCurrentUser 等假基础设施
- [x] 测试 DbContext 使用 SQLite in-memory，含覆盖各能力的测试实体（普通、审计、软删、多租户、带导航属性）
- [x] 容器可解析 TDbContext 与 IUnitOfWork，SaveChanges 基本读写可用
- [x] `dotnet test` 全绿

## 2. 只读仓储首条贯通线（按 Id 查询 + 默认注册）

**What to build:** 第一条 tracer bullet：从领域层接口到 EF Core 实现再到自动注册的完整贯通。框架使用者在调用 `AddCikeDbContext` 后，无需任何配置即可从容器注入只读仓储并按 Id 查单个实体。

**Blocked by:** 1. 仓储测试基座（容器组装 + SQLite in-memory）

- [x] `Cike.Data.Domain` 新增 `IReadOnlyRepository<TEntity, TKey>`（本票仅含 GetAsync/FindAsync）
- [x] `Cike.Data.EFCore` 新增 `EfCoreRepository<TDbContext, TEntity, TKey>` 实现 GetAsync/FindAsync，并暴露 DbContext 访问能力（访问器接口）
- [x] `AddCikeDbContext` 新增默认仓储注册参数（向后兼容），为每个实现了 `IEntity<TKey>` 的 DbSet 实体注册到只读仓储接口
- [x] 测试：从容器解析 `IReadOnlyRepository<TEntity, long>`，实现类型为 EfCoreRepository；GetAsync 命中返回实体；FindAsync 未找到返回 null；GetAsync 未找到抛 UserFriendlyException
- [x] 注意与项目内已有同名 EntityHelper 消歧

## 3. 条件列表、分页与计数查询

**What to build:** 只读仓储的完整查询面：条件列表、分页排序（一次返回数据与总数）、计数与存在性判断。列表页场景一次调用完成分页+过滤。

**Blocked by:** 2. 只读仓储首条贯通线（按 Id 查询 + 默认注册）

- [x] `IReadOnlyRepository` 增加 GetListAsync（无参/带谓词）、GetPagedListAsync（IPagedAndSortedRequest + 可选谓词）、GetCountAsync（无参/带谓词）、AnyAsync（可选谓词），EfCoreRepository 同步实现
- [x] 分页返回 (Total, Items) 元组，复用现有分页扩展；Sorting 字符串排序生效；PageSize/Page 边界行为正确
- [x] 谓词版本只返回匹配行；GetCountAsync/AnyAsync 不拉取全量数据
- [x] 多租户实体经仓储查询时按当前用户租户过滤（假当前用户切换租户验证）

## 4. 写入路径与 autoSave 语义

**What to build:** 仓储的完整写入能力：三层接口的写入半边。插入自动生成主键与审计字段，软删实体删除后数据仍在但查询不可见，autoSave 可控保存时机，领域事件保存后自动入队。

**Blocked by:** 2. 只读仓储首条贯通线（按 Id 查询 + 默认注册）

- [x] `IBasicRepository`（继承只读 + 写方法）与 `IRepository`（合并标记）接口及实现：Insert/Update/Delete 及各自 Many 变体、按 Id 删除
- [x] autoSave 默认 true 立即落库；传 false 时不落库直到显式 SaveChanges/工作单元提交
- [x] 插入 long 主键自动生成（雪花）；审计实体 CreatedAt/CreatedBy/UpdatedAt/UpdatedBy 自动填充
- [x] 软删实体 DeleteAsync 后 IsDeleted=true、查询与计数自动排除、数据库行仍存在
- [x] 按 Id 删除时实体不存在静默返回（幂等）
- [x] 聚合根挂载的领域事件在保存后入队（可通过测试替身观察）
- [x] 默认注册补全到 IRepository / IBasicRepository 两个接口

## 5. IQueryable 出口扩展

**What to build:** 仓储表达力的逃生舱：当仓储方法不够用时，通过扩展方法拿到 IQueryable 做导航属性加载与任意复杂查询，以及拿到强类型 DbContext 触达仓储未覆盖的 EF 能力。

**Blocked by:** 2. 只读仓储首条贯通线（按 Id 查询 + 默认注册）

- [x] `GetQueryableAsync` 扩展：从任意仓储接口实例取 IQueryable
- [x] `GetDbContextAsync<TDbContext>` 扩展：取强类型 DbContext
- [x] 测试：经 IQueryable 完成带 Include 的导航属性查询并物化结果；强类型 DbContext 获取后可执行原生 EF 操作
- [x] 全局查询过滤器（软删/多租户）在拿到的 IQueryable 上依然生效

## 6. 自定义仓储覆盖与文档收尾

**What to build:** 框架的扩展闭环与使用文档：业务方为特定聚合编写带领域语义的自定义仓储后自动注册并覆盖默认仓储；两个包的 README 让使用者知道全部能力怎么用。

**Blocked by:** 3. 条件列表、分页与计数查询；4. 写入路径与 autoSave 语义；5. IQueryable 出口扩展

- [x] 测试：自定义仓储（继承 EfCoreRepository + 业务接口 + IScopedDependency 约定）被自动注册，且解析同实体的 IRepository 返回自定义实现（覆盖默认）
- [x] Cike.Data.Domain README：三层接口说明、注入用法、读写分离注入建议
- [x] Cike.Data.EFCore README：默认注册参数、GetQueryableAsync/GetDbContextAsync 用法、自定义仓储写法、多 DbContext 同实体覆盖语义
- [x] 文档不引用具体文件路径，示例与最终 API 签名一致

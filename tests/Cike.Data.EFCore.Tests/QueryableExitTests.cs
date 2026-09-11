namespace Cike.Data.EFCore.Tests;

/// <summary>
/// 工单 5 验收：IQueryable 出口——GetQueryableAsync 做 Include/复杂查询、
/// GetDbContextAsync 取强类型 DbContext、全局过滤器（软删/多租户）在出口查询上依然生效。
/// </summary>
public class QueryableExitTests : IClassFixture<CikeEfCoreTestHost>
{
    private readonly CikeEfCoreTestHost _host;

    public QueryableExitTests(CikeEfCoreTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task GetQueryableAsync_Include_LoadsNavigationProperties()
    {
        var marker = Guid.NewGuid().ToString("N");
        var order = new Order { Title = $"order-{marker}" };
        order.Lines.Add(new OrderLine { ProductName = $"line-{marker}", Quantity = 2 });
        order.Lines.Add(new OrderLine { ProductName = $"line-{marker}", Quantity = 3 });
        await _host.SeedAsync(order);

        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Order, long>>();
        var queryable = await repository.GetQueryableAsync();

        var loaded = await queryable
            .Include(o => o.Lines)
            .Where(o => o.Title == $"order-{marker}")
            .FirstOrDefaultAsync();

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Lines.Count);
        Assert.All(loaded.Lines, l => Assert.Equal(loaded.Id, l.OrderId));
    }

    [Fact]
    public async Task GetDbContextAsync_ReturnsTypedDbContext_ForRawEfOperations()
    {
        var name = $"category-{Guid.NewGuid():N}";
        await _host.SeedAsync(new Category { Name = name });

        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Category, long>>();

        var dbContext = await repository.GetDbContextAsync<TestDbContext, Category, long>();

        Assert.IsType<TestDbContext>(dbContext);
        // 原生 SQL：仓储方法覆盖不到的 EF 能力
        var row = await dbContext.Categories
            .FromSqlRaw($"SELECT * FROM Categories WHERE Name = '{name}'")
            .SingleOrDefaultAsync();
        Assert.NotNull(row);
        Assert.Equal(name, row.Name);
    }

    [Fact]
    public async Task GetQueryableAsync_AppliesSoftDeleteFilter()
    {
        var title = $"order-{Guid.NewGuid():N}";
        var order = new Order { Title = title };
        await _host.SeedAsync(order);
        using (var scope = _host.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Order, long>>();
            await repository.DeleteAsync(order.Id);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }

        using var verifyScope = _host.CreateScope();
        var verifyRepository = verifyScope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Order, long>>();
        var queryable = await verifyRepository.GetQueryableAsync();

        // 软删过滤器在出口 IQueryable 上依然生效：已删实体不可见
        Assert.Equal(0, await queryable.CountAsync(o => o.Id == order.Id));
    }

    [Fact]
    public async Task GetQueryableAsync_AppliesMultiTenantFilter()
    {
        var marker = Guid.NewGuid().ToString("N");
        try
        {
            _host.SetTenant(1);
            await _host.SeedAsync(new TenantOrder { Title = $"order-{marker}-a" });
            _host.SetTenant(2);
            await _host.SeedAsync(new TenantOrder { Title = $"order-{marker}-b" });

            _host.SetTenant(1);
            using var scope = _host.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<TenantOrder, long>>();
            var queryable = await repository.GetQueryableAsync();

            var visible = await queryable.Where(o => o.Title.Contains(marker)).ToListAsync();
            Assert.Single(visible);
            Assert.EndsWith("-a", visible[0].Title);
        }
        finally
        {
            _host.SetTenant(null);
        }
    }
}

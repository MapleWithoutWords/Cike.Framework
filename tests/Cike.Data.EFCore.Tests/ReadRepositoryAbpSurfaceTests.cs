namespace Cike.Data.EFCore.Tests;

/// <summary>
/// 只读仓储 ABP 对齐面（用户反馈增量）：skip/take 分页、WithDetailsAsync 两种形态、
/// includeDetails 导航加载、GetQueryableAsync 接口成员。
/// </summary>
public class ReadRepositoryAbpSurfaceTests : IClassFixture<CikeEfCoreTestHost>
{
    private readonly CikeEfCoreTestHost _host;

    public ReadRepositoryAbpSurfaceTests(CikeEfCoreTestHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task GetPagedListAsync_SkipTakeSorting_ABStyle()
    {
        var prefix = await SeedCategoriesAsync(5);
        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Category, long>>();

        // 跳过 1 条取 2 条，按 Name 升序 → 第 2、3 条
        var page = await repository.GetPagedListAsync(1, 2, "Name", c => c.Name.StartsWith(prefix));

        Assert.Equal(2, page.Count);
        Assert.Equal($"{prefix}-01", page[0].Name);
        Assert.Equal($"{prefix}-02", page[1].Name);
    }

    [Fact]
    public async Task GetPagedListAsync_SkipTake_EmptySorting_Works()
    {
        var prefix = await SeedCategoriesAsync(3);
        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Category, long>>();

        var page = await repository.GetPagedListAsync(0, 2, string.Empty, c => c.Name.StartsWith(prefix));

        Assert.Equal(2, page.Count);
    }

    [Fact]
    public async Task WithDetailsAsync_NoArgs_IncludesAllNavigations()
    {
        var marker = Guid.NewGuid().ToString("N");
        var order = new Order { Title = $"order-{marker}" };
        order.Lines.Add(new OrderLine { ProductName = $"line-{marker}", Quantity = 1 });
        order.Lines.Add(new OrderLine { ProductName = $"line-{marker}", Quantity = 2 });
        await _host.SeedAsync(order);

        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Order, long>>();
        var queryable = await repository.WithDetailsAsync();

        var loaded = await queryable.FirstOrDefaultAsync(o => o.Title == $"order-{marker}");
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Lines.Count);
    }

    [Fact]
    public async Task WithDetailsAsync_PropertyPaths_IncludesSpecifiedNavigation()
    {
        var marker = Guid.NewGuid().ToString("N");
        var order = new Order { Title = $"order-{marker}" };
        order.Lines.Add(new OrderLine { ProductName = $"line-{marker}", Quantity = 1 });
        await _host.SeedAsync(order);

        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Order, long>>();

        var queryable = await repository.WithDetailsAsync(o => o.Lines);

        var loaded = await queryable.FirstOrDefaultAsync(o => o.Title == $"order-{marker}");
        Assert.NotNull(loaded);
        Assert.Single(loaded.Lines);
    }

    [Fact]
    public async Task GetAsync_IncludeDetails_LoadsNavigationByDefault()
    {
        var categoryName = $"category-{Guid.NewGuid():N}";
        var category = new Category { Name = categoryName };
        await _host.SeedAsync(category);
        var product = new Product { Name = $"product-{Guid.NewGuid():N}", CategoryId = category.Id };
        await _host.SeedAsync(product);

        using (var scope = _host.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Product, long>>();

            // 默认 includeDetails=true：导航属性已加载（无懒加载，非 null 即已物化）
            var withDetails = await repository.GetAsync(product.Id);
            Assert.NotNull(withDetails.Category);
            Assert.Equal(categoryName, withDetails.Category.Name);
        }

        // 显式关闭导航需在独立 scope 验证：同 scope 内前一步 Include 已跟踪 Category，
        // EF 关系修复（fixup）会把导航填上，与 includeDetails 无关
        using (var scope = _host.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Product, long>>();
            var withoutDetails = await repository.FindAsync(product.Id, includeDetails: false);
            Assert.NotNull(withoutDetails);
            Assert.Null(withoutDetails.Category);
        }
    }

    [Fact]
    public async Task GetListAsync_IncludeDetails_LoadsNavigation()
    {
        var categoryName = $"category-{Guid.NewGuid():N}";
        var productName = $"product-{Guid.NewGuid():N}";
        var category = new Category { Name = categoryName };
        await _host.SeedAsync(category);
        await _host.SeedAsync(new Product { Name = productName, CategoryId = category.Id });

        using var scope = _host.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IReadOnlyRepository<Product, long>>();

        var list = await repository.GetListAsync(p => p.Name == productName, includeDetails: true);

        var product = Assert.Single(list);
        Assert.NotNull(product.Category);
        Assert.Equal(categoryName, product.Category.Name);
    }

    /// <summary>插入 count 个唯一前缀、带序号的 Category 并提交，返回前缀。</summary>
    private async Task<string> SeedCategoriesAsync(int count)
    {
        var prefix = $"cat-{Guid.NewGuid():N}";
        var categories = Enumerable.Range(0, count)
            .Select(i => new Category { Name = $"{prefix}-{i:D2}" })
            .ToArray();
        await _host.SeedAsync(categories);
        return prefix;
    }
}

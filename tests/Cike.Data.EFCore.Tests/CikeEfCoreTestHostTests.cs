namespace Cike.Data.EFCore.Tests;

/// <summary>
/// 工单 1 验收：测试基座本身可用——容器按生产方式组装，
/// 可解析 DbContext 与工作单元，SaveChanges 基本读写、主键生成、审计填充正常。
/// </summary>
public class CikeEfCoreTestHostTests : IClassFixture<CikeEfCoreTestHost>
{
    private readonly CikeEfCoreTestHost _host;

    public CikeEfCoreTestHostTests(CikeEfCoreTestHost host)
    {
        _host = host;
    }

    [Fact]
    public void ResolveDbContext_FromContainer()
    {
        using var scope = _host.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        Assert.NotNull(dbContext);
    }

    [Fact]
    public async Task ResolveUnitOfWork_CanBeginAndCommitTransaction()
    {
        using var scope = _host.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.BeginTransactionAsync();
        Assert.True(unitOfWork.IsTransactionBegun);

        await unitOfWork.CommitAsync();
        Assert.Equal(UnitOfWorkCommitState.Committed, unitOfWork.CommitState);
    }

    [Fact]
    public async Task SaveChanges_PersistsAcrossScopes()
    {
        var name = $"category-{Guid.NewGuid():N}";
        using (var scope = _host.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            dbContext.Categories.Add(new Category { Name = name });
            await dbContext.SaveChangesAsync();
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }

        using (var scope = _host.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var found = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == name);
            Assert.NotNull(found);
        }
    }

    [Fact]
    public async Task SaveChanges_AutoGeneratesIdAndAuditFields()
    {
        var name = $"product-{Guid.NewGuid():N}";
        Product product;
        try
        {
            _host.CurrentUser.Id = "42";
            using var scope = _host.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var category = new Category { Name = $"category-{Guid.NewGuid():N}" };
            product = new Product { Name = name, Category = category };
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
        }
        finally
        {
            // 复位，避免污染类内共享的当前用户状态
            _host.CurrentUser.Id = null;
        }

        Assert.NotEqual(0L, product.Id);
        Assert.NotEqual(default, product.CreatedAt);
        Assert.Equal(42L, product.CreatedBy);
    }
}

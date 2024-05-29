using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Data;

public class CQRSDbContextFactory : IDesignTimeDbContextFactory<CQRSDbContext>
{
    public CQRSDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CQRSDbContext>();
        optionsBuilder.UseMySql("Server=localhost;Port=6031;Database=todoapp;Uid=root;Pwd=lfm@123;", ServerVersion.AutoDetect("Server=localhost;Port=6031;Database=todoapp;Uid=root;Pwd=lfm@123;"));

        return new CQRSDbContext(optionsBuilder.Options, new ServiceCollection().AddLogging().BuildServiceProvider(), false);
    }
}

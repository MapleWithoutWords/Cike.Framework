using CQRS.Domain.Buyers;
using CQRS.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace CQRS.EntityFrameworkCore;

public class CqrsDbContext(DbContextOptions<CqrsDbContext> options, IServiceProvider serviceProvider)
    : CikeDbContext<CqrsDbContext>(options, serviceProvider)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Buyer> Buyers => Set<Buyer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CqrsDbContext).Assembly);
    }
}

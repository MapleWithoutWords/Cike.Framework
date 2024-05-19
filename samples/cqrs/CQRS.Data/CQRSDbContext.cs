using Cike.Data.EFCore;
using CQRS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Data;

public class CQRSDbContext : CikeDbContext<CQRSDbContext>
{
    public CQRSDbContext(DbContextOptions<CQRSDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>(b =>
        {
            b.ToTable("Todo");
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).IsRequired().HasMaxLength(512);
        });
        base.OnModelCreating(modelBuilder);
    }
}

using Cike.Data.EFCore;
using CQRS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CQRS.Data;

public class CQRSDbContext : CikeDbContext<CQRSDbContext>
{
    public CQRSDbContext(DbContextOptions<CQRSDbContext> options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var longListToStringConverter = new ValueConverter<List<long>, string>(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList());
        modelBuilder.Entity<Todo>(b =>
        {
            b.ToTable("Todo");
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).IsRequired().HasMaxLength(512);
            b.Property(x => x.Tests).IsRequired().HasConversion(longListToStringConverter);
        });
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using CQRS.Domain.Orders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQRS.EntityFrameworkCore.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.OwnsOne(o => o.Address, a =>
        {
            a.Property(x => x.Province).HasMaxLength(50);
            a.Property(x => x.City).HasMaxLength(50);
            a.Property(x => x.Street).HasMaxLength(200);
            a.Property(x => x.ZipCode).HasMaxLength(10);
        });

        builder.HasMany(o => o.Lines).WithOne()
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using CQRS.Domain.Orders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQRS.EntityFrameworkCore.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.OwnsOne(l => l.UnitPrice, m => m.Property(x => x.Amount).HasPrecision(18, 2));
    }
}

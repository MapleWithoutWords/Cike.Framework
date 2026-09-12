using Microsoft.EntityFrameworkCore;
using CQRS.Domain.Buyers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQRS.EntityFrameworkCore.Configurations;

public class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.Property(b => b.TotalAmount).HasPrecision(18, 2);
    }
}

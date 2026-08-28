namespace Cike.Data.EFCore.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureByConvention(this EntityTypeBuilder b)
    {
        b.TryConfigureConcurrencyStamp();
        b.TryConfigureSoftDelete();
        b.TryConfigureMultiTenant();
        b.TryConfigureAudited();
    }

    public static void TryConfigureConcurrencyStamp(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<IHasConcurrencyStamp>())
        {
            b.Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                .IsConcurrencyToken()
                .HasMaxLength(40)
                .HasColumnName(nameof(IHasConcurrencyStamp.ConcurrencyStamp));
        }
    }

    public static void TryConfigureSoftDelete(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<ISoftDelete>())
        {
            b.Property(nameof(ISoftDelete.IsDeleted))
                .IsRequired()
                .HasColumnName(nameof(ISoftDelete.IsDeleted));
        }
    }

    public static void TryConfigureMultiTenant(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<IMultiTenant>())
        {
            b.Property(nameof(IMultiTenant.TenantId))
                .IsRequired(false)
                .HasColumnName(nameof(IMultiTenant.TenantId));
        }
    }

    public static void TryConfigureAudited(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<IAuditedEntity<Guid>>())
        {
            b.Property(nameof(IAuditedEntity<Guid>.CreatedAt))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.CreatedAt))
                .HasComment(nameof(IAuditedEntity<Guid>.CreatedAt));
            b.Property(nameof(IAuditedEntity<Guid>.CreatedBy))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.CreatedBy))
                .HasComment(nameof(IAuditedEntity<Guid>.CreatedBy));
            b.Property(nameof(IAuditedEntity<Guid>.UpdatedBy))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.UpdatedBy))
                .HasComment(nameof(IAuditedEntity<Guid>.UpdatedBy));
            b.Property(nameof(IAuditedEntity<Guid>.UpdatedAt))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.UpdatedAt))
                .HasComment(nameof(IAuditedEntity<Guid>.UpdatedAt));
        }
    }
}

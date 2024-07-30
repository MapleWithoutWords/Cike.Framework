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
            b.Property(nameof(IAuditedEntity<Guid>.CreateTime))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.CreateTime))
                .HasComment(nameof(IAuditedEntity<Guid>.CreateTime));
            b.Property(nameof(IAuditedEntity<Guid>.CreateUserId))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.CreateUserId))
                .HasComment(nameof(IAuditedEntity<Guid>.CreateUserId));
            b.Property(nameof(IAuditedEntity<Guid>.UpdateUserId))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.UpdateUserId))
                .HasComment(nameof(IAuditedEntity<Guid>.UpdateUserId));
            b.Property(nameof(IAuditedEntity<Guid>.UpdateTime))
                .IsRequired()
                .HasColumnName(nameof(IAuditedEntity<Guid>.UpdateTime))
                .HasComment(nameof(IAuditedEntity<Guid>.UpdateTime));
        }
    }
}

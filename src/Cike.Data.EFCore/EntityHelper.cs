namespace Cike.Data.EFCore;

internal class EntityHelper(ISnowflakeIdGenerator _snowflakeIdGenerator, IGuidGenerator _guidGenerator, ICurrentUser _currentUser) : ISingletonDependency
{
    public void SetAuditedProperty(EntityEntry entry)
    {
        var item = entry.Entity;
        if (item is IEntity<long> longIdEntity && longIdEntity.Id <= 0)
        {
            entry.Property(nameof(longIdEntity.Id)).CurrentValue = _snowflakeIdGenerator.NextId();
        }
        else if (item is IEntity<Guid> guidIdEntity && guidIdEntity.Id == Guid.Empty)
        {
            entry.Property(nameof(guidIdEntity.Id)).CurrentValue = _guidGenerator.Create();
        }
        if (item is IAuditedEntity<long> longAuditedEntity)
        {
            long.TryParse(_currentUser.Id, out var userId);
            if (longAuditedEntity.CreateTime <= DateTime.Now)
            {
                longAuditedEntity.CreateUserId = userId;
                longAuditedEntity.CreateTime = DateTime.Now;
            }
            if (longAuditedEntity.UpdateTime <= DateTime.Now)
            {
                longAuditedEntity.UpdateTime = DateTime.Now;
                longAuditedEntity.UpdateUserId = userId;
            }
        }
        else if (item is IAuditedEntity<Guid> guidAuditedEntity)
        {
            Guid.TryParse(_currentUser.Id, out var userId);
            if (guidAuditedEntity.CreateTime <= DateTime.Now)
            {
                guidAuditedEntity.CreateUserId = userId;
                guidAuditedEntity.CreateTime = DateTime.Now;
            }
            if (guidAuditedEntity.UpdateTime <= DateTime.Now)
            {
                guidAuditedEntity.UpdateTime = DateTime.Now;
                guidAuditedEntity.UpdateUserId = userId;
            }
        }
    }
}

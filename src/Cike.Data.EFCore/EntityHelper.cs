namespace Cike.Data.EFCore;

public class EntityHelper(ISnowflakeIdGenerator _snowflakeIdGenerator, IGuidGenerator _guidGenerator, ICurrentUser _currentUser) : ISingletonDependency
{
    public void SetAuditedProperty(IEnumerable<object> entities)
    {
        foreach (var item in entities)
        {
            if (item is IEntity<long> longIdEntity && longIdEntity.Id <= 0)
            {
                longIdEntity.Id = _snowflakeIdGenerator.NextId();
            }
            else if (item is IEntity<Guid> guidIdEntity && guidIdEntity.Id == Guid.Empty)
            {
                guidIdEntity.Id = _guidGenerator.Create();
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
}

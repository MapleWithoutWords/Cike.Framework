namespace Cike.Domain.Entities;

public abstract class Entity<TKey> : IEntity<TKey>
{
    public TKey Id { get; set; } = default!;

    public object[] GetKeys()
    {
        return [Id];
    }
}

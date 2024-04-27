namespace Cike.Data;

public interface IEntity<TKey>
{
    public TKey Id { get; set; }
}

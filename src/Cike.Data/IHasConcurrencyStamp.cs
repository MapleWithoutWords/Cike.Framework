namespace Cike.Data;

public interface IHasConcurrencyStamp
{
    string ConcurrencyStamp { get; set; }
}

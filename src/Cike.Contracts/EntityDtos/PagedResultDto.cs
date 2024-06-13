namespace Cike.Contracts.EntityDtos;

public class PagedResultDto<T>
{
    public PagedResultDto()
    {

    }
    public PagedResultDto(long total, List<T> items)
    {
        Total = total;
        Items = items;
    }

    public long Total { get; set; }

    public List<T> Items { get; set; } = new List<T>();
}

using Cike.Data;

namespace Cike.Contracts.EntityDtos;

public class PagedAndSortedRequestDto : PagedAndSortedResultRequest<PagedAndSortedRequestDto>
{
    public List<DynamicQuery> DynamicQuery { get; set; } = new();
}



public abstract class PagedAndSortedResultRequest<T> : FromUri<T>, IPagedAndSortedRequest where T : class, new()
{
    public virtual int PageIndex { get; set; }

    public virtual int PageSize { get; set; }

    public virtual string Sorting { get; set; } = null!;
}
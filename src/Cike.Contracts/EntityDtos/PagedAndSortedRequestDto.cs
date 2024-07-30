namespace Cike.Contracts.EntityDtos;

public abstract class PagedAndSortedResultRequest : IPagedAndSortedRequest
{
    public virtual int PageIndex { get; set; } = 1;

    public virtual int PageSize { get; set; } = 10;

    public virtual string? Sorting { get; set; } = null!;
}
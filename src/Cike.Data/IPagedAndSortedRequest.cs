namespace Cike.Data;

public interface IPagedAndSortedRequest
{
    int Page { get; set; }
    int PageSize { get; set; }
    string Sorting { get; set; }
}

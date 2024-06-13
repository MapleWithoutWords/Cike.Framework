namespace Cike.Data;

public interface IPagedAndSortedRequest
{
    int PageIndex { get; set; }
    int PageSize { get; set; }
    string Sorting { get; set; }
}

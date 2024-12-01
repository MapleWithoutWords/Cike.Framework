namespace OInchFan.Application.Subjects.CQRSApplications.Queries;

public record GetPagedCQRSApplicationQuery(string? Keyword, PagedAndSortedResultRequest Dto) : Query<PagedResultDto<CQRSApplicationItemDto>>
{
}

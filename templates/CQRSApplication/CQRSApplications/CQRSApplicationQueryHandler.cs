namespace OInchFan.Application.Subjects.CQRSApplications;

public class CQRSApplicationQueryHandler(OInchFanDbContext _oInchFanDbContext)
{
    [LocalEventHandler]
    public async Task GetPagedListAsync(GetPagedCQRSApplicationQuery query)
    {
        var pagedResult = await _oInchFanDbContext.CQRSApplications
            .WhereIf(!query.Keyword.IsNullOrEmpty(), e => e.Code.Contains(query.Keyword!) || e.Name.Contains(query.Keyword!))
            .ToPaginationAsync(query.Dto);

        query.Result = new PagedResultDto<CQRSApplicationItemDto>(pagedResult.Total, pagedResult.Items.Adapt<List<CQRSApplicationItemDto>>());
    }

    [LocalEventHandler]
    public async Task GetAsync(GetCQRSApplicationQuery queryCommand)
    {
        var CQRSApplication = await _oInchFanDbContext.CQRSApplications.GetAsync(queryCommand.Id);

        queryCommand.Result = CQRSApplication.Adapt<CQRSApplicationItemDto>();
    }
}

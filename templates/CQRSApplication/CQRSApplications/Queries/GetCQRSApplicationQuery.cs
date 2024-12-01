namespace OInchFan.Application.Subjects.CQRSApplications.Queries;

public record GetCQRSApplicationQuery(long Id) : Query<CQRSApplicationItemDto>
{
}

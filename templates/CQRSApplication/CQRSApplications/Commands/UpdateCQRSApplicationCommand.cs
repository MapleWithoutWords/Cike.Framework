namespace OInchFan.Application.Subjects.CQRSApplications.Commands;

public record UpdateCQRSApplicationCommand(long Id, AddUpdateCQRSApplicationDto Dto) : Command
{
}

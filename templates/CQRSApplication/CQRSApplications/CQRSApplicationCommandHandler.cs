using OInchFan.Application.Subjects.CQRSApplications.Commands;

namespace OInchFan.Application.Subjects.CQRSApplications;

public class CQRSApplicationCommandHandler(OInchFanDbContext _oInchFanDbContext)
{
    [LocalEventHandler]
    public async Task AddAsync(AddCQRSApplicationCommand command)
    {
        var entity = command.Dto.Adapt<CQRSApplication>();
        // Do some stuff

        await _oInchFanDbContext.CQRSApplications.AddAsync(entity);
        await _oInchFanDbContext.SaveChangesAsync();
    }

    [LocalEventHandler]
    public async Task UpdateAsync(UpdateCQRSApplicationCommand command)
    {
        var entity = await _oInchFanDbContext.CQRSApplications.GetAsync(command.Id);

        entity = command.Dto.Adapt(entity);
        // Do some stuff

        _oInchFanDbContext.CQRSApplications.Update(entity);
        await _oInchFanDbContext.SaveChangesAsync();
    }

    [LocalEventHandler]
    public async Task DeleteAsync(DeleteCQRSApplicationCommand command)
    {
        _oInchFanDbContext.Remove<CQRSApplication>(e => e.Id == command.Id);
        await _oInchFanDbContext.SaveChangesAsync();
    }
}

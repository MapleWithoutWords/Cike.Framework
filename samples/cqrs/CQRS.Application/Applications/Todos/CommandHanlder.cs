using Cike.EventBus.LocalEvent;
using CQRS.Application.Applications.Todos.Commands;
using CQRS.Data;
using CQRS.Data.Entities;
using Mapster;

namespace CQRS.Application.Applications.Todos;

public class CommandHanlder(CQRSDbContext _dbContext)
{
    [LocalEventHandler]
    public async Task CreateAsync(TodoCreateCommand command)
    {
        var todo = command.Dto.Adapt<Todo>();
        await _dbContext.AddAsync(todo);
    }

    [LocalEventHandler]
    public async Task UpdateAsync(TodoUpdateCommand command)
    {
        var todo = await _dbContext.Todos.FindAsync(command.Id);
        if (todo == null)
        {
            throw new Exception("Todo not found");
        }

        command.Dto.Adapt(todo);
        _dbContext.Update(todo);
    }

    [LocalEventHandler]
    public async Task DeleteAsync(TodoDeleteCommand command)
    {
        var todo = await _dbContext.Todos.FindAsync(command.Id);
        if (todo == null)
        {
            throw new Exception("Todo not found");
        }

        _dbContext.SoftDelete(todo);
    }
}

using Cike.Caching;
using Cike.EventBus.Local;
using CQRS.Application.Applications.Todos.Commands;
using CQRS.Data;
using CQRS.Data.Entities;
using Mapster;

namespace CQRS.Application.Applications.Todos;

public class CommandHanlder(CQRSDbContext _cqrsDbContext, IMultilevelCacheClient _multilevelCacheClient)
{
    [LocalEventHandler]
    public async Task CreateAsync(TodoCreateCommand command)
    {
        var todo = command.Dto.Adapt<Todo>();
        await _cqrsDbContext.AddAsync(todo);
        //await _multilevelCacheClient.SetAsync(todo.Id.ToString(), todo);
    }

    [LocalEventHandler]
    public async Task UpdateAsync(TodoUpdateCommand command)
    {
        var todo = await _cqrsDbContext.Todos.FindAsync(command.Id);
        if (todo == null)
        {
            throw new Exception("Todo not found");
        }

        command.Dto.Adapt(todo);
        _cqrsDbContext.Update(todo);
    }

    [LocalEventHandler]
    public async Task DeleteAsync(TodoDeleteCommand command)
    {
        var todo = await _cqrsDbContext.Todos.FindAsync(command.Id);
        if (todo == null)
        {
            throw new Exception("Todo not found");
        }

        _cqrsDbContext.Remove(todo);
    }
}

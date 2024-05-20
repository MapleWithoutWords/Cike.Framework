using Cike.Domain.Repositories;
using Cike.EventBus.LocalEvent;
using CQRS.Application.Applications.Todos.Commands;
using CQRS.Data;
using CQRS.Data.Entities;
using Mapster;

namespace CQRS.Application.Applications.Todos;

public class CommandHanlder(IRepository<Todo, Guid> _repository)
{
    [LocalEventHandler]
    public async Task CreateAsync(TodoCreateCommand command)
    {
        var todo = command.Dto.Adapt<Todo>();
        await _repository.AddAsync(todo);
    }

    [LocalEventHandler]
    public async Task UpdateAsync(TodoUpdateCommand command)
    {
        var todo = await _repository.FindAsync(command.Id);
        if (todo == null)
        {
            throw new Exception("Todo not found");
        }

        command.Dto.Adapt(todo);
        await _repository.UpdateAsync(todo);
    }

    [LocalEventHandler]
    public async Task DeleteAsync(TodoDeleteCommand command)
    {
        var todo = await _repository.FindAsync(command.Id);
        if (todo == null)
        {
            throw new Exception("Todo not found");
        }

        await _repository.DeleteAsync(todo);
    }
}

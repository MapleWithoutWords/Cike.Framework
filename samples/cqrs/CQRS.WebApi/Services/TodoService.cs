using Cike.AspNetCore.MinimalAPIs;
using Cike.EventBus.LocalEvent;
using CQRS.Application.Applications.Todos.Commands;
using CQRS.Application.Applications.Todos.Queries;
using CQRS.Application.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CQRS.WebApi.Services;

public class TodoService : MinimalApiServiceBase
{
    public async Task<Results<Ok<IEnumerable<TodoItemDto>>, NotFound>> GetListAsync([FromServices] ILocalEventBus localEventBus)
    {
        var query = new TodoGetListQuery();
        await localEventBus.PublishAsync(query);
        return TypedResults.Ok(query.Result);
    }

    public async Task CreateAsync(TodoCreateUpdateDto dto, [FromServices] ILocalEventBus localEventBus)
    {
        var command = new TodoCreateCommand(dto);
        await localEventBus.PublishAsync(command);
    }
}

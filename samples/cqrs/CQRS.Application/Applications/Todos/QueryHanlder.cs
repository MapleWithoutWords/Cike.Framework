using Cike.EventBus.Local;
using CQRS.Application.Applications.Todos.Queries;
using CQRS.Application.Dtos;
using CQRS.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Application.Applications.Todos;

public class QueryHanlder(CQRSDbContext _dbContext)
{
    [LocalEventHandler]
    public async Task GetAsync(TodoGetQuery query)
    {
        var todo = await _dbContext.Todos.FindAsync(query.Id);
        query.Result = todo == null ? null : todo.Adapt<TodoItemDto>();
    }

    [LocalEventHandler]
    public async Task GetListAsync(TodoGetListQuery query)
    {
        var todos = await _dbContext.Todos.AsNoTracking()
            .Where(e => e.Tests.Contains(1))
            .ToListAsync();



        query.Result = todos.Adapt<List<TodoItemDto>>();
    }
}

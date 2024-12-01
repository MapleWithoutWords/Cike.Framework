using Cike.Caching;
using Cike.EventBus.LocalEvent;
using CQRS.Application.Applications.Todos.Queries;
using CQRS.Application.Dtos;
using CQRS.Data;
using CQRS.Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Application.Applications.Todos;

public class QueryHanlder(CQRSDbContext _dbContext, IMultilevelCacheClient _multilevelCacheClient)
{
    [LocalEventHandler]
    public async Task GetAsync(TodoGetQuery query)
    {
        var todocache = await _multilevelCacheClient.GetAsync<Todo>(query.Id.ToString());
        var todo = await _dbContext.Todos.FindAsync(query.Id);
        query.Result = todo == null ? null : todo.Adapt<TodoItemDto>();
    }

    [LocalEventHandler]
    public async Task GetListAsync(TodoGetListQuery query)
    {
        var todos = await _dbContext.Todos.ToListAsync();
        query.Result = todos.Adapt<List<TodoItemDto>>();
    }
}

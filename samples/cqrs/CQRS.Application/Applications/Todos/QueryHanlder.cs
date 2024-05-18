using CQRS.Application.Applications.Todos.Queries;
using CQRS.Application.Dtos;
using CQRS.Data;
using CQRS.Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Application.Applications.Todos;

public class QueryHanlder(CQRSDbContext _dbContext)
{
    public async Task GetAsync(TodoGetQuery query)
    {
        var todo = await _dbContext.Todos.FindAsync(query.Id);
        query.Result = todo == null ? null : todo.Adapt<TodoItemDto>();
    }

    public async Task GetListAsync(TodoGetListQuery query, CancellationToken cancellationToken)
    {
        var todos = await _dbContext.Todos.ToListAsync();
        query.Result = todos.Adapt<List<TodoItemDto>>();
    }
}

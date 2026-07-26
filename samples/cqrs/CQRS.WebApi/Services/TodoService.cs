using Cike.AspNetCore.MinimalAPIs;
using Cike.Auth;
using Cike.EventBus.Local;
using CQRS.Application.Applications.Todos.Commands;
using CQRS.Application.Applications.Todos.Queries;
using CQRS.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CQRS.WebApi.Services;

public class TodoService : MinimalApiServiceBase
{
    public TodoService()
    {
        RouteOptions.RouteHandlerBuilder = routeHanlderBuilder =>
        {
            routeHanlderBuilder.RequireAuthorization("FrontendUser");
        };
    }

    public async Task<Results<Ok<IEnumerable<TodoItemDto>>, NotFound>> GetListAsync([FromServices] ILocalEventBus localEventBus, [FromServices] ICurrentUser currentUser, [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var userPrinal = httpContextAccessor.HttpContext?.User;
        var query = new TodoGetListQuery();
        await localEventBus.PublishAsync(query);
        return TypedResults.Ok(query.Result);
    }

    public async Task<Results<Ok<TodoItemDto>, NotFound>> GetAsync(Guid id, [FromServices] ILocalEventBus localEventBus)
    {
        var query = new TodoGetQuery(id);
        await localEventBus.PublishAsync(query);
        return query.Result == null ? TypedResults.NotFound() : TypedResults.Ok(query.Result);
    }

    public async Task CreateAsync(TodoCreateUpdateDto dto, [FromServices] ILocalEventBus localEventBus)
    {
        var command = new TodoCreateCommand(dto);
        await localEventBus.PublishAsync(command);
    }

    public async Task UpdateAsync(Guid id, TodoCreateUpdateDto dto, [FromServices] ILocalEventBus localEventBus)
    {
        var command = new TodoUpdateCommand(id, dto);
        await localEventBus.PublishAsync(command);
    }

    public async Task DeleteAsync(Guid id, [FromServices] ILocalEventBus localEventBus)
    {
        var command = new TodoDeleteCommand(id);
        await localEventBus.PublishAsync(command);
    }

    [AllowAnonymous]
    public async Task<Results<Ok<string>, BadRequest>> Login1Async([FromServices] IConfiguration configuration)
    {
        var token = GetToken(configuration, 1);
        return TypedResults.Ok(token.Token);
    }


    [AllowAnonymous]
    public async Task<Results<Ok<string>, BadRequest>> Login2Async([FromServices] IConfiguration configuration)
    {
        var token = GetToken(configuration, 2);
        return TypedResults.Ok(token.Token);
    }

    private (string Token, DateTime Expire) GetToken(IConfiguration configuration, int userType)
    {
        var nowTime = DateTime.Now;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "123@qq.com"),
            new Claim(ClaimTypes.MobilePhone, "13133737905"),
            new Claim(ClaimTypes.Name, "test"),
            new Claim("userType", userType.ToString()),
            //new Claim(ClaimTypes.Role, string.Join(",",roleList.Select(e=>e.Name))),
        };
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var algorithm = SecurityAlgorithms.HmacSha256;
        var signingCredentials = new SigningCredentials(secretKey, algorithm);
        var jwtSecurityToken = new JwtSecurityToken(
            configuration["Jwt:Issuer"],     //Issuer
            configuration["Jwt:Audience"],   //Audience
            claims,                          //Claims,
            nowTime,                    //notBefore
            nowTime.AddHours(4),    //expires
            signingCredentials
        );
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return (token, nowTime.AddHours(4));
    }
}

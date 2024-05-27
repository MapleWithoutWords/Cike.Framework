using Cike.EventBus.Local;
using Cike.EventBus.LocalEvent;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cike.AspNetCore.MinimalAPIs.Tests;

/// <summary>
/// 用户服务
/// </summary>
public class UserAppService : MinimalApiServiceBase
{
    public async Task<Results<Ok<object>, NotFound, BadRequest>> GetAsync([FromServices] LocalEventBus _localEventBus)
    {
        var userGetEvent = new UserGetEvent("User", 1, 10);
        await _localEventBus.PublishAsync(userGetEvent);
        return TypedResults.Ok(userGetEvent.User);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Results<Ok<string>, NotFound, BadRequest>> CreateAsync([FromServices] LocalEventBus _localEventBus, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }

    public async Task<Results<Ok<string>, NotFound, BadRequest>> UpdateAsync([FromServices] LocalEventBus _localEventBus, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }

    [MinimalApiRoute("api/v1/users/save", ["Post"])]
    public async Task<Results<Ok<string>, NotFound, BadRequest>> SaveAsync([FromServices] LocalEventBus _localEventBus, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }
    public async Task<Results<Ok<string>, NotFound, BadRequest>> DeleteAsync([FromServices] LocalEventBus _localEventBus, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }
    public async Task<Results<Ok<string>, NotFound, BadRequest>> GetListAsync([FromServices] LocalEventBus _localEventBus, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }

}

/// <summary>
/// 
/// </summary>
/// <param name="Keyword"></param>
/// <param name="PageIndex"></param>
/// <param name="PageSize"></param>
public record UserGetEvent(string Keyword, int PageIndex, int PageSize) : LocalEvent
{
    /// <summary>
    /// 
    /// </summary>
    public object User { get; set; } = null!;
}

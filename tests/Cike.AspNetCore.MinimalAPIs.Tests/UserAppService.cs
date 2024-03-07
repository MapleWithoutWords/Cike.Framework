using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cike.AspNetCore.MinimalAPIs.Tests;

/// <summary>
/// 用户服务
/// </summary>
public class UserAppService : MinimalApiServiceBase
{
    public async Task<Results<Ok<string>, NotFound, BadRequest>> GetAsync()
    {
        return TypedResults.Ok("Hello, World!");
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Results<Ok<string>, NotFound, BadRequest>> CreateAsync([FromServices] IConfiguration configuration, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }

}

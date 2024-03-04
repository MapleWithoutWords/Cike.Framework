using Microsoft.AspNetCore.Http.HttpResults;

namespace Cike.AspNetCore.MinimalAPIs.Tests;

public class UserAppService : MinimalApiServiceBase
{

    public async Task<Results<Ok<string>, NotFound, BadRequest>> CreateAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok(name);
    }

}

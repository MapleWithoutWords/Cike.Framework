using Cike.AspNetCore.MinimalAPIs;

namespace CQRS.WebApi.Services;

public class TestService : MinimalApiServiceBase
{
    public TestService()
    {
        RouteOptions.RouteHandlerBuilder = routeHanlderBuilder =>
        {
            routeHanlderBuilder.RequireAuthorization("BackendUser");
        };
    }

    public string GetAsync()
    {
        return "Hello,word!";
    }
}

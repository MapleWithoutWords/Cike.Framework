using Cike.EventBus.LocalEvent;

namespace Cike.AspNetCore.MinimalAPIs.Tests;

public class QueryHandler(IConfiguration _configuration)
{
    [LocalEventHandler]
    public async Task GetListAsync(UserGetEvent userGetEvent)
    {
        userGetEvent.User = new
        {
            Id = 1,
            Name = "User 1"
        };
    }
}

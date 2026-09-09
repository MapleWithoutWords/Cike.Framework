using Cike.Auth.Extensions;
using Cike.Auth.MultiTenant;
using Microsoft.AspNetCore.Http;

namespace Cike.Auth.Middlewares;

public class TenantMiddleware(ICurrentTenant _currentTenant) : IMiddleware, ITransientDependency
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = GetTenantId(context);
        if (tenantId != _currentTenant.Id)
        {
            using (_currentTenant.Change(tenantId))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }

    private long GetTenantId(HttpContext context)
    {
        var tenantId = context.User.GetLongValue(CikeClaimTypes.TenantId);
        if (tenantId > 0)
        {
            return tenantId;
        }

        if (context.Request.Headers.TryGetValue(CikeClaimTypes.TenantId, out var tenantStrval)
            && long.TryParse(tenantStrval.ToString(), out var headerTenantId))
        {
            return headerTenantId;
        }

        if (context.Request.Cookies.TryGetValue(CikeClaimTypes.TenantId, out var tenantStr)
            && long.TryParse(tenantStr.ToString(), out var cookieTenantId))
        {
            return cookieTenantId;
        }

        if (context.Request.Query.TryGetValue(CikeClaimTypes.TenantId, out tenantStrval)
            && long.TryParse(tenantStrval.ToString(), out var queryStringTenantId))
        {
            return queryStringTenantId;
        }

        return 0;
    }
}

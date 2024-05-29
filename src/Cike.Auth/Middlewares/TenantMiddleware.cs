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

    private Guid GetTenantId(HttpContext context)
    {
        var tenantId = context.User.GetGuidValue(CikeClaimTypes.TenantId);
        if (tenantId.HasValue)
        {
            return tenantId.Value;
        }

        if (context.Request.Headers.TryGetValue(CikeClaimTypes.TenantId, out var tenantStrval)
            && Guid.TryParse(tenantStrval.ToString(), out var headerTenantId))
        {
            return headerTenantId;
        }

        if (context.Request.Cookies.TryGetValue(CikeClaimTypes.TenantId, out var tenantStr)
            && Guid.TryParse(tenantStr.ToString(), out var cookieTenantId))
        {
            return cookieTenantId;
        }

        if (context.Request.Query.TryGetValue(CikeClaimTypes.TenantId, out tenantStrval)
            && Guid.TryParse(tenantStrval.ToString(), out var queryStringTenantId))
        {
            return queryStringTenantId;
        }

        return Guid.Empty;
    }
}

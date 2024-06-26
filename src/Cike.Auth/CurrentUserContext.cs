﻿using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Cike.Auth;

public class CurrentUserContext : ICurrentUser, ISingletonDependency
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? GetClaimsPrincipal()
    {
        return _httpContextAccessor.HttpContext?.User;
    }

    public virtual Claim[] FindClaims(string claimType)
    {
        return GetClaimsPrincipal()?.Claims.Where(c => c.Type == claimType).ToArray() ?? new Claim[0];
    }

    protected string? GetValue(string key)
    {
        return GetClaimsPrincipal()?.Claims.FirstOrDefault(e => e.Type == key)?.Value;
    }
    protected long? GetLongValue(string key)
    {
        var str = GetValue(key);
        if (long.TryParse(str, out var result))
        {
            return result;
        }
        return null;
    }
    protected Guid? GetGuidValue(string key)
    {
        var str = GetValue(key);
        if (Guid.TryParse(str, out var result))
        {
            return result;
        }
        return null;
    }

    public virtual string? Id => GetValue(CikeClaimTypes.UserId);

    public virtual string? UserName => this.GetValue(CikeClaimTypes.UserName);

    public virtual string? Name => this.GetValue(CikeClaimTypes.Name);

    public virtual string? SurName => this.GetValue(CikeClaimTypes.SurName);

    public virtual string? PhoneNumber => this.GetValue(CikeClaimTypes.PhoneNumber);


    public virtual string? Email => this.GetValue(CikeClaimTypes.Email);


    public virtual Guid? TenantId => GetGuidValue(CikeClaimTypes.TenantId);

    public virtual string[] Roles => FindClaims(CikeClaimTypes.Role).Select(c => c.Value).Distinct().ToArray();

    public virtual bool IsAuthorization => _httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated ?? true;
}

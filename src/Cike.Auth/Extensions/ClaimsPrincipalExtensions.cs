using System.Security.Claims;

namespace Cike.Auth.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Claim[] FindClaims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.Claims.Where(c => c.Type == claimType).ToArray();
    }

    public static string? GetValue(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.FindClaims(claimType).FirstOrDefault()?.Value;
    }
    public static Guid? GetGuidValue(this ClaimsPrincipal claimsPrincipal, string key)
    {
        var str = claimsPrincipal.GetValue(key);
        if (Guid.TryParse(str, out Guid result))
        {
            return result;
        }
        return null;
    }
}

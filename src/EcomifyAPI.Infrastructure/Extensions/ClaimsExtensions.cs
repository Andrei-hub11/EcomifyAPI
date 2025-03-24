using System.Security.Claims;

using Newtonsoft.Json.Linq;

namespace EcomifyAPI.Infrastructure.Extensions;

internal static class ClaimsExtensions
{
    public static bool HasRole(this ClaimsPrincipal user, string roleName)
    {
        var scopeClaim = user.FindFirst(claim => claim.Type == "scope");
        if (scopeClaim != null && scopeClaim.Value.Split(' ').Any(s => s.Equals("chat-app-scope", StringComparison.OrdinalIgnoreCase)))
        {
            var resourceAccessClaim = user.FindFirst(claim => claim.Type == "resource_access");
            if (resourceAccessClaim != null)
            {
                var resourceAccess = JObject.Parse(resourceAccessClaim.Value);
                var clientRoles = resourceAccess["chat-app-client"]?["roles"]?.Select(r => r.ToString());
                if (clientRoles != null && clientRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
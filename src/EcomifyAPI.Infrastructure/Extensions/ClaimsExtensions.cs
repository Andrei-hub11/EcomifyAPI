namespace EcomifyAPI.Infrastructure.Extensions;

using System.Security.Claims;

using Newtonsoft.Json.Linq;

internal static class ClaimsExtensions
{
    public static bool HasRole(this ClaimsPrincipal user, string roleName)
    {
        var resourceAccessClaim = user.FindFirst("resource_access");
        if (resourceAccessClaim is null)
            return false;

        var parsed = JObject.Parse(resourceAccessClaim.Value);

        var clientRoles = parsed["base-realm"]?["roles"]?.Values<string>();

        return clientRoles?.Contains(roleName, StringComparer.OrdinalIgnoreCase) ?? false;
    }
}
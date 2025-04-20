using System.Security.Claims;
using System.Text.Json;

internal static class JwtRoleClaimsHelper
{
    public static IEnumerable<Claim> ExtractRolesFromClaims(ClaimsPrincipal principal)
    {
        var json = principal.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(json)) return [];

        try
        {
            var roles = JsonDocument.Parse(json)
                .RootElement
                .GetProperty("base-realm")
                .GetProperty("roles")
                .EnumerateArray()
                .Select(r => r.GetString())
                .Where(r => !string.IsNullOrWhiteSpace(r));

            return roles.Select(role => new Claim(ClaimTypes.Role, role!));
        }
        catch
        {
            return [];
        }
    }
}
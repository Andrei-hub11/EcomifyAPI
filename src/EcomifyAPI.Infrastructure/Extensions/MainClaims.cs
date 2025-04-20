using System.Security.Claims;

using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Infrastructure.Extensions;

internal static class MainClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal? principal)
    {
        Claim? userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);

        return userIdClaim == null
            ? throw new UnauthorizeUserAccessException("The user context is not available")
            : userIdClaim.Value;
    }

    public static string GetEmail(this ClaimsPrincipal? principal)
    {
        Claim? emailClaim = principal?.FindFirst(ClaimTypes.Email);
        return emailClaim == null ? throw new UnauthorizeUserAccessException("The user context is not available") : emailClaim.Value;
    }
}
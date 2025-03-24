using System.Security.Claims;

using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Infrastructure.Extensions;

internal static class MainClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal? principal)
    {
        Claim? userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);

        return userIdClaim == null
            ? throw new UnauthorizeUserAccessException("O contexto do usuário não está disponível")
            : userIdClaim.Value;
    }
}
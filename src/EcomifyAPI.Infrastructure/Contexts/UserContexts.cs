using EcomifyAPI.Application.Contracts.Contexts;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Infrastructure.Extensions;

using Microsoft.AspNetCore.Http;

namespace EcomifyAPI.Infrastructure.Contexts;

internal sealed class UserContexts : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContexts(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User?.GetUserId() ??
        throw new UnauthorizeUserAccessException("The user context is not available");

    public string Email =>
        _httpContextAccessor.HttpContext?.User?.GetEmail() ??
        throw new UnauthorizeUserAccessException("The user context is not available");

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ??
        throw new UnauthorizeUserAccessException("The user context is not available");

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ??
        throw new UnauthorizeUserAccessException("The user context is not available");
}
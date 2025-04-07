using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;

namespace EcomifyAPI.Api.Middleware;

public class TokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerHelper<TokenMiddleware> _logger;

    public TokenMiddleware(RequestDelegate next, ILoggerHelper<TokenMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICookieService cookieService, IAccountService accountService)
    {
        var accessToken = context.Request.Cookies["access_token"];
        var refreshToken = context.Request.Cookies["refresh_token"];

        var endpoint = context.GetEndpoint();
        var isProtected = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null;

        if (accessToken is not null)
        {
            context.Request.Headers.Authorization = $"Bearer {accessToken}";
        }

        if (refreshToken is not null && accessToken is null)
        {
            var result = await accountService.UpdateAccessTokenAsync(new UpdateAccessTokenRequestDTO(refreshToken));

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to refresh access token with provided refresh token.");
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            cookieService.SetCookie("access_token", result.Value.AccessToken, 15);

            context.Request.Headers.Authorization = $"Bearer {result.Value.AccessToken}";
        }

        if (refreshToken is null && accessToken is null && isProtected)
        {
            throw new UnauthorizedAccessException("No token provided");
        }

        await _next(context);
    }
}
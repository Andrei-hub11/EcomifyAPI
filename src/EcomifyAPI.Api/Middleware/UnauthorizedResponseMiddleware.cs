using System.Net;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Middleware;

public class UnauthorizedResponseMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);

        if (httpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized && !httpContext.Response.HasStarted)
        {
            var response = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "You do not have access to this feature, or have not yet logged in",
                Detail = "An authentication error has occurred. Please check your credentials and try again.",
                Instance = Guid.NewGuid().ToString(),
                Type = $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{(int)HttpStatusCode.Unauthorized}",
            };

            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }
}
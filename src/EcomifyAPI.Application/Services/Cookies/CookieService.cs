using EcomifyAPI.Application.Contracts.Services;

using Microsoft.AspNetCore.Http;

namespace EcomifyAPI.Application.Services.Cookies;

public class CookieService : ICookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCookie(string key)
    {
        return _httpContextAccessor.HttpContext.Request.Cookies[key];
    }

    public void SetCookie(string key, string value, int? expireTime)
    {
        bool isRefreshToken = key == "refresh_token";

        _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, new CookieOptions
        {
            Expires = isRefreshToken ? DateTime.Now.AddDays(expireTime ?? 14) : DateTime.Now.AddMinutes(expireTime ?? 30),
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });
    }

    public void DeleteCookie(string key)
    {
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
    }
}
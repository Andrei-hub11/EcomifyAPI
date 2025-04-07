namespace EcomifyAPI.Application.Contracts.Services;

public interface ICookieService
{
    string GetCookie(string key);
    void SetCookie(string key, string value, int? expireTime);
    void DeleteCookie(string key);
}
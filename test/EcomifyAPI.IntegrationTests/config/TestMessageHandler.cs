using System.Diagnostics.CodeAnalysis;
using System.Net;

using Microsoft.Net.Http.Headers;

public class TestHttpClientHandler : DelegatingHandler
{
    [NotNull]
    private CookieContainer cookies = new();

    public TestHttpClientHandler([NotNull] HttpMessageHandler innerHandler)
        : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync([NotNull] HttpRequestMessage request, CancellationToken ct)
    {
        Uri requestUri = request.RequestUri ?? new Uri("https://localhost");

        string cookieHeader = this.cookies.GetCookieHeader(requestUri);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        HttpResponseMessage response = await base.SendAsync(request, ct);

        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> setCookieHeaders))
        {
            foreach (SetCookieHeaderValue setCookieHeader in SetCookieHeaderValue.ParseList(setCookieHeaders.ToList()))
            {
                try
                {
                    if (setCookieHeader.Name.Value == null || setCookieHeader.Value == null)
                        continue;

                    Cookie cookie = new(
                        setCookieHeader.Name.Value,
                        setCookieHeader.Value.Value ?? string.Empty,
                        setCookieHeader.Path.HasValue ? setCookieHeader.Path.Value : "/",
                        setCookieHeader.Domain.HasValue ? setCookieHeader.Domain.Value : requestUri.Host
                    );

                    if (setCookieHeader.Expires.HasValue)
                    {
                        cookie.Expires = setCookieHeader.Expires.Value.DateTime;
                    }

                    cookie.Secure = setCookieHeader.Secure;

                    cookie.HttpOnly = setCookieHeader.HttpOnly;

                    this.cookies.Add(requestUri, cookie);
                }
                catch (Exception)
                {
                    // Log or silent handling for invalid cookies
                    // Can add logging here if needed
                }
            }
        }

        return response;
    }

    public CookieContainer GetCookies() => cookies;
    public void ClearCookies() => cookies = new();

    public void RemoveAccessToken(Uri baseAddress)
    {
        var cookies = this.cookies.GetCookies(baseAddress);

        foreach (Cookie cookie in cookies)
        {
            if (cookie.Name == "access_token")
            {
                var expiredCookie = new Cookie("access_token", "", cookie.Path, cookie.Domain)
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Secure = cookie.Secure,
                    HttpOnly = cookie.HttpOnly
                };

                this.cookies.Add(baseAddress, expiredCookie);
                break;
            }
        }
    }
}
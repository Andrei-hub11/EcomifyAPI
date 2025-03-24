using Newtonsoft.Json;

namespace EcomifyAPI.Contracts.Models;

public class KeycloakToken
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}
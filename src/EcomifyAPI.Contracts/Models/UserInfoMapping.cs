using Newtonsoft.Json;

namespace EcomifyAPI.Contracts.Models;

public sealed class UserInfoMapping
{
    [JsonProperty("sub")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("preferred_username")]
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
}
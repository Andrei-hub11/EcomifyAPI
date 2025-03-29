using Newtonsoft.Json;

namespace EcomifyAPI.Contracts.Models;

public class ResourceAccess
{
    [JsonProperty("roles")]
    public List<string> Roles { get; set; } = [];
}
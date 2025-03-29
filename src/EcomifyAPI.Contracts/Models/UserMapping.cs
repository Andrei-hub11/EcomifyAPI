using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EcomifyAPI.Contracts.Models;

public class UserMapping
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string ProfileImagePath { get; set; } = string.Empty;

    [JsonExtensionData]
    private readonly IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        if (_additionalData != null && _additionalData.TryGetValue("attributes", out var attributesToken))
        {
            var attributes = attributesToken.ToObject<Dictionary<string, JToken>>();
            if (attributes != null && attributes.TryGetValue("profileImagePath", out var profileImageUrlToken))
            {
                ProfileImagePath = ExtractValue(profileImageUrlToken);
            }

            if (attributes != null && attributes.TryGetValue("normalizedUserName", out var normalizedUserName))
            {
                UserName = ExtractValue(normalizedUserName);
            }
        }
    }

    private string ExtractValue(JToken token)
    {
        if (token.Type == JTokenType.Array)
        {
            return token.FirstOrDefault()?.ToString() ?? string.Empty;
        }
        return token.ToString();
    }
}
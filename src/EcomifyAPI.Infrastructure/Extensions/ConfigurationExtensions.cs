using Microsoft.Extensions.Configuration;

namespace EcomifyAPI.Infrastructure.Extensions;

internal static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrEmpty(value) ? throw new InvalidOperationException($"Configuration key '{key}' is missing or null.") : value;
    }
}
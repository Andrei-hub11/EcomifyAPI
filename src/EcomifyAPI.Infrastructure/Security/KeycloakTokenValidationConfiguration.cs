using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcomifyAPI.Infrastructure.Security;

public sealed class KeycloakTokenValidationConfiguration(IOptions<KeycloakSettings> keycloakSettings)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly KeycloakSettings _keycloakSettings = keycloakSettings.Value;

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    public void Configure(JwtBearerOptions options)
    {
        options.Authority = $"{_keycloakSettings.AuthServerUrl}realms/{_keycloakSettings.Realm}";
        options.RequireHttpsMetadata = false;
        options.Audience = _keycloakSettings.Resource;
        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = _keycloakSettings.VerifyTokenAudience,
            ValidAudience = _keycloakSettings.Resource,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateLifetime = true,
        };
    }
}
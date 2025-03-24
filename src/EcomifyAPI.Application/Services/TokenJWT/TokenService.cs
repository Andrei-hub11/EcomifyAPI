using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.TokenJWT;
using EcomifyAPI.Domain.Entities;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EcomifyAPI.Application.Services.TokenJWT;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerHelper<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILoggerHelper<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GeneratePasswordResetToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["TokenSecret"] ??
            throw new NullReferenceException("'TokenSecret' cannot be null"));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email.Value)
        }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidatePasswordResetToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["TokenSecret"] ??
            throw new NullReferenceException("'TokenSecret' cannot be null"));

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var userIdClaim = claimsPrincipal.FindFirst("id");
            if (userIdClaim != null)
            {
                return true;
            }
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogError(ex, $"The token has expired. Details: {ex.Message}");
            return false;
        }
        catch (Exception)
        {
            throw;
        }

        return false;
    }
}
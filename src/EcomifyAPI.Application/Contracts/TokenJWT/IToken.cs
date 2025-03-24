using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.Contracts.TokenJWT;

public interface ITokenService
{
    string GeneratePasswordResetToken(User user);
    bool ValidatePasswordResetToken(string token);
}
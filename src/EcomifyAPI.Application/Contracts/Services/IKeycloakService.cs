using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IKeycloakService
{
    Task<Result<UserMapping>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponseDTO>> GetAllUsersAsync();
    Task<Result<AuthKeycloakResponseDTO>> RegisterUserAync(UserRegisterRequestDTO requestDTO, string profileImageUrl,
        CancellationToken cancellationToken);
    Task<Result<AuthKeycloakResponseDTO>> RegisterAdminAsync(UserRegisterRequestDTO requestDTO, string profileImageUrl,
        CancellationToken cancellationToken);
    Task<Result<AuthKeycloakResponseDTO>> LoginUserAync(UserLoginRequestDTO request, CancellationToken cancellationToken);
    Task<KeycloakToken> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateUserPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken);
    Task<bool> DeleteUserByIdAsync(string userId);
}
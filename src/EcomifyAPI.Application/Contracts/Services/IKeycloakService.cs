using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IKeycloakService
{
    Task<Result<UserMapping>> GetUserByEmailAsync(string email);
    Task<IReadOnlyList<UserResponseDTO>> GetAllUsersAsync();
    Task<Result<UserInfoMapping>> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken);
    Task<Result<AuthResponseDTO>> RegisterUserAync(UserRegisterRequestDTO requestDTO, string profileImageUrl,
        CancellationToken cancellationToken);
    Task<Result<AuthResponseDTO>> RegisterAdminAsync(UserRegisterRequestDTO requestDTO, string profileImageUrl,
        CancellationToken cancellationToken);
    Task<Result<AuthResponseDTO>> LoginUserAync(UserLoginRequestDTO request, CancellationToken cancellationToken);
    Task<KeycloakToken> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateUserPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken);
    Task<bool> DeleteUserByIdAsync(string userId);
}
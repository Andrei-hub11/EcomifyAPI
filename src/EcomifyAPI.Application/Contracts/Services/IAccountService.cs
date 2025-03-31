using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IAccountService
{
    Task<Result<UserResponseDTO>> GetAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );
    Task<Result<UserResponseDTO>> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
    Task<Result<AuthResponseDTO>> RegisterAsync(
        UserRegisterRequestDTO request,
        CancellationToken cancellationToken = default
    );
    Task<Result<AuthResponseDTO>> LoginAsync(
        UserLoginRequestDTO request,
        CancellationToken cancellationToken = default
    );
    Task<Result<bool>> ForgotPasswordAsync(
        ForgetPasswordRequestDTO request,
        CancellationToken cancellationToken
    );
    Task<Result<UpdateAccessTokenResponseDTO>> UpdateAccessTokenAsync(
        UpdateAccessTokenRequestDTO request,
        CancellationToken cancellationToken = default
    );
    Task<Result<bool>> UpdatePasswordAsync(
        UpdatePasswordRequestDTO request,
        CancellationToken cancellationToken = default
    );
    //Task<Result<UserResponseDTO>> UpdateUserAsync(
    //    string userId,
    //    UpdateUserRequestDTO request,
    //    CancellationToken cancellationToken = default
    //);
    /*  Task CleanupTestUsersAsync(CancellationToken cancellationToken = default); */
}
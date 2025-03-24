using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IAccountService
{
    Task<Result<UserResponseDTO>> GetUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    );
    Task<Result<UserResponseDTO>> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
    Task<Result<AuthResponseDTO>> RegisterUserAsync(
        UserRegisterRequestDTO request,
        CancellationToken cancellationToken = default
    );
    Task<Result<AuthResponseDTO>> LoginUserAsync(
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
    Task<Result<bool>> UpdateUserPasswordAsync(
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
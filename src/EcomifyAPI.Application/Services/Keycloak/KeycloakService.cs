using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.Contracts.UtillityFactories;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Application.Extensions;
using EcomifyAPI.Common.Helpers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Exceptions;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

namespace EcomifyAPI.Application.Services.Keycloak;

public class KeycloakService : IKeycloakService
{
    private readonly string _endpointAdminBase;
    private readonly string _endpointClientBase;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILoggerHelper<KeycloakService> _logger;
    private readonly IKeycloakServiceErrorHandler _keycloakServiceErrorHandler;
    private readonly TimeSpan _tokenExpiryBuffer = TimeSpan.FromMinutes(1);
    private KeycloakToken _cachedToken = default!;
    private DateTimeOffset _tokenExpiration = DateTimeOffset.MinValue;

    public KeycloakService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILoggerHelper<KeycloakService> logger,
        IKeycloakServiceErrorHandler keycloakServiceErrorHandler
    )
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _endpointAdminBase = _configuration.GetRequiredValue("UserKeycloakAdmin:EndpointBase");
        _endpointClientBase = _configuration.GetRequiredValue("UserKeycloakClient:EndpointBase");
        _keycloakServiceErrorHandler = keycloakServiceErrorHandler;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserResponseDTO>> GetAllUsersAsync()
    {
        var users = await GetUsersAsync();

        return users.ToReponseDTO();
    }

    public async Task<Result<AuthResponseDTO>> RegisterUserAync(
        UserRegisterRequestDTO request,
        string profileImageUrl,
        CancellationToken cancellationToken
    )
    {
        Result<UserMapping> newUser = default!;
        bool isRollback = true;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tokenResponse = await GetAdminTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                tokenResponse.AccessToken
            );

            var user = new
            {
                username = request.UserName,
                email = request.Email,
                enabled = true,
                groups = new[] { "/Users" },
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = request.Password,
                        temporary = false,
                    },
                },
                attributes = new Dictionary<string, string>
                {
                    ["profileImagePath"] = profileImageUrl,
                    ["normalizedUserName"] = request.UserName,
                },
            };

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_endpointAdminBase}/users", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var resultMap = await _keycloakServiceErrorHandler.ExtractErrorFromResponse(
                    response
                );
                return Result.Fail(resultMap.Errors);
            }

            newUser = await GetUserByNameAsync(request.UserName);

            if (newUser.IsFailure)
            {
                return Result.Fail(newUser.Errors);
            }


            var userToken = await GetUserTokenAsync(request.UserName, request.Password);

            if (userToken.IsFailure)
            {
                return Result.Fail(userToken.Errors);
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(userToken.Value.AccessToken);
            var rolesClaim = token.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;

            ThrowHelper.ThrowIfNull(rolesClaim);

            var resourceAccess = JsonConvert.DeserializeObject<Dictionary<string, ResourceAccess>>(rolesClaim);

            ThrowHelper.ThrowIfNull(resourceAccess);

            var baseRealmRoles = resourceAccess["base-realm"].Roles;

            ThrowHelper.ThrowIfNull(baseRealmRoles);

            var userRoles = new HashSet<string>(baseRealmRoles);

            isRollback = false;

            return new AuthResponseDTO(
                newUser.Value.ToResponseDTO(),
                userToken.Value.AccessToken,
                userToken.Value.RefreshToken,
                userRoles
            );
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            // If the user was successfully created in Keycloak, try to delete it in case of failure
            if (
                isRollback
                && newUser != null
                && !newUser.IsFailure
                && !string.IsNullOrEmpty(newUser.Value.Id)
            )
            {
                try
                {
                    await DeleteUserByIdAsync(newUser.Value.Id);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(
                        deleteEx,
                        $"Failed to delete user with ID: {newUser.Value.Id} after a registration failure."
                    );
                }
            }
        }
    }

    public async Task<Result<AuthResponseDTO>> LoginUserAync(
        UserLoginRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userToken = await GetUserTokenAsync(request.Email, request.Password);

            if (userToken.IsFailure)
            {
                return Result.Fail(userToken.Errors);
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(userToken.Value.AccessToken);
            var rolesClaim = token.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;

            ThrowHelper.ThrowIfNull(rolesClaim);

            var resourceAccess = JsonConvert.DeserializeObject<Dictionary<string, ResourceAccess>>(rolesClaim);

            ThrowHelper.ThrowIfNull(resourceAccess);

            var baseRealmRoles = resourceAccess["base-realm"].Roles;

            ThrowHelper.ThrowIfNull(baseRealmRoles);

            var userRoles = new HashSet<string>(baseRealmRoles);

            var user = await GetUserByEmailAsync(request.Email);

            if (user.IsFailure)
            {
                return Result.Fail(user.Errors);
            }

            return new AuthResponseDTO(
                user.Value.ToResponseDTO(),
                AccessToken: userToken.Value.AccessToken,
                RefreshToken: userToken.Value.RefreshToken,
                Roles: userRoles
            );
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<KeycloakToken> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var formData = new Dictionary<string, string>
        {
            { "client_id", _configuration.GetRequiredValue("UserKeycloakClient:client_id") },
            {
                "client_secret",
                _configuration.GetRequiredValue("UserKeycloakClient:client_secret")
            },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
        };

        var tokenEndpoint = _configuration.GetRequiredValue("UserKeycloakClient:TokenEndpoint");

        var content = new FormUrlEncodedContent(formData);
        var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException($"Request failed: {response.StatusCode}, {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonConvert.DeserializeObject<KeycloakToken>(jsonResponse);

        ThrowHelper.ThrowIfNull(tokenResponse);

        return tokenResponse;
    }

    private async Task<IEnumerable<UserMapping>> GetUsersAsync()
    {
        var apiUrl = $"{_endpointAdminBase}/users";

        var tokenResponse = await GetAdminTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new BadRequestException(
                $"Failed to retrieve user details: {response.StatusCode}, {error}"
            );
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var users = JsonConvert.DeserializeObject<List<UserMapping>>(jsonResponse);

        if (users == null)
        {
            throw new NotFoundException("Users not found");
        }

        return users;
    }

    private async Task<Result<UserMapping>> GetUserByNameAsync(string userName)
    {
        var apiUrl = $"{_endpointAdminBase}/users/?username={userName}";

        var tokenResponse = await GetAdminTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new BadRequestException(
                $"Failed to retrieve user details: {response.StatusCode}, {error}"
            );
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        // However, it will return a unique user.
        var users = JsonConvert.DeserializeObject<List<UserMapping>>(jsonResponse);

        if (users == null || users.Count == 0)
        {
            return Result.Fail(UserErrorFactory.UserNotFoundByName(userName));
        }

        return users.First();
    }

    public async Task<Result<UserMapping>> GetUserByEmailAsync(string email)
    {
        var apiUrl = $"{_endpointAdminBase}/users/?email={email}";

        var tokenResponse = await GetAdminTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new BadRequestException(
                $"Failed to retrieve user details: {response.StatusCode}, {error}"
            );
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        // However, it will return a unique user.
        var users = JsonConvert.DeserializeObject<List<UserMapping>>(jsonResponse);

        if (users == null || users.Count == 0)
        {
            return Result.Fail(UserErrorFactory.UserNotFoundByEmail(email));
        }

        return users.First();
    }

    public async Task<Result<UserInfoMapping>> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var apiUrl = $"{_endpointClientBase}/protocol/openid-connect/userinfo";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

        var response = await _httpClient.GetAsync(apiUrl, cancellationToken);

        if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Result.Fail(UserErrorFactory.InvalidOrExpiredToken("/userinfo"));
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException(
                $"Failed to retrieve user details: {response.StatusCode}, {error}"
            );
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        var user = JsonConvert.DeserializeObject<UserInfoMapping>(jsonResponse);

        ThrowHelper.ThrowIfNull(user);

        return user;
    }

    private async Task<KeycloakToken> GetAdminTokenAsync()
    {
        if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiration - _tokenExpiryBuffer)
        {
            return _cachedToken;
        }

        var formData = new Dictionary<string, string>
        {
            { "client_id", _configuration.GetRequiredValue("UserKeycloakAdmin:client_id") },
            { "client_secret", _configuration.GetRequiredValue("UserKeycloakAdmin:client_secret") },
            { "grant_type", "password" },
            { "username", _configuration.GetRequiredValue("UserKeycloakAdmin:username") },
            { "password", _configuration.GetRequiredValue("UserKeycloakAdmin:password") },
        };

        var tokenEndpoint = _configuration.GetRequiredValue("UserKeycloakAdmin:TokenEndpoint");

        var content = new FormUrlEncodedContent(formData);
        var response = await _httpClient.PostAsync(tokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new BadRequestException($"Request failed: {response.StatusCode}, {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var tokenResponse = JsonConvert.DeserializeObject<KeycloakToken>(jsonResponse);

        ThrowHelper.ThrowIfNull(tokenResponse);

        _cachedToken = tokenResponse;
        _tokenExpiration = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        return tokenResponse;
    }

    private async Task<Result<KeycloakToken>> GetUserTokenAsync(string username, string password)
    {
        var formData = new Dictionary<string, string>
        {
            { "client_id", _configuration.GetRequiredValue("UserKeycloakClient:client_id") },
            {
                "client_secret",
                _configuration.GetRequiredValue("UserKeycloakClient:client_secret")
            },
            { "grant_type", "password" },
            { "username", username },
            { "password", password },
            { "scope", "offline_access" },
        };

        var tokenEndpoint = _configuration.GetRequiredValue("UserKeycloakClient:TokenEndpoint");

        var content = new FormUrlEncodedContent(formData);
        var response = await _httpClient.PostAsync(tokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var result = await _keycloakServiceErrorHandler.ExtractErrorFromResponse(response);
            return Result.Fail(result.Errors);
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<KeycloakToken>(jsonResponse);

        ThrowHelper.ThrowIfNull(tokenResponse);

        return tokenResponse;
    }
    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tokenResponse = await GetAdminTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        var updateUserUrl = $"{_endpointAdminBase}/users/{user.Id}";

        // warning: sending email, even if it is not actually being updated, to avoid deleting this field in keycloak

        var updatedUser = new
        {
            username = user.UserName,
            email = user.Email,
            attributes = new Dictionary<string, string>
            {
                ["profileImagePath"] = user.ProfileImagePath.Value,
                ["normalizedUserName"] = user.UserName,
            },
        };

        var json = JsonConvert.SerializeObject(updatedUser);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(updateUserUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException(
                $"Failed to update user {user.Id}: {response.StatusCode}, {errorContent}"
            );
        }
    }

    public async Task UpdateUserPasswordAsync(
        string userId,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tokenResponse = await GetAdminTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        var resetPasswordUrl =
            $"{_endpointAdminBase}/users/{userId}/reset-password";

        var resetPasswordPayload = new
        {
            type = "password",
            value = newPassword,
            temporary = false,
        };

        var json = JsonConvert.SerializeObject(resetPasswordPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(resetPasswordUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException(
                $"Failed to reset password for user {userId}: {response.StatusCode}, {errorContent}"
            );
        }
    }

    public async Task<bool> DeleteUserByIdAsync(string userId)
    {
        var tokenResponse = await GetAdminTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        var deleteUserUrl = $"{_endpointAdminBase}/users/{userId}";
        var response = await _httpClient.DeleteAsync(deleteUserUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new BadRequestException(
                $"Failed to add role to user: {response.StatusCode}, {error}"
            );
        }

        return true;
    }
}
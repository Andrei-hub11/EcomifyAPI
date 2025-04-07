using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/account")]
[ApiController]
[ServiceFilter(typeof(ResultFilter))]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ICookieService _cookieService;
    private readonly IWebHostEnvironment _environment;

    public AccountController(IAccountService accountService, ICookieService cookieService, IWebHostEnvironment environment)
    {
        _accountService = accountService;
        _cookieService = cookieService;
        _environment = environment;
    }

    /// <summary>
    /// Retrieves the user profile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="UserResponseDTO"/> containing the user profile.</returns>
    /// <response code="200">Returns the user profile when found successfully.</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="422">Validation errors</response>
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var accessToken = HttpContext
            .Request.Headers.Authorization.ToString()
            .Replace("Bearer ", "");

        var result = await _accountService.GetAsync(accessToken, cancellationToken);

        return result.Match(
            onSuccess: (user) => Ok(user),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    //[Authorize(Policy = "Admin")]
    //[HttpGet("users")]
    //public async Task<IActionResult> GetAllUsers()
    //{
    //    try
    //    {
    //        var users = await _accountService.GetAllUsersAsync();

    //        return Ok(new
    //        {
    //            Users = users
    //        });
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}

    /// <summary>
    /// Registers a new user in the keycloak client.
    /// </summary>
    /// <param name="request">The user registration request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="UserResponseDTO"/> containing the registered user.</returns>
    /// <response code="200">Returns the registered user when found successfully.</response>
    /// <response code="400">Some invalid data was provided.</response>
    /// <response code="409">Email already exists</response>
    /// <response code="422">Validation errors</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] UserRegisterRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        var result = await _accountService.RegisterAsync(request, cancellationToken);


        return result.Match(
            onSuccess: (authResponse) => Ok(authResponse),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Logs in a user in the keycloak client.
    /// </summary>
    /// <param name="request">The user login request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="AuthResponseDTO"/> containing the authenticated user.</returns>
    /// <response code="200">Returns the authenticated user when found successfully.</response>
    /// <response code="400">Some invalid data was provided.</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Validation errors</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] UserLoginRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        var result = await _accountService.LoginAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (authResponse) => Ok(authResponse),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Sends a forgot password email to the user.
    /// </summary>
    /// <param name="request">The forgot password request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A boolean value indicating whether the forgot password email was sent successfully.</returns>
    /// <response code="200">Returns true if the forgot password email was sent successfully.</response>
    /// <response code="400">Some invalid data was provided.</response>
    /// <response code="422">Validation errors</response>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgetPasswordRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        var result = await _accountService.ForgotPasswordAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (isSuccess) => Ok(isSuccess),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Refreshes the access token.
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="UpdateAccessTokenResponseDTO"/> containing the refreshed access token.</returns>
    /// <response code="200">Returns the refreshed access token when found successfully.</response>
    /// <response code="400">Some invalid data was provided.</response>
    /// <response code="422">Validation errors</response>
    [HttpPost("token-renew")]
    public async Task<IActionResult> RefreshAccessToken(
        [FromBody] UpdateAccessTokenRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        var result = await _accountService.UpdateAccessTokenAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (accessToken) => Ok(accessToken),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    //[Authorize]
    //[HttpPut("profile/{userId}")]
    //public async Task<IActionResult> UpdateUserAsync(
    //    [FromBody] UpdateUserRequestDTO request,
    //    string userId,
    //    CancellationToken cancellationToken
    //)
    //{
    //    var result = await _accountService.UpdateUserAsync(userId, request, cancellationToken);

    //    return result.Match(
    //        onSuccess: (user) => Ok(user),
    //        onFailure: (errors) => errors.ToProblemDetailsResult()
    //    );
    //}

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _cookieService.DeleteCookie("access_token");
        _cookieService.DeleteCookie("refresh_token");

        return Ok();
    }

    /// <summary>
    /// Updates the user password.
    /// </summary>
    /// <param name="request">The update password request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A boolean value indicating whether the password is updated, false otherwise.</returns>
    /// <response code="200">Returns true if the password is updated, false otherwise.</response>
    /// <response code="400">Some invalid data was provided.</response>
    /// <response code="422">Validation errors</response>
    [HttpPut("update-password")]
    public async Task<IActionResult> UpdateUserPassword(
        [FromBody] UpdatePasswordRequestDTO request,
        CancellationToken cancellationToken
    )
    {
        var result = await _accountService.UpdatePasswordAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (isSuccess) => Ok(isSuccess),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    //[HttpDelete("test-cleanup")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    //public async Task<IActionResult> CleanupTestUsers(CancellationToken cancellationToken)
    //{
    //    if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"))
    //    {
    //        return NotFound();
    //    }

    //    await _accountService.CleanupTestUsersAsync(cancellationToken);
    //    return Ok();
    //}
}
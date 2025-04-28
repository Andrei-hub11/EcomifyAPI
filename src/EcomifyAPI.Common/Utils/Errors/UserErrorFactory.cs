using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Errors;

public sealed class UserErrorFactory
{
    /// <summary>
    /// Creates an error when the provided password does not match the stored password.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing a password mismatch error.</returns>
    public static ValidationError InvalidPassword(string field)
    {
        return Error.Validation(
            "The provided password is incorrect.",
            "ERR_INVALID_PASSWORD", field);
    }

    /// <summary>
    /// Creates a not found error for a user by ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>An <see cref="Error"/> instance representing a user not found error.</returns>
    public static Error UserNotFoundById(string id)
    {
        return Error.NotFound($"User with id = '{id}' was not found.", "ERR_USER_NOT_FOUND");
    }

    /// <summary>
    /// Creates a not found error for a user by username.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <returns>An <see cref="Error"/> instance representing a user not found error.</returns>
    public static Error UserNotFoundByName(string userName)
    {
        return Error.NotFound($"User with username = '{userName}' was not found.", "ERR_USER_NOT_FOUND");
    }

    /// <summary>
    /// Creates a not found error for a user by email.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <returns>An <see cref="Error"/> instance representing a user not found error.</returns>
    public static Error UserNotFoundByEmail(string email)
    {
        return Error.NotFound($"User with email = '{email}' was not found.", "ERR_USER_NOT_FOUND");
    }

    /// <summary>
    /// Creates a not found error for an address by ID.
    /// </summary>
    /// <param name="id">The address identifier.</param>
    /// <returns>An <see cref="Error"/> instance representing an address not found error.</returns>
    public static Error AddressNotFoundById(Guid id)
    {
        return Error.NotFound($"Address with ID = '{id}' was not found.", "ERR_ADDRESS_NOT_FOUND");
    }

    /// <summary>
    /// Creates a conflict error for an existing username without specifying the username.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing a conflict error.</returns>
    public static Error UsernameAlreadyExists()
    {
        return Error.Conflict("An account with the provided username is already registered.", "ERR_DUPLICATE_USERNAME");
    }

    ///// <summary>
    ///// Creates a conflict error for an existing email.
    ///// </summary>
    ///// <param name="email">The email that already exists.</param>
    ///// <returns>An <see cref="Error"/> instance representing a conflict error.</returns>
    //public static Error EmailAlreadyExists(string email)
    //{
    //    return Error.Conflict($"The email '{email}' is already registered.", "ERR_DUPLICATE_EMAIL");
    //}

    /// <summary>
    /// Creates a conflict error for an existing email without specifying the email address.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing a conflict error.</returns>
    public static Error EmailAlreadyExists()
    {
        return Error.Conflict("An account with the provided email is already registered.", "ERR_DUPLICATE_EMAIL");
    }

    /// <summary>
    /// Creates a conflict error for an existing address with the provided fields.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing a conflict error.</returns>
    public static Error AddressAlreadyExists()
    {
        return Error.Conflict("An address with the provided fields is already registered.", "ERR_DUPLICATE_ADDRESS");
    }

    /// <summary>
    /// Creates an unauthorized error indicating that the user needs to authenticate to access a specified resource.
    /// </summary>
    /// <param name="resourceName">The name or description of the resource the user attempted to access.</param>
    /// <returns>An <see cref="Error"/> instance representing an unauthorized access error.</returns>
    public static Error UnauthorizedAccess(string resourceName)
    {
        return Error.Unauthorized($"Authentication required to access the resource '{resourceName}'.", "ERR_UNAUTHORIZED_ACCESS");
    }

    /// <summary>
    /// Creates an error indicating that the provided token is no longer valid.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing an invalid token error.</returns>
    public static Error InvalidTokenError()
    {
        // Cria e retorna um erro de token inválido
        return Error.Unauthorized("The provided token is no longer valid.", "ERR_TOKEN_INVALID");
    }

    /// <summary>
    /// Creates an error indicating that the authentication token may be invalid or expired.
    /// </summary>
    /// <param name="resource">The resource or endpoint where the authentication failure occurred.</param>
    /// <returns>An <see cref="Error"/> instance representing an invalid or expired token error.</returns>
    public static Error InvalidOrExpiredToken(string resource)
    {
        return Error.Unauthorized($"The authentication token may be invalid or expired while accessing '{resource}'.", "ERR_INVALID_OR_EXPIRED_TOKEN");
    }

    /// <summary>
    /// Creates an error when the provided email or password is incorrect.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing an email or password mismatch error.</returns>
    public static Error InvalidEmailOrPassword()
    {
        return Error.Unauthorized(
            "The provided email or password is incorrect.",
            "ERR_INVALID_EMAIL_OR_PASSWORD");
    }

    ///// <summary>
    ///// Creates a forbidden error indicating that the user does not have access to a specified resource.
    ///// </summary>
    ///// <param name="resourceName">The name or description of the resource the user attempted to access.</param>
    ///// <returns>An <see cref="Error"/> instance representing a forbidden access error.</returns>
    //public static Error AccessDenied(string resourceName)
    //{
    //    return Error.Unauthorized($"Access denied to the resource '{resourceName}'.", "ERR_ACCESS_DENIED");
    //}
}
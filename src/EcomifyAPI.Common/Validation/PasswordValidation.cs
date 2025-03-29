using System.Text.RegularExpressions;

using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Validation;

public static class PasswordValidation
{
    public static IReadOnlyList<ValidationError> Validate(string password)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add(ValidationError.Create("Password cannot be empty", "ERR_PASSWORD_EMPTY", "Password"));
        }

        if (password.Length < 8)
        {
            errors.Add(ValidationError.Create("Password must be at least 8 characters long", "ERR_PASSWORD_TOO_SHORT", "Password"));
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add(ValidationError.Create("Password must contain at least one uppercase letter", "ERR_PASSWORD_NO_UPPERCASE", "Password"));
        }

        if (!Regex.IsMatch(password, @"(?:.*[!@#$%^&*]){2,}"))
        {
            errors.Add(ValidationError.Create("Password must contain at least two special characters", "ERR_PASSWORD_SPECIAL_CHAR", "Password"));
        }

        return errors;
    }
}
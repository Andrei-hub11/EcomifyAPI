using System.Text.RegularExpressions;

using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct Email
{
    public string Value { get; init; }

    public Email(string value)
    {
        ValidateEmail(value);
        Value = value;
    }

    private static void ValidateEmail(string value)
    {
        var errors = new List<IError>();

        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(Error.Validation("Email cannot be empty", "ERR_EMAIL_EMPTY", "Email"));
        }

        if (!IsValidEmail(value))
        {
            errors.Add(Error.Validation("Invalid email format", "ERR_EMAIL_INVALID", "Email"));
        }

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);

            if (addr.Address != email)
            {
                return false;
            }

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        catch
        {
            return false;
        }
    }
}
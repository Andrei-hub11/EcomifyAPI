using System.Text.RegularExpressions;

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
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty", nameof(value));
        }

        if (!IsValidEmail(value))
        {
            throw new ArgumentException("Invalid email format", nameof(value));
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
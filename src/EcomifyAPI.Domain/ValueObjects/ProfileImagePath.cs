using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct ProfileImagePath
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly char[] CommonInvalidPathChars = ['<', '>', ':', '"', '|', '?', '*', '\0'];

    public string Value { get; init; }

    public ProfileImagePath(string value)
    {
        //ignore the validation if the value is null, because it's optional
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = string.Empty;
            return;
        }

        ValidateProfileImagePath(value);
        Value = value;
    }

    public static void ValidateProfileImagePath(string value)
    {
        var errors = new List<IError>();

        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(Error.Validation("Profile image path cannot be empty", "ERR_IMG_PATH_EMPTY", "ProfileImagePath"));
        }

        if (value.Length > 255)
        {
            errors.Add(Error.Validation("Profile image path cannot be longer than 255 characters", "ERR_IMG_PATH_LONG", "ProfileImagePath"));
        }

        if (Path.IsPathRooted(value) || value.StartsWith("/") || (value.Length >= 2 && value[1] == ':'))
        {
            errors.Add(Error.Validation("Profile image path cannot be a rooted path", "ERR_IMG_PATH_ROOTED", "ProfileImagePath"));
        }

        if (value.Contains(".."))
        {
            errors.Add(Error.Validation("Profile image path cannot contain '..'", "ERR_IMG_PATH_DOTDOT", "ProfileImagePath"));
        }

        if (Path.GetInvalidPathChars().Any(c => value.Contains(c)) || value.IndexOfAny(CommonInvalidPathChars) >= 0)
        {
            errors.Add(Error.Validation("Profile image path contains invalid characters", "ERR_IMG_PATH_INV_CHAR", "ProfileImagePath"));
        }

        if (AllowedExtensions.All(ext => !value.EndsWith(ext)))
        {
            errors.Add(Error.Validation("Profile image path has an invalid extension", "ERR_IMG_PATH_EXT", "ProfileImagePath"));
        }

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }
    }

}
namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct ProfileImagePath
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public string Value { get; init; }

    public ProfileImagePath(string value)
    {
        ValidateProfileImagePath(value);
        Value = value;
    }

    public static void ValidateProfileImagePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Profile image path cannot be empty", nameof(value));
        }

        if (value.Length > 255)
        {
            throw new ArgumentException("Profile image path cannot be longer than 255 characters", nameof(value));
        }

        if (Path.IsPathRooted(value))
        {
            throw new ArgumentException("Profile image path cannot be a rooted path", nameof(value));
        }

        if (value.Contains(".."))
        {
            throw new ArgumentException("Profile image path cannot contain '..'", nameof(value));
        }

        if (Path.GetInvalidPathChars().Any(value.Contains))
        {
            throw new ArgumentException("Profile image path contains invalid characters", nameof(value));
        }

        if (AllowedExtensions.All(ext => !value.EndsWith(ext)))
        {
            throw new ArgumentException("Profile image path has an invalid extension", nameof(value));
        }
    }

}
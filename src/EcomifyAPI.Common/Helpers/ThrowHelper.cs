using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EcomifyAPI.Common.Helpers;

public class ThrowHelper
{
    public static void ThrowIfNull<T>([NotNull] T? value, string message = "",
        [CallerArgumentExpression(nameof(value))] string valueExpression = "Not provided")
    {
        _ = value ?? throw new ArgumentNullException(message ?? $"{valueExpression} cannot be null");
    }
}
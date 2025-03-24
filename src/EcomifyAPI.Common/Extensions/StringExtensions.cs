namespace EcomifyAPI.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Capitalizes the first letter of the given string.
    /// </summary>
    /// <param name="str">The string whose first letter is to be capitalized.</param>
    /// <returns>
    /// The input string with its first letter capitalized if the string is not null or empty; 
    /// otherwise, returns the original string.
    /// </returns>
    /// <example>
    /// <code>
    /// string example = "hello";
    /// string result = example.CapitalizeFirstLetter(); // Result: "Hello"
    /// </code>
    /// </example>
    public static string CapitalizeFirstLetter(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return str.ToUpper();
        }

        return string.Format("{0}{1}", char.ToUpper(str[0]), str.Substring(1));
    }
}
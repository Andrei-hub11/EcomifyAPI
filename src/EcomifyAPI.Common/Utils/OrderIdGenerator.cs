using System.Text;

namespace EcomifyAPI.Common.Utils;

public static class OrderIdGenerator
{
    private static readonly Random _random = new();
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Numbers = "0123456789";

    public static string GenerateOrderId()
    {
        string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        string randomPart = GenerateRandomString(3, Letters) + GenerateRandomString(3, Numbers);

        return $"ORD-{datePart}-{randomPart}";
    }

    private static string GenerateRandomString(int length, string charset)
    {
        StringBuilder result = new();
        for (int i = 0; i < length; i++)
        {
            result.Append(charset[_random.Next(charset.Length)]);
        }
        return result.ToString();
    }
}
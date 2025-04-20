using System.ComponentModel;
using System.Reflection;

namespace EcomifyAPI.Common.Extensions.Enums;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field is null)
        {
            return "Unknown";
        }

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute is not null ? attribute.Description : value.ToString();
    }
}
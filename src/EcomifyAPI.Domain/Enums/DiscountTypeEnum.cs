using System.ComponentModel;

namespace EcomifyAPI.Domain.Enums;

public enum DiscountType
{
    [Description("Fixed")]
    Fixed = 1,
    [Description("Percentage")]
    Percentage = 2,
    [Description("Coupon")]
    Coupon = 3
}
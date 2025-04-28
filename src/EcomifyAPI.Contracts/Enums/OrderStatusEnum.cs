using System.ComponentModel;

namespace EcomifyAPI.Contracts.Enums;

public enum OrderStatusDTO
{
    [Description("Confirmed")]
    Confirmed = 1,
    [Description("Shipped")]
    Shipped = 2,
    [Description("Completed")]
    Completed = 3,
    [Description("Cancelled")]
    Cancelled = 4,
    [Description("Refunded")]
    Refunded = 5
}
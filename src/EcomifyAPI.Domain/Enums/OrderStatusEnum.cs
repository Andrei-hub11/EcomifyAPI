namespace EcomifyAPI.Domain.Enums;

public enum OrderStatusEnum
{
    Created = 1,
    Pending = 2,
    Processing = 3,
    Confirmed = 4,
    Failed = 5,
    Shipped = 6,
    Completed = 7,
    Cancelled = 8,
}
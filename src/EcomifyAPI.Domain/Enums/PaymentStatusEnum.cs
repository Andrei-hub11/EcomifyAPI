namespace EcomifyAPI.Domain.Enums;

public enum PaymentStatusEnum
{
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    RefundRequested = 4,
    Refunded = 5,
    Cancelled = 6
}
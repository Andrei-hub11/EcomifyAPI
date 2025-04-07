using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Domain.ValueObjects;

public class PaymentStatusChange
{
    public Guid Id { get; }
    public PaymentStatusEnum Status { get; }
    public DateTime Timestamp { get; }
    public string Reference { get; }

    public PaymentStatusChange(Guid id, PaymentStatusEnum status, DateTime timestamp, string reference)
    {
        Id = id;
        Status = status;
        Timestamp = timestamp;
        Reference = reference;
    }
}
using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public sealed class PaymentRecordHistoryMapping
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public PaymentStatusDTO Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string Reference { get; set; } = string.Empty;
}
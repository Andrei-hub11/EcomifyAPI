using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public sealed class PaymentRecordMapping
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "BRL";
    public PaymentMethodEnumDTO PaymentMethod { get; set; }
    public Guid TransactionId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public PaymentStatusDTO Status { get; set; }
    public string GatewayResponse { get; set; } = string.Empty;
    public string CcLastFourDigits { get; set; } = string.Empty;
    public string CcBrand { get; set; } = string.Empty;
    public string PaypalEmail { get; set; } = string.Empty;
    public string PaypalPayerId { get; set; } = string.Empty;
    public IEnumerable<PaymentRecordHistoryMapping> StatusHistory { get; set; } = [];
}
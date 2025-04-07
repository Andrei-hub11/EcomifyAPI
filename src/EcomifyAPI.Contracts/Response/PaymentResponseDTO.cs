using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Response;

public sealed record PaymentResponseDTO(
    Guid TransactionId,
    decimal Amount,
    PaymentMethodEnumDTO PaymentMethod,
    DateTime ProcessedAt,
    PaymentStatusDTO Status,
    string? GatewayResponse,
    string? CcLastFourDigits,
    string? CcBrand,
    string? PaypalEmail,
    IReadOnlyList<PaymentStatusHistoryResponseDTO> StatusHistory
);
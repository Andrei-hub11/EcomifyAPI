using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Response;

public sealed record PaymentStatusHistoryResponseDTO(
    Guid Id,
    PaymentStatusDTO Status,
    DateTime Timestamp,
    string Reference
    );
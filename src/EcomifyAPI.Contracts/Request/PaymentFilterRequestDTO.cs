using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public record PaymentFilterRequestDTO(
    int PageSize,
    int PageNumber,
    string? CustomerId,
    decimal? Amount,
    PaymentStatusDTO? Status,
    PaymentMethodEnumDTO? PaymentMethod,
    string? PaymentReference,
    DateTime? StartDate,
    DateTime? EndDate
);
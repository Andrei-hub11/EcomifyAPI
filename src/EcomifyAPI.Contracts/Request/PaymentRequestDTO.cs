using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record PaymentRequestDTO(
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentMethodEnumDTO PaymentMethod,
    CreditCardDetailsDTO? CreditCardDetails,
    PayPalDetailsDTO? PayPalDetails
);
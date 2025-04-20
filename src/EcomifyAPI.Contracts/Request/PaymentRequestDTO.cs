using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record PaymentRequestDTO(
    string UserId,
    string Currency,
    PaymentMethodEnumDTO PaymentMethod,
    CreditCardDetailsDTO? CreditCardDetails,
    PayPalDetailsDTO? PayPalDetails,
    AddressRequestDTO ShippingAddress,
    AddressRequestDTO BillingAddress
);
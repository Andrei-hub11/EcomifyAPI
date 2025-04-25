using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record PaymentRequestDTO(
    string UserId,
    PaymentMethodEnumDTO PaymentMethod,
    CreditCardDetailsDTO? CreditCardDetails,
    PayPalDetailsDTO? PayPalDetails,
    AddressRequestDTO ShippingAddress,
    AddressRequestDTO BillingAddress
);
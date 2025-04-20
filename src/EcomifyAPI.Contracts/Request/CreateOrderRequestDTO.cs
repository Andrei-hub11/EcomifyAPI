namespace EcomifyAPI.Contracts.Request;

public sealed record CreateOrderRequestDTO(
    string UserId,
    AddressRequestDTO ShippingAddress,
    AddressRequestDTO BillingAddress
);
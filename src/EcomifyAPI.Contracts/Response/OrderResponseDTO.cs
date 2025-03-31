using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record OrderResponseDTO(
    Guid Id,
    string UserId,
    DateTime OrderDate,
    OrderStatusDTO Status,
    decimal TotalAmount,
    string CurrencyCode,
    AddressDTO ShippingAddress,
    AddressDTO BillingAddress,
    IReadOnlyList<OrderItemDTO> Items
    );
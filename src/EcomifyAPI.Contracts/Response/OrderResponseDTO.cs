using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record OrderResponseDTO(
    Guid Id,
    string UserId,
    DateTime OrderDate,
    OrderStatusDTO Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TotalWithDiscount,
    string CurrencyCode,
    AddressResponseDTO ShippingAddress,
    AddressResponseDTO BillingAddress,
    IReadOnlyList<OrderItemDTO> Items,
    DateTime? ShippedAt
    );
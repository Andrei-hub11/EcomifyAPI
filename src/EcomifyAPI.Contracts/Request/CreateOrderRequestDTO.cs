using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Request;

public sealed record CreateOrderRequestDTO(
    string UserId,
    OrderStatusEnum Status,
    AddressDTO ShippingAddress,
    AddressDTO BillingAddress,
    IReadOnlyList<OrderItemDTO> Items
);
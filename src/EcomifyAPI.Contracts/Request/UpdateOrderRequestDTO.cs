using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateOrderRequestDTO(
    Guid OrderId,
    OrderStatusDTO Status,
    AddressRequestDTO ShippingAddress,
    AddressRequestDTO BillingAddress,
    List<OrderItemDTO> ItemsToUpdate,
    List<Guid> ItemsToRemove
);
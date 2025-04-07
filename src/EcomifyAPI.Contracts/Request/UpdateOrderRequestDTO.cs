using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateOrderRequestDTO(
    Guid OrderId,
    OrderStatusDTO Status,
    AddressDTO ShippingAddress,
    AddressDTO BillingAddress,
    List<OrderItemDTO> ItemsToUpdate,
    List<Guid> ItemsToRemove
);
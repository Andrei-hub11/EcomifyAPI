using EcomifyAPI.Contracts.Models;
namespace EcomifyAPI.Contracts.EmailModels;

public sealed record DeliveryConfirmationEmail(
    string CustomerName,
    Guid OrderId,
    DateTime OrderDate,
    DateTime DeliveryDate,
    string RecipientName,
    string DeliveryCompany,
    string TrackingNumber,
    string Currency,
    IReadOnlyList<OrderItemDTO> OrderItems,
    decimal Subtotal,
    decimal ShippingCost,
    decimal Discount,
    decimal Total,
    AddressResponseDTO DeliveryAddress
);
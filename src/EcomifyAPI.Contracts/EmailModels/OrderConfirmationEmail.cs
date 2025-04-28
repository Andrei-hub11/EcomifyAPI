using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.EmailModels;

public sealed record OrderConfirmationEmail(
    string CustomerName,
    Guid OrderId,
    DateTime OrderDate,
    string PaymentMethod,
    string OrderStatus,
    string TotalFormatted,
    IReadOnlyList<OrderItemDTO> OrderItems,
    AddressResponseDTO ShippingAddress,
    string ShippingMethod,
    DateTime EstimatedDeliveryDate,
    string OrderTrackingUrl,
    decimal Discount,
    decimal ShippingCost,
    decimal Subtotal,
    decimal Total,
    string Currency
);
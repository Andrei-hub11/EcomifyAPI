using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsOrders
{
    public static OrderStatusEnum ToOrderStatusDomain(this OrderStatusDTO status)
    {
        return status switch
        {
            OrderStatusDTO.Created => OrderStatusEnum.Created,
            OrderStatusDTO.Pending => OrderStatusEnum.Pending,
            OrderStatusDTO.Processing => OrderStatusEnum.Processing,
            OrderStatusDTO.Confirmed => OrderStatusEnum.Confirmed,
            OrderStatusDTO.Shipped => OrderStatusEnum.Shipped,
            OrderStatusDTO.Completed => OrderStatusEnum.Completed,
            OrderStatusDTO.Cancelled => OrderStatusEnum.Cancelled,
            _ => throw new ArgumentException("Invalid order status"),
        };
    }

    public static OrderStatusDTO ToOrderStatusDTO(this OrderStatusEnum status)
    {
        return status switch
        {
            OrderStatusEnum.Created => OrderStatusDTO.Created,
            OrderStatusEnum.Pending => OrderStatusDTO.Pending,
            OrderStatusEnum.Processing => OrderStatusDTO.Processing,
            OrderStatusEnum.Confirmed => OrderStatusDTO.Confirmed,
            OrderStatusEnum.Shipped => OrderStatusDTO.Shipped,
            OrderStatusEnum.Completed => OrderStatusDTO.Completed,
            OrderStatusEnum.Cancelled => OrderStatusDTO.Cancelled,
            _ => throw new ArgumentException("Invalid order status"),
        };
    }

    private static AddressDTO ToAddressDTO(this ShippingAddressMapping address)
    {
        return new AddressDTO(
            address.ShippingStreet,
            address.ShippingNumber,
            address.ShippingCity,
            address.ShippingState,
            address.ShippingZipCode,
            address.ShippingCountry,
            address.ShippingComplement);
    }

    private static AddressDTO ToAddressDTO(this BillingAddressMapping address)
    {
        return new AddressDTO(
            address.BillingStreet,
            address.BillingNumber,
            address.BillingCity,
            address.BillingState,
            address.BillingZipCode,
            address.BillingCountry,
            address.BillingComplement);
    }

    private static IReadOnlyList<OrderItemDTO> ToOrderItemDTO(this List<OrderItemMapping> items)
    {
        return [.. items.Select(item =>
        new OrderItemDTO(
            item.ProductId,
            item.Quantity,
            new CurrencyDTO(item.CurrencyCode, item.UnitPrice)))];
    }

    public static OrderResponseDTO ToResponseDTO(this OrderMapping order)
    {
        return new OrderResponseDTO(
            order.Id,
            order.UserId,
            order.OrderDate,
            order.Status,
            order.TotalAmount,
            order.CurrencyCode,
            order.ShippingAddress.ToAddressDTO(),
            order.BillingAddress.ToAddressDTO(),
            order.Items.ToOrderItemDTO()
        );
    }

    public static IReadOnlyList<OrderResponseDTO> ToResponseDTO(this IEnumerable<OrderMapping> orders)
    {
        return [.. orders.Select(order => order.ToResponseDTO())];
    }
}
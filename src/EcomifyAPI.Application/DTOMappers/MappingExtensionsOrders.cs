using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsOrders
{

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
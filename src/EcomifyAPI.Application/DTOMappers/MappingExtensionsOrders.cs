using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsOrders
{

    private static AddressDTO ToAddressDTO(this AddressMapping address)
    {
        return new AddressDTO(address.Street, address.Number, address.City, address.State, address.ZipCode, address.Country, address.Complement);
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
}
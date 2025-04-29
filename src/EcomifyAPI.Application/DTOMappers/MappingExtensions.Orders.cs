using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsOrders
{
    public static OrderStatusEnum ToOrderStatusDomain(this OrderStatusDTO status)
    {
        return status switch
        {
            OrderStatusDTO.Confirmed => OrderStatusEnum.Confirmed,
            OrderStatusDTO.Shipped => OrderStatusEnum.Shipped,
            OrderStatusDTO.Completed => OrderStatusEnum.Completed,
            OrderStatusDTO.Cancelled => OrderStatusEnum.Cancelled,
            OrderStatusDTO.Refunded => OrderStatusEnum.Refunded,
            _ => throw new ArgumentException("Invalid order status"),
        };
    }

    public static OrderStatusDTO ToOrderStatusDTO(this OrderStatusEnum status)
    {
        return status switch
        {
            OrderStatusEnum.Confirmed => OrderStatusDTO.Confirmed,
            OrderStatusEnum.Shipped => OrderStatusDTO.Shipped,
            OrderStatusEnum.Completed => OrderStatusDTO.Completed,
            OrderStatusEnum.Cancelled => OrderStatusDTO.Cancelled,
            OrderStatusEnum.Refunded => OrderStatusDTO.Refunded,
            _ => throw new ArgumentException("Invalid order status"),
        };
    }

    private static AddressResponseDTO ToAddressDTO(this ShippingAddressMapping address)
    {
        return new AddressResponseDTO(
            null,
            address.ShippingStreet,
            address.ShippingNumber,
            address.ShippingCity,
            address.ShippingState,
            address.ShippingZipCode,
            address.ShippingCountry,
            address.ShippingComplement);
    }

    private static AddressResponseDTO ToAddressDTO(this BillingAddressMapping address)
    {
        return new AddressResponseDTO(
            null,
            address.BillingStreet,
            address.BillingNumber,
            address.BillingCity,
            address.BillingState,
            address.BillingZipCode,
            address.BillingCountry,
            address.BillingComplement);
    }

    private static AddressRequestDTO ToAddressRequestDTO(this Address address)
    {
        return new AddressRequestDTO(
            address.Street,
            address.Number,
            address.City,
            address.State,
            address.ZipCode,
            address.Country,
            address.Complement
            );
    }

    private static List<OrderItem> ToOrderItem(this List<OrderItemMapping> items)
    {
        return [.. items.Select(item =>
        new OrderItem(
            item.OrderId,
            item.ProductId,
            item.Quantity,
            new Money(item.CurrencyCode, item.UnitPrice)))];
    }

    private static IReadOnlyList<OrderItemDTO> ToOrderItemDTO(this List<OrderItemMapping> items)
    {
        return [.. items.Select(item =>
        new OrderItemDTO(
            item.ItemId,
            item.ProductId,
            item.ProductName,
            item.Quantity,
            new MoneyDTO(item.CurrencyCode, item.UnitPrice),
            item.TotalPrice))];
    }

    public static DiscountHistoryDTO ToDTO(this DiscountHistory discountHistory)
    {
        return new DiscountHistoryDTO(
            discountHistory.Id,
            discountHistory.OrderId,
            discountHistory.CustomerId,
            discountHistory.DiscountId,
            (DiscountTypeEnum)discountHistory.DiscountType,
            discountHistory.DiscountAmount,
            discountHistory.Percentage,
            discountHistory.FixedAmount,
            discountHistory.CouponCode,
            discountHistory.AppliedAt);
    }

    public static IReadOnlyList<DiscountHistoryDTO> ToDTO(this IEnumerable<DiscountHistory> discountHistories)
    {
        return [.. discountHistories.Select(d => d.ToDTO())];
    }

    public static DiscountHistoryDTO ToDTO(this DiscountHistoryMapping mapping)
    {
        return new DiscountHistoryDTO(
            mapping.Id,
            mapping.OrderId,
            mapping.CustomerId,
            mapping.DiscountId,
            (DiscountTypeEnum)mapping.DiscountType,
            mapping.DiscountAmount,
            mapping.Percentage,
            mapping.FixedAmount,
            mapping.CouponCode,
            mapping.AppliedAt);
    }

    public static IReadOnlyList<DiscountHistoryDTO> ToDTO(this IEnumerable<DiscountHistoryMapping> mappings)
    {
        return [.. mappings.Select(m => m.ToDTO())];
    }

    public static OrderResponseDTO ToResponseDTO(this OrderMapping order)
    {
        return new OrderResponseDTO(
            order.Id,
            order.UserId,
            order.OrderDate,
            order.Status,
            order.TotalAmount,
            order.DiscountAmount,
            order.TotalWithDiscount,
            order.CurrencyCode,
            order.ShippingAddress.ToAddressDTO(),
            order.BillingAddress.ToAddressDTO(),
            order.Items.ToOrderItemDTO()
        );
    }

    public static PaginatedResponseDTO<OrderResponseDTO> ToResponseDTO(this IEnumerable<OrderMapping> orders, int pageNumber, int pageSize, long totalCount)
    {
        return new PaginatedResponseDTO<OrderResponseDTO>(
            [.. orders.Select(order => order.ToResponseDTO())],
            pageSize,
            pageNumber,
            totalCount
        );
    }

    public static IReadOnlyList<OrderResponseDTO> ToResponseDTO(this IEnumerable<OrderMapping> orders)
    {
        return [.. orders.Select(order => order.ToResponseDTO())];
    }

    public static Order ToDomain(this OrderMapping order)
    {
        var domain = Order.From(
            order.Id,
            order.UserId,
            order.OrderDate,
            order.Status.ToOrderStatusDomain(),
            order.CreatedAt,
            order.CompletedAt,
            new Address(
                order.ShippingAddress.ShippingStreet,
                order.ShippingAddress.ShippingNumber,
                order.ShippingAddress.ShippingCity,
                order.ShippingAddress.ShippingState,
                order.ShippingAddress.ShippingZipCode,
                order.ShippingAddress.ShippingCountry,
                order.ShippingAddress.ShippingComplement),
            new Address(
                order.BillingAddress.BillingStreet,
                order.BillingAddress.BillingNumber,
                order.BillingAddress.BillingCity,
                order.BillingAddress.BillingState,
                order.BillingAddress.BillingZipCode,
                order.BillingAddress.BillingCountry,
                order.BillingAddress.BillingComplement),
            order.Items.ToOrderItem(),
            order.DiscountAmount
        );

        if (domain.IsFailure)
        {
            throw new BadRequestException(string.Join(", ", domain.Errors));
        }

        return domain.Value;
    }

    public static IReadOnlyList<Order> ToDomain(this IEnumerable<OrderMapping> orders)
    {
        return [.. orders.Select(order => order.ToDomain())];
    }
}
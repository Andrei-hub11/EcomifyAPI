using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsCarts
{

    private static decimal GetAmountDiscount(this DiscountCartMapping dto)
    {
        return dto.DiscountType switch
        {
            DiscountTypeEnum.Fixed => dto.FixedAmount ?? 0,
            DiscountTypeEnum.Percentage => dto.Percentage ?? 0,
            DiscountTypeEnum.Coupon => dto.Percentage ?? dto.FixedAmount
            ?? throw new InvalidOperationException($"Sometthing is wrong with the discount with id = '{dto.Id}'"),
            _ => throw new InvalidOperationException($"Sometthing is wrong with the discount with id = '{dto.Id}'")
        };
    }

    public static CartDiscount ToDomain(this DiscountCartMapping dto)
    {
        return new CartDiscount(dto.Id, new Money("BRL", dto.GetAmountDiscount()),
        (DiscountType)dto.DiscountType, dto.ValidFrom, dto.ValidTo);
    }

    public static List<CartDiscount> ToDomain(this IEnumerable<DiscountCartMapping> dtos)
    {
        return !dtos.Any() ? [] : [.. dtos.Select(d => d.ToDomain())];
    }

    public static CartItem ToDomain(this CartItemMapping dto)
    {
        return new CartItem(dto.ProductId, dto.Quantity, new Money(dto.ItemCurrencyCode, dto.UnitPrice));
    }

    public static List<CartItem> ToDomain(this IEnumerable<CartItemMapping> dtos)
    {
        return !dtos.Any() ? [] : [.. dtos.Select(i => i.ToDomain())];
    }

    private static CartItemResponseDTO ToCartItemResponseDTO(this CartItem item)
    {
        return new CartItemResponseDTO(
            item.Id,
            item.ProductId,
            item.Quantity,
            new MoneyDTO(item.UnitPrice.Code, item.UnitPrice.Amount),
            new MoneyDTO(item.TotalPrice.Code, item.TotalPrice.Amount)
        );
    }

    public static CartResponseDTO ToDTO(this Cart cart)
    {
        return new CartResponseDTO(
            cart.Id,
            cart.UserId,
            cart.Items.Count != 0 ? cart.Items.Select(i => i.ToCartItemResponseDTO()).ToList() : [],
            new MoneyDTO(cart.TotalAmount.Code, cart.TotalAmount.Amount),
            new MoneyDTO(cart.TotalWithDiscount.Code, cart.TotalWithDiscount.Amount),
            cart.CreatedAt,
            cart.UpdatedAt
        );
    }
}
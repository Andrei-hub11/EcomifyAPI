using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsCarts
{

    private static CartItemResponseDTO ToCartItemResponseDTO(this CartItem item)
    {
        return new CartItemResponseDTO(
            item.Id,
            item.ProductId,
            item.Quantity,
            new CurrencyDTO(item.UnitPrice.Code, item.UnitPrice.Amount),
            new CurrencyDTO(item.TotalPrice.Code, item.TotalPrice.Amount)
        );
    }

    public static CartResponseDTO ToDTO(this Cart cart)
    {
        return new CartResponseDTO(
            cart.Id,
            cart.UserId,
            cart.Items.Count != 0 ? cart.Items.Select(i => i.ToCartItemResponseDTO()).ToList() : [],
            new CurrencyDTO(cart.TotalAmount.Code, cart.TotalAmount.Amount),
            cart.CreatedAt,
            cart.UpdatedAt
        );
    }
}
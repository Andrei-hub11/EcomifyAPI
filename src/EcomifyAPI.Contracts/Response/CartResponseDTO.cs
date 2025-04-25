using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record CartResponseDTO(
    Guid Id,
    string UserId,
    IReadOnlyList<CartItemResponseDTO> Items,
    MoneyDTO TotalAmount,
    MoneyDTO? TotalWithDiscount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
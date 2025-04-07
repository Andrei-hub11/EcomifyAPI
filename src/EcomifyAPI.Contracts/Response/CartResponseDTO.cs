using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record CartResponseDTO(
    Guid Id,
    string UserId,
    IReadOnlyList<CartItemResponseDTO> Items,
    CurrencyDTO TotalAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
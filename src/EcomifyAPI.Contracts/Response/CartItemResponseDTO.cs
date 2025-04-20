using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record CartItemResponseDTO(
    Guid Id,
    Guid ProductId,
    int Quantity,
    MoneyDTO UnitPrice,
    MoneyDTO TotalPrice
);
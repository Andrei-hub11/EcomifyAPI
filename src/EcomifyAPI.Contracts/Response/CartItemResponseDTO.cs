using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record CartItemResponseDTO(
    Guid Id,
    Guid ProductId,
    ProductResponseDTO Product,
    int Quantity,
    MoneyDTO UnitPrice
);
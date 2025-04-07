using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record CartResposeDTO(
    Guid Id,
    string UserId,
    List<CartItemResponseDTO> Items,
    CurrencyDTO TotalAmount,
    DateTime CreatedAt
);
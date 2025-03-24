namespace EcomifyAPI.Contracts.Models;

public sealed record OrderItemDTO(
    Guid ProductId,
    int Quantity,
    CurrencyDTO UnitPrice
);
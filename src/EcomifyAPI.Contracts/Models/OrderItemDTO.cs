namespace EcomifyAPI.Contracts.Models;

public sealed record OrderItemDTO(
    Guid ItemId,
    Guid ProductId,
    int Quantity,
    CurrencyDTO UnitPrice
);
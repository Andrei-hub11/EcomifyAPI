namespace EcomifyAPI.Contracts.Models;

public sealed record OrderItemDTO(
    Guid ItemId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    MoneyDTO UnitPrice,
    decimal Subtotal
);
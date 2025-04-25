using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.EmailModels;

public sealed record OrderDetails(
    Guid OrderId,
    string TransactionId,
    MoneyDTO Amount,
    DateTime OrderDate,
    decimal TotalAmount,
    string CustomerName
);
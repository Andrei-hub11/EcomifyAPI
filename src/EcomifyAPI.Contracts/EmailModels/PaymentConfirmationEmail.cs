namespace EcomifyAPI.Contracts.EmailModels;

public sealed record PaymentConfirmationEmail(
    string OrderNumber,
    decimal OrderAmount,
    string Currency,
    string CustomerName
);
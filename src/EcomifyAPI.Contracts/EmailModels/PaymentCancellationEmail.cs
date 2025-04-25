namespace EcomifyAPI.Contracts.EmailModels;

public sealed record PaymentCancellationEmail(
    string OrderNumber,
    decimal OrderAmount,
    string Currency,
    string CustomerName,
    string CancellationReason
);
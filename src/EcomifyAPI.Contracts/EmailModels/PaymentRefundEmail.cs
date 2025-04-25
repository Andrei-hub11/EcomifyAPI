namespace EcomifyAPI.Contracts.EmailModels;

public sealed record PaymentRefundEmail(
    string OrderNumber,
    decimal OrderAmount,
    string Currency,
    string CustomerName,
    string RefundReason,
    DateTime EstimatedRefundDate
);
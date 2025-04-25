using EcomifyAPI.Common.Utils;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Domain.Entities;

public sealed class PaymentRecord
{
    public Guid PaymentId { get; private set; }
    public Guid OrderId { get; private set; }

    public Money Amount { get; private set; }
    public PaymentMethodEnum PaymentMethod { get; private set; }
    public Guid TransactionId { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public PaymentStatusEnum Status { get; private set; }

    public string GatewayResponse { get; private set; } = string.Empty;
    private readonly IPaymentMethodDetails _paymentMethodDetails = default!;

    private readonly List<PaymentStatusChange> _statusHistory = [];
    public IReadOnlyCollection<PaymentStatusChange> StatusHistory => _statusHistory.AsReadOnly();

    private PaymentRecord(
        Guid paymentId,
        Guid orderId,
        Money amount,
        PaymentMethodEnum paymentMethod,
        Guid transactionId,
        string gatewayResponse,
        IPaymentMethodDetails paymentMethodDetails)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
        Status = PaymentStatusEnum.Processing;
        GatewayResponse = gatewayResponse;
        _paymentMethodDetails = paymentMethodDetails;

        if (paymentId == Guid.Empty)
        {
            _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.Processing, DateTime.UtcNow,
            OrderIdGenerator.GenerateOrderId()));
        }
    }

    public static PaymentRecord CreateCreditCardPayment(
        Guid orderId,
        Money amount,
        Guid transactionId,
        string lastFourDigits,
        string cardBrand,
        string gatewayResponse)
    {
        var details = new CreditCardDetails(lastFourDigits, cardBrand);
        return new PaymentRecord(Guid.Empty, orderId, amount, PaymentMethodEnum.CreditCard, transactionId, gatewayResponse, details);
    }

    public static PaymentRecord CreatePayPalPayment(
        Guid orderId,
        Money amount,
        Guid transactionId,
        string paypalEmail,
        string paypalPayerId,
        string gatewayResponse)
    {
        var details = new PayPalDetails(paypalEmail, paypalPayerId);
        return new PaymentRecord(Guid.Empty, orderId, amount, PaymentMethodEnum.PayPal, transactionId, gatewayResponse, details);
    }

    public static PaymentRecord From(
        Guid paymentId,
        Guid orderId,
        Money amount,
        PaymentMethodEnum paymentMethod,
        Guid transactionId,
        string gatewayResponse,
        IPaymentMethodDetails paymentMethodDetails)
    {
        return new PaymentRecord(paymentId, orderId, amount, paymentMethod, transactionId, gatewayResponse, paymentMethodDetails);
    }

    public CreditCardDetails? GetCreditCardDetails()
    {
        if (PaymentMethod != PaymentMethodEnum.CreditCard)
            throw new InvalidOperationException("This payment was not made with a credit card");

        if (_paymentMethodDetails is CreditCardDetails creditCardDetails)
            return creditCardDetails;

        return null;
    }

    public PayPalDetails? GetPayPalDetails()
    {
        if (PaymentMethod != PaymentMethodEnum.PayPal)
            throw new InvalidOperationException("This payment was not made with PayPal");

        if (_paymentMethodDetails is PayPalDetails payPalDetails)
            return payPalDetails;

        return null;
    }

    public Result<bool> MarkAsSucceeded(string gatewayReference)
    {
        if (Status == PaymentStatusEnum.Succeeded || Status == PaymentStatusEnum.Refunded)
            return Result.Fail("Impossible to mark as succeeded a payment already finalized");

        Status = PaymentStatusEnum.Succeeded;
        _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.Succeeded, DateTime.UtcNow, gatewayReference));

        return true;
    }

    public Result<bool> MarkAsFailed(string reason)
    {
        if (Status == PaymentStatusEnum.Succeeded || Status == PaymentStatusEnum.Refunded)
            return Result.Fail("Impossible to mark as failed a payment already finalized");

        Status = PaymentStatusEnum.Failed;
        _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.Failed, DateTime.UtcNow, reason));

        return true;
    }

    public Result<bool> RequestRefund(decimal refundAmount, string reason)
    {
        if (Status != PaymentStatusEnum.Succeeded)
            return Result.Fail("Only succeeded payments can be refunded");

        if (refundAmount > Amount.Amount)
            return Result.Fail("Refund amount is greater than the payment amount");

        Status = PaymentStatusEnum.RefundRequested;
        _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.RefundRequested, DateTime.UtcNow, reason));

        return true;
    }

    public Result<bool> ConfirmRefund(string gatewayRefundId)
    {
        if (Status != PaymentStatusEnum.RefundRequested)
            return Result.Fail("Only payments with refund requested can be confirmed as refunded");

        Status = PaymentStatusEnum.Refunded;
        _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.Refunded, DateTime.UtcNow, gatewayRefundId));

        return true;
    }

    public Result<bool> MarkAsCancelled(string reason)
    {
        if (Status == PaymentStatusEnum.Refunded || Status == PaymentStatusEnum.Cancelled)
            return Result.Fail("Payment has already been finalized and cannot be cancelled");

        Status = PaymentStatusEnum.Cancelled;
        _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.Cancelled, DateTime.UtcNow, reason));

        return true;
    }

    public Result<bool> MarkAsRefunded(string reason)
    {
        if (Status == PaymentStatusEnum.Refunded || Status == PaymentStatusEnum.Cancelled)
            return Result.Fail("Payment has already been finalized and cannot be refunded");

        Status = PaymentStatusEnum.Refunded;
        _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), PaymentStatusEnum.Refunded, DateTime.UtcNow, reason));

        return true;
    }

    public Result<bool> UpdateFromGateway(string gatewayStatus, string gatewayReference)
    {
        PaymentStatusEnum newStatus = MapGatewayStatusToPaymentStatus(gatewayStatus);

        if (IsValidStatusTransition(Status, newStatus))
        {
            Status = newStatus;
            _statusHistory.Add(new PaymentStatusChange(Guid.NewGuid(), newStatus, DateTime.UtcNow, gatewayReference));
            return true;
        }

        return Result.Fail($"Invalid status transition: {Status} -> {newStatus}");
    }

    private PaymentStatusEnum MapGatewayStatusToPaymentStatus(string gatewayStatus)
    {
        return PaymentMethod switch
        {
            PaymentMethodEnum.PayPal => MapPayPalStatus(gatewayStatus),
            PaymentMethodEnum.CreditCard => MapCreditCardStatus(gatewayStatus),
            _ => throw new NotSupportedException($"Payment method not supported: {PaymentMethod}")
        };
    }

    private PaymentStatusEnum MapPayPalStatus(string paypalStatus)
    {
        return paypalStatus.ToLower() switch
        {
            "completed" => PaymentStatusEnum.Succeeded,
            "pending" => PaymentStatusEnum.Processing,
            "failed" => PaymentStatusEnum.Failed,
            "refunded" => PaymentStatusEnum.Refunded,
            "cancelled" => PaymentStatusEnum.Cancelled,
            _ => PaymentStatusEnum.Unknown
        };
    }

    private PaymentStatusEnum MapCreditCardStatus(string cardStatus)
    {
        return cardStatus.ToLower() switch
        {
            "approved" => PaymentStatusEnum.Succeeded,
            "pending" => PaymentStatusEnum.Processing,
            "declined" => PaymentStatusEnum.Failed,
            "refunded" => PaymentStatusEnum.Refunded,
            "cancelled" => PaymentStatusEnum.Cancelled,
            _ => PaymentStatusEnum.Unknown
        };
    }

    private bool IsValidStatusTransition(PaymentStatusEnum currentStatus, PaymentStatusEnum newStatus)
    {
        if (currentStatus == PaymentStatusEnum.Refunded)
            return false;

        if (currentStatus == PaymentStatusEnum.Succeeded && newStatus == PaymentStatusEnum.Failed)
            return false;

        return true;
    }
}
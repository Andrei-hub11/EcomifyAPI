using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Email;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Contracts.EmailModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IPaymentMethodFactory _paymentMethodFactory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IAccountService _accountService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;

    public PaymentService(
        IPaymentMethodFactory paymentMethodFactory,
        IOrderService orderService,
        ICartService cartService,
        IAccountService accountService,
        IUnitOfWork unitOfWork,
        IEmailSender emailSender)
    {
        _paymentMethodFactory = paymentMethodFactory;
        _orderService = orderService;
        _cartService = cartService;
        _accountService = accountService;
        _unitOfWork = unitOfWork;
        _paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
        _orderRepository = _unitOfWork.GetRepository<IOrderRepository>();
        _emailSender = emailSender;
    }

    public async Task<Result<PaymentResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

        if (payment is null)
        {
            return Result.Fail(PaymentErrorFactory.PaymentNotFound(id));
        }

        return Result.Ok(payment.ToDTO());
    }

    public async Task<Result<IReadOnlyList<PaymentResponseDTO>>> GetAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetAsync(cancellationToken);

        return Result.Ok(payments.ToDTO());
    }

    public async Task<Result<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await _cartService.GetCartAsync(request.UserId, cancellationToken);

            if (cart.IsFailure)
            {
                return Result.Fail(cart.Errors);
            }

            if (cart.Value.Items.Count == 0)
            {
                return Result.Fail(OrderErrorFactory.CartEmpty());
            }

            decimal amount = cart.Value.TotalWithDiscount?.Amount ?? cart.Value.TotalAmount.Amount;
            var currency = cart.Value.TotalWithDiscount?.Code ?? cart.Value.TotalAmount.Code;

            // Simulate a delay in the payment process
            await Task.Delay(2000, cancellationToken);

            var paymentMethod = _paymentMethodFactory.GetPaymentMethod(request.PaymentMethod);

            if (paymentMethod is null)
            {
                return Result.Fail(PaymentErrorFactory.InvalidPaymentMethod(request.PaymentMethod.ToString()));
            }

            PaymentDetails paymentDetails = request.CreditCardDetails as PaymentDetails
          ?? request.PayPalDetails as PaymentDetails
          ?? throw new InvalidOperationException("Invalid payment method");

            var paymentResult = await paymentMethod.ProcessPaymentAsync(paymentDetails, cancellationToken);

            if (paymentResult.IsFailure)
            {
                return Result.Fail(PaymentErrorFactory.PaymentFailed());
            }

            // Create order after payment is confirmed
            var userInfo = await _accountService.GetByIdAsync(request.UserId, cancellationToken);

            if (userInfo.IsFailure)
            {
                return Result.Fail(userInfo.Errors);
            }

            // Use shipping and billing addresses from the request
            // Create order
            var createOrderRequest = new CreateOrderRequestDTO(
                request.UserId,
                request.ShippingAddress,
                request.BillingAddress
            );

            var orderResult = await _orderService.CreateOrderAsync(createOrderRequest, cancellationToken);

            if (orderResult.IsFailure)
            {
                return Result.Fail(orderResult.Errors);
            }

            var order = orderResult.Value;

            // Now create the payment record with the real order ID
            Result<PaymentRecord> paymentRecord = default!;

            if (request.PaymentMethod == PaymentMethodEnumDTO.CreditCard && request.CreditCardDetails is CreditCardDetailsDTO creditCardDetails)
            {
                paymentRecord = PaymentRecord.CreateCreditCardPayment(
                    order.Id,
                    amount > 0 ? new Money(currency, amount) : new Money(currency, order.TotalAmount),
                    paymentResult.Value.TransactionId,
                    creditCardDetails.CardNumber.Substring(creditCardDetails.CardNumber.Length - 4),
                    GetCardBrand(creditCardDetails.CardNumber),
                    paymentResult.Value.Reference
                );
            }

            if (request.PaymentMethod == PaymentMethodEnumDTO.PayPal && request.PayPalDetails is PayPalDetailsDTO payPalDetails)
            {
                paymentRecord = PaymentRecord.CreatePayPalPayment(
                    order.Id,
                    amount > 0 ? new Money(currency, amount) : new Money(currency, order.TotalAmount),
                    paymentResult.Value.TransactionId,
                    payPalDetails.PayerEmail,
                    payPalDetails.PayerId.ToString(),
                    paymentResult.Value.Reference
                );
            }

            if (paymentRecord.IsFailure)
            {
                return Result.Fail(paymentRecord.Errors);
            }

            paymentRecord.Value.MarkAsSucceeded(paymentResult.Value.Reference);

            // Create payment record
            var paymentId = await _paymentRepository.CreateAsync(paymentRecord.Value, cancellationToken);

            foreach (var item in paymentRecord.Value.StatusHistory)
            {
                await _paymentRepository.CreateStatusHistoryAsync(paymentId, item, cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(new PaymentResponseDTO(
                paymentRecord.Value.TransactionId,
                paymentRecord.Value.Amount.Amount,
                paymentRecord.Value.PaymentMethod.ToDTO(),
                paymentRecord.Value.ProcessedAt,
                paymentRecord.Value.Status.ToDTO(),
                paymentRecord.Value.GatewayResponse,
                paymentRecord.Value.PaymentMethod
                    == PaymentMethodEnum.CreditCard ? paymentRecord.Value.GetCreditCardDetails()?.LastFourDigits : null,
                paymentRecord.Value.PaymentMethod
                    == PaymentMethodEnum.CreditCard ? paymentRecord.Value.GetCreditCardDetails()?.CardBrand : null,
                paymentRecord.Value.PaymentMethod
                    == PaymentMethodEnum.PayPal ? paymentRecord.Value.GetPayPalDetails()?.PayPalEmail.Value : null,
                [.. paymentRecord.Value.StatusHistory.Select(statusHistory => statusHistory.ToDTO())]
            ));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<bool>> CancelPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

            if (payment is null)
            {
                return Result.Fail(PaymentErrorFactory.PaymentNotFound(id));
            }

            // Check if payment can be canceled (only if not already refunded/canceled)
            if (payment.Status == PaymentStatusDTO.Refunded || payment.Status == PaymentStatusDTO.Cancelled)
            {
                return Result.Fail(PaymentErrorFactory.PaymentAlreadyProcessed(payment.PaymentId));
            }

            // Get order details
            var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);

            if (order is null)
            {
                return Result.Fail(OrderErrorFactory.OrderNotFound(payment.OrderId));
            }

            // Prevent cancellation if the order is already shipped or completed
            if (order.Status.ToOrderStatusDomain() == OrderStatusEnum.Shipped ||
                order.Status.ToOrderStatusDomain() == OrderStatusEnum.Completed)
            {
                return Result.Fail(
                    Error.Failure(
                        $"Cannot cancel payment for order that has already been {order.Status.ToString().ToLower()}.",
                        "ERR_ORDER_ALREADY_PROCESSED"
                    )
                );
            }

            // Get user info
            var userResult = await _accountService.GetByIdAsync(order.UserId, cancellationToken);

            if (userResult.IsFailure)
            {
                return Result.Fail(userResult.Errors);
            }

            // Add some delay to simulate payment gateway communication
            await Task.Delay(1000, cancellationToken);

            // Update payment status
            var paymentDomain = payment.ToDomain();
            var cancellationReason = "Payment cancelled by user request";
            paymentDomain.MarkAsCancelled(cancellationReason);

            // Update payment in database
            await _paymentRepository.UpdateAsync(paymentDomain, cancellationToken);

            // Add status history entry
            await _paymentRepository.CreateStatusHistoryAsync(
                payment.PaymentId,
                new PaymentStatusChange(
                    Guid.NewGuid(),
                    PaymentStatusEnum.Cancelled,
                    DateTime.UtcNow,
                    cancellationReason
                ),
                cancellationToken
            );

            // Update order status
            await _orderService.UpdateStatusAsync(order.Id, OrderStatusEnum.Cancelled, cancellationToken);

            // Create an order details object for the email
            var orderDetails = new OrderDetails(
                order.Id,
                payment.TransactionId.ToString(),
                new MoneyDTO(payment.CurrencyCode, payment.Amount),
                order.OrderDate,
                order.TotalAmount,
                userResult.Value.User.UserName);

            // Send email notification
            await _emailSender.SendPaymentCancellationEmail(userResult.Value.User.Email, orderDetails);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<bool>> RefundPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

            if (payment is null)
            {
                return Result.Fail(PaymentErrorFactory.PaymentNotFound(id));
            }

            // Check if payment can be refunded (only if not already refunded/canceled)
            if (payment.Status == PaymentStatusDTO.Refunded || payment.Status == PaymentStatusDTO.Cancelled)
            {
                return Result.Fail(PaymentErrorFactory.PaymentAlreadyProcessed(payment.PaymentId));
            }

            // Get order details
            var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);

            if (order is null)
            {
                return Result.Fail(OrderErrorFactory.OrderNotFound(payment.OrderId));
            }

            // Get user info
            var userResult = await _accountService.GetByIdAsync(order.UserId, cancellationToken);

            if (userResult.IsFailure)
            {
                return Result.Fail(userResult.Errors);
            }

            // Simulate payment gateway processing time
            await Task.Delay(2000, cancellationToken);

            // Update payment status
            var paymentDomain = payment.ToDomain();
            var refundReason = "Payment refunded - We hope to see you again soon!";
            paymentDomain.MarkAsRefunded(refundReason);

            // Update payment in database
            await _paymentRepository.UpdateAsync(paymentDomain, cancellationToken);

            // Add status history entry
            await _paymentRepository.CreateStatusHistoryAsync(
                payment.PaymentId,
                new PaymentStatusChange(
                    Guid.NewGuid(),
                    PaymentStatusEnum.Refunded,
                    DateTime.UtcNow,
                    refundReason
                ),
                cancellationToken
            );

            // Update order status (using Refunded status instead of Cancelled)
            await _orderService.UpdateStatusAsync(order.Id, OrderStatusEnum.Refunded, cancellationToken);

            // Create an order details object for the email
            var orderDetails = new OrderDetails(
                order.Id,
                payment.TransactionId.ToString(),
                new MoneyDTO(payment.CurrencyCode, payment.Amount),
                order.OrderDate,
                order.TotalAmount,
                userResult.Value.User.UserName);

            // Send email notification
            await _emailSender.SendPaymentRefundEmail(userResult.Value.User.Email, orderDetails);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public string GetCardBrand(string cardNumber)
    {
        return cardNumber switch
        {
            _ when cardNumber.StartsWith("4") => "Visa",
            _ when cardNumber.StartsWith("5") => "MasterCard",
            _ when cardNumber.StartsWith("3") => "American Express",
            _ => "Unknown"
        };
    }
}
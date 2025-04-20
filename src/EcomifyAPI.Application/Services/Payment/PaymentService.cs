using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
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

    public PaymentService(
        IPaymentMethodFactory paymentMethodFactory,
        IOrderService orderService,
        ICartService cartService,
        IAccountService accountService,
        IUnitOfWork unitOfWork)
    {
        _paymentMethodFactory = paymentMethodFactory;
        _orderService = orderService;
        _cartService = cartService;
        _accountService = accountService;
        _unitOfWork = unitOfWork;
        _paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
        _orderRepository = _unitOfWork.GetRepository<IOrderRepository>();
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
            // Generate a temporary PaymentId that will be used for tracking
            var tempPaymentId = Guid.NewGuid();

            // Get cart for the user
            var cart = await _cartService.GetCartAsync(request.UserId, cancellationToken);

            if (cart.IsFailure)
            {
                return Result.Fail(cart.Errors);
            }

            if (cart.Value.Items.Count == 0)
            {
                return Result.Fail(OrderErrorFactory.CartEmpty());
            }

            decimal amount = cart.Value.TotalWithDiscount.Amount;

            // Simulate a delay in the payment process
            await Task.Delay(16000, cancellationToken);

            var paymentMethod = _paymentMethodFactory.GetPaymentMethod(GetPaymentMethod(request.PaymentMethod));

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

            // Get the new order ID
            var order = await _orderRepository.GetLatestOrderByUserIdAsync(request.UserId, cancellationToken);

            if (order is null)
            {
                return Result.Fail("Failed to retrieve the created order");
            }

            // Now create the payment record with the real order ID
            Result<PaymentRecord> paymentRecord = default!;

            if (request.PaymentMethod == PaymentMethodEnumDTO.CreditCard && request.CreditCardDetails is CreditCardDetailsDTO creditCardDetails)
            {
                paymentRecord = PaymentRecord.CreateCreditCardPayment(
                    order.Id,
                    new Money(request.Currency, amount),
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
                    new Money(request.Currency, amount),
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
            await _paymentRepository.CreateAsync(paymentRecord.Value, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(new PaymentResponseDTO(
                paymentRecord.Value.TransactionId,
                paymentRecord.Value.Amount.Amount,
                paymentRecord.Value.PaymentMethod.ToDTO(),
                paymentRecord.Value.ProcessedAt,
                paymentRecord.Value.Status.ToDTO(),
                paymentRecord.Value.GatewayResponse,
                paymentRecord.Value.GetCreditCardDetails()?.LastFourDigits,
                paymentRecord.Value.GetCreditCardDetails()?.CardBrand,
                paymentRecord.Value.GetPayPalDetails()?.PayPalEmail.Value,
                paymentRecord.Value.StatusHistory.Select(statusHistory => statusHistory.ToDTO()).ToList()
            ));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return Result.Fail($"Payment processing failed: {ex.Message}");
        }
    }

    public Task<Result<bool>> RefundPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> CancelPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private string GetPaymentMethod(PaymentMethodEnumDTO paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethodEnumDTO.CreditCard => "CreditCard",
            PaymentMethodEnumDTO.PayPal => "PayPal",
            _ => throw new ArgumentException("Invalid payment method")
        };
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